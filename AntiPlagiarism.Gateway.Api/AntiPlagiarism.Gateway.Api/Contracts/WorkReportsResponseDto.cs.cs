using System.Collections.Generic;

namespace AntiPlagiarism.Gateway.Api.Contracts
{
    public sealed class WorkReportsResponseDto
    {
        public string WorkId { get; init; }

        public IReadOnlyCollection<ReportResponseDto> Reports { get; init; }

        public WorkReportsResponseDto(string workId, IReadOnlyCollection<ReportResponseDto> reports)
        {
            WorkId = workId;
            Reports = reports;
        }
    }
}
