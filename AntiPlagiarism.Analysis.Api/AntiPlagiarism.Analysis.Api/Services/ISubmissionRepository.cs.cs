using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Analysis.Api.Models;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public interface ISubmissionRepository
    {
        Task AddAsync(Submission submission, CancellationToken cancellationToken = default);

        Task<Submission?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Submission>> GetByWorkIdAsync(
            string workId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Submission>> GetAllAsync(
            CancellationToken cancellationToken = default);
    }
}
