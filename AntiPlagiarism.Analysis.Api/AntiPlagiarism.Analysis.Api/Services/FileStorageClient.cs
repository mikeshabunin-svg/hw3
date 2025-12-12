using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public sealed class FileStorageClient : IFileStorageClient
    {
        private readonly HttpClient _httpClient;

        public FileStorageClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            var baseUrl = configuration["FileStorage:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "http://localhost:5110";
            }

            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<byte[]> DownloadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"/files/{fileId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = response.StatusCode;
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Failed to download file {fileId} from File Storage. Status: {statusCode}. Details: {detail}");
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
