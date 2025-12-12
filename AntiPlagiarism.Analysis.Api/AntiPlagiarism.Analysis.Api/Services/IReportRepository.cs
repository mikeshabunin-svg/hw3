using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Analysis.Api.Models;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public interface IReportRepository
    {
        Task AddAsync(Report report, CancellationToken cancellationToken = default);

        Task<Report?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Report>> GetByWorkIdAsync(
            string workId,
            ISubmissionRepository submissionRepository,
            CancellationToken cancellationToken = default);
    }
}
