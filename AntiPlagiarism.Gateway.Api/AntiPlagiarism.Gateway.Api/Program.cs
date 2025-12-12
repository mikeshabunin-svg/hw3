using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using AntiPlagiarism.Gateway.Api.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient для FileStorage
builder.Services.AddHttpClient("FileStorage", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["Downstream:FileStorage:BaseUrl"] ?? "http://localhost:5110";
    client.BaseAddress = new Uri(baseUrl);
});

// HttpClient для Analysis
builder.Services.AddHttpClient("Analysis", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["Downstream:Analysis:BaseUrl"] ?? "http://localhost:5173";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===============================
// POST /api/works/{workId}/submit
// ===============================
app.MapPost("/api/works/{workId}/submit", async (
        string workId,
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
{
    var request = context.Request;

    // НИКАКИХ [FromForm] и IFormFile -> не будет антифорджери-метаданных
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data content type.");
    }

    var form = await request.ReadFormAsync(cancellationToken);

    var file = form.Files.GetFile("file");
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Form field 'file' is required.");
    }

    var studentId = form["studentId"].ToString();
    if (string.IsNullOrWhiteSpace(studentId))
    {
        return Results.BadRequest("Form field 'studentId' is required.");
    }

    var fileKind = form["fileKind"].ToString();
    if (string.IsNullOrWhiteSpace(fileKind))
    {
        fileKind = "Work";
    }

    // 1) Отправляем файл в FileStorage
    var fileStorageClient = httpClientFactory.CreateClient("FileStorage");

    using var multipart = new MultipartFormDataContent();

    await using var fileStream = file.OpenReadStream();
    var fileContent = new StreamContent(fileStream);
    fileContent.Headers.ContentType = new MediaTypeHeaderValue(
        string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType);

    multipart.Add(fileContent, "file", file.FileName);
    multipart.Add(new StringContent(fileKind), "fileKind");

    using var uploadResponse = await fileStorageClient.PostAsync("/files", multipart, cancellationToken);
    if (!uploadResponse.IsSuccessStatusCode)
    {
        var errorText = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
        return Results.Problem(
            title: "Failed to upload file to File Storage.",
            detail: errorText,
            statusCode: StatusCodes.Status502BadGateway);
    }

    var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadFileResponseDto>(
        cancellationToken: cancellationToken);
    if (uploadResult is null)
    {
        return Results.Problem(
            title: "Failed to parse File Storage response.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    // 2) Создаём submission в Analysis
    var analysisClient = httpClientFactory.CreateClient("Analysis");

    var submissionRequest = new CreateSubmissionRequestDto(
        workId: workId,
        studentId: studentId,
        fileId: uploadResult.FileId,
        submittedAtUtc: null);

    using var submissionResponse = await analysisClient.PostAsJsonAsync(
        "/submissions",
        submissionRequest,
        cancellationToken);

    if (!submissionResponse.IsSuccessStatusCode)
    {
        var errorText = await submissionResponse.Content.ReadAsStringAsync(cancellationToken);
        return Results.Problem(
            title: "Failed to create submission in Analysis service.",
            detail: errorText,
            statusCode: StatusCodes.Status502BadGateway);
    }

    var submissionResult = await submissionResponse.Content.ReadFromJsonAsync<SubmissionAnalysisResponseDto>(
        cancellationToken: cancellationToken);

    if (submissionResult is null)
    {
        return Results.Problem(
            title: "Failed to parse Analysis response.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Ok(submissionResult);
})
    .WithName("SubmitWork")
    .Produces<SubmissionAnalysisResponseDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status502BadGateway);

// ========================
// GET /api/reports/{id}
// ========================
app.MapGet("/api/reports/{reportId:guid}", async (
        Guid reportId,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
{
    var analysisClient = httpClientFactory.CreateClient("Analysis");

    var response = await analysisClient.GetAsync($"/reports/{reportId}", cancellationToken);
    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }

    if (!response.IsSuccessStatusCode)
    {
        var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
        return Results.Problem(
            title: "Failed to get report from Analysis service.",
            detail: errorText,
            statusCode: StatusCodes.Status502BadGateway);
    }

    var report = await response.Content.ReadFromJsonAsync<ReportResponseDto>(
        cancellationToken: cancellationToken);
    if (report is null)
    {
        return Results.Problem(
            title: "Failed to parse report from Analysis service.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Ok(report);
})
    .WithName("GetReportViaGateway")
    .Produces<ReportResponseDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status502BadGateway);

// ==================================
// GET /api/works/{workId}/reports
// ==================================
app.MapGet("/api/works/{workId}/reports", async (
        string workId,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
{
    var analysisClient = httpClientFactory.CreateClient("Analysis");

    var response = await analysisClient.GetAsync($"/works/{workId}/reports", cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
        return Results.Problem(
            title: "Failed to get work reports from Analysis service.",
            detail: errorText,
            statusCode: StatusCodes.Status502BadGateway);
    }

    var result = await response.Content.ReadFromJsonAsync<WorkReportsResponseDto>(
        cancellationToken: cancellationToken);
    if (result is null)
    {
        return Results.Problem(
            title: "Failed to parse work reports from Analysis service.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Ok(result);
})
    .WithName("GetWorkReportsViaGateway")
    .Produces<WorkReportsResponseDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status502BadGateway);

app.Run();
