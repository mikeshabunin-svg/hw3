using System;
using System.Collections.Generic;

namespace AntiPlagiarism.Analysis.Api.Contracts
{
    public sealed class WorkReportsResponse
    {
        public string WorkId { get; init; }

        public IReadOnlyCollection<ReportResponse> Reports { get; init; }

        public WorkReportsResponse(string workId, IReadOnlyCollection<ReportResponse> reports)
        {
            WorkId = workId;
            Reports = reports;
        }
    }
}
