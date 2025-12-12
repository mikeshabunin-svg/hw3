using System;
using System.Linq;
using System.Threading;
using AntiPlagiarism.Analysis.Api.Contracts;
using AntiPlagiarism.Analysis.Api.Models;
using AntiPlagiarism.Analysis.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISubmissionRepository, InMemorySubmissionRepository>();
builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();
builder.Services.AddSingleton<IPlagiarismAnalyzer, SimplePlagiarismAnalyzer>();

builder.Services.AddHttpClient<IFileStorageClient, FileStorageClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// POST /submissions
app.MapPost("/submissions", async (
    CreateSubmissionRequest request,
    ISubmissionRepository submissionRepository,
    IReportRepository reportRepository,
    IFileStorageClient fileStorageClient,
    IPlagiarismAnalyzer analyzer,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.WorkId))
    {
        return Results.BadRequest("WorkId is required.");
    }

    if (string.IsNullOrWhiteSpace(request.StudentId))
    {
        return Results.BadRequest("StudentId is required.");
    }

    var submittedAt = request.SubmittedAtUtc ?? DateTime.UtcNow;

    var submission = new Submission(
        id: Guid.NewGuid(),
        workId: request.WorkId,
        studentId: request.StudentId,
        fileId: request.FileId,
        submittedAtUtc: submittedAt,
        status: SubmissionStatus.InProgress);

    await submissionRepository.AddAsync(submission, cancellationToken);

    byte[] fileBytes;
    try
    {
        fileBytes = await fileStorageClient.DownloadFileAsync(request.FileId, cancellationToken);
    }
    catch (Exception ex)
    {
        submission.Status = SubmissionStatus.Failed;
        return Results.Problem(
            title: "Failed to download file from File Storage.",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }

    var contentHash = SimplePlagiarismAnalyzer.ComputeSha256(fileBytes);
    submission.ContentHash = contentHash;

    var report = await analyzer.AnalyzeAsync(submission, contentHash, submissionRepository, cancellationToken);
    await reportRepository.AddAsync(report, cancellationToken);

    submission.Status = SubmissionStatus.Completed;

    var response = new SubmissionAnalysisResponse(
        submissionId: submission.Id,
        workId: submission.WorkId,
        studentId: submission.StudentId,
        fileId: submission.FileId,
        status: submission.Status.ToString(),
        reportId: report.Id,
        isPlagiarism: report.IsPlagiarism,
        similarityPercent: report.SimilarityPercent,
        baseSubmissionId: report.BaseSubmissionId,
        createdAtUtc: report.CreatedAtUtc);

    return Results.Ok(response);
})
.WithName("CreateSubmission")
.Produces<SubmissionAnalysisResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status502BadGateway);

// GET /reports/{reportId}
app.MapGet("/reports/{reportId:guid}", async (
    Guid reportId,
    IReportRepository reportRepository,
    ISubmissionRepository submissionRepository,
    CancellationToken cancellationToken) =>
{
    var report = await reportRepository.GetAsync(reportId, cancellationToken);
    if (report is null)
    {
        return Results.NotFound();
    }

    var submission = await submissionRepository.GetAsync(report.SubmissionId, cancellationToken);
    if (submission is null)
    {
        return Results.Problem(
            title: "Submission not found for report.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    var response = new ReportResponse(
        reportId: report.Id,
        submissionId: submission.Id,
        workId: submission.WorkId,
        studentId: submission.StudentId,
        fileId: submission.FileId,
        isPlagiarism: report.IsPlagiarism,
        similarityPercent: report.SimilarityPercent,
        baseSubmissionId: report.BaseSubmissionId,
        createdAtUtc: report.CreatedAtUtc,
        submittedAtUtc: submission.SubmittedAtUtc);

    return Results.Ok(response);
})
.WithName("GetReport")
.Produces<ReportResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// GET /works/{workId}/reports
app.MapGet("/works/{workId}/reports", async (
    string workId,
    IReportRepository reportRepository,
    ISubmissionRepository submissionRepository,
    CancellationToken cancellationToken) =>
{
    var reports = await reportRepository.GetByWorkIdAsync(workId, submissionRepository, cancellationToken);

    var submissionsById = (await submissionRepository.GetByWorkIdAsync(workId, cancellationToken))
        .ToDictionary(s => s.Id);

    var reportDtos = reports
        .Select(r =>
        {
            var submission = submissionsById[r.SubmissionId];
            return new ReportResponse(
                reportId: r.Id,
                submissionId: submission.Id,
                workId: submission.WorkId,
                studentId: submission.StudentId,
                fileId: submission.FileId,
                isPlagiarism: r.IsPlagiarism,
                similarityPercent: r.SimilarityPercent,
                baseSubmissionId: r.BaseSubmissionId,
                createdAtUtc: r.CreatedAtUtc,
                submittedAtUtc: submission.SubmittedAtUtc);
        })
        .ToArray();

    var response = new WorkReportsResponse(workId, reportDtos);

    return Results.Ok(response);
})
.WithName("GetWorkReports")
.Produces<WorkReportsResponse>(StatusCodes.Status200OK);

app.Run();
