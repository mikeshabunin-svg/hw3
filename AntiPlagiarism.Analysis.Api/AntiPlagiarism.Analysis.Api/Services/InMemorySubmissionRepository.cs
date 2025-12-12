using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Analysis.Api.Models;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public sealed class InMemorySubmissionRepository : ISubmissionRepository
    {
        private readonly ConcurrentDictionary<Guid, Submission> _submissions = new();

        public Task AddAsync(Submission submission, CancellationToken cancellationToken = default)
        {
            _submissions[submission.Id] = submission;
            return Task.CompletedTask;
        }

        public Task<Submission?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _submissions.TryGetValue(id, out var submission);
            return Task.FromResult<Submission?>(submission);
        }

        public Task<IReadOnlyCollection<Submission>> GetByWorkIdAsync(
            string workId,
            CancellationToken cancellationToken = default)
        {
            var result = _submissions.Values
                .Where(s => s.WorkId == workId)
                .OrderBy(s => s.SubmittedAtUtc)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<Submission>>(result);
        }

        public Task<IReadOnlyCollection<Submission>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var result = _submissions.Values
                .OrderBy(s => s.SubmittedAtUtc)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<Submission>>(result);
        }
    }
}
