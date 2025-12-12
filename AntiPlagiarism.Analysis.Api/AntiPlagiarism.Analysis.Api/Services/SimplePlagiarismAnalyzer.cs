using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Analysis.Api.Models;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public sealed class SimplePlagiarismAnalyzer : IPlagiarismAnalyzer
    {
        public async Task<Report> AnalyzeAsync(
            Submission submission,
            string contentHash,
            ISubmissionRepository submissionRepository,
            CancellationToken cancellationToken = default)
        {
            var allSubmissions = await submissionRepository.GetByWorkIdAsync(
                submission.WorkId,
                cancellationToken);

            var baseSubmission = allSubmissions
                .Where(s => s.Id != submission.Id)
                .Where(s => s.StudentId != submission.StudentId)
                .Where(s => s.ContentHash == contentHash)
                .Where(s => s.SubmittedAtUtc < submission.SubmittedAtUtc)
                .OrderBy(s => s.SubmittedAtUtc)
                .FirstOrDefault();

            var isPlagiarism = baseSubmission is not null;
            var similarity = isPlagiarism ? 100.0 : 0.0;
            var baseSubmissionId = baseSubmission?.Id;

            var report = new Report(
                id: Guid.NewGuid(),
                submissionId: submission.Id,
                isPlagiarism: isPlagiarism,
                similarityPercent: similarity,
                baseSubmissionId: baseSubmissionId,
                createdAtUtc: DateTime.UtcNow);

            return report;
        }

        public static string ComputeSha256(byte[] content)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(content);
            return Convert.ToHexString(hashBytes);
        }
    }
}
