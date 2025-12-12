using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Analysis.Api.Models;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public interface IPlagiarismAnalyzer
    {
        Task<Report> AnalyzeAsync(
            Submission submission,
            string contentHash,
            ISubmissionRepository submissionRepository,
            CancellationToken cancellationToken = default);
    }
}
