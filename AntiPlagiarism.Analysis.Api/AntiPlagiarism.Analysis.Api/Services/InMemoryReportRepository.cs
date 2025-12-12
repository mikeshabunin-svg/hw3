using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntiPlagiarism.Analysis.Api.Models;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public sealed class InMemoryReportRepository : IReportRepository
    {
        private readonly ConcurrentDictionary<Guid, Report> _reports = new();

        public Task AddAsync(Report report, CancellationToken cancellationToken = default)
        {
            _reports[report.Id] = report;
            return Task.CompletedTask;
        }

        public Task<Report?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _reports.TryGetValue(id, out var report);
            return Task.FromResult<Report?>(report);
        }

        public async Task<IReadOnlyCollection<Report>> GetByWorkIdAsync(
            string workId,
            ISubmissionRepository submissionRepository,
            CancellationToken cancellationToken = default)
        {
            var submissions = await submissionRepository.GetByWorkIdAsync(workId, cancellationToken);
            var submissionIds = submissions.Select(s => s.Id).ToHashSet();

            var result = _reports.Values
                .Where(r => submissionIds.Contains(r.SubmissionId))
                .OrderBy(r => r.CreatedAtUtc)
                .ToArray();

            return result;
        }
    }
}
