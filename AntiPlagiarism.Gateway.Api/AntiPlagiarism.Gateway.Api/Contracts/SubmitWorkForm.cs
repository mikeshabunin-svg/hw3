using Microsoft.AspNetCore.Http;

namespace AntiPlagiarism.Gateway.Api.Contracts
{
    public sealed class SubmitWorkForm
    {
        public IFormFile File { get; set; } = default!;

        public string StudentId { get; set; } = string.Empty;

        public string? FileKind { get; set; }
    }
}
