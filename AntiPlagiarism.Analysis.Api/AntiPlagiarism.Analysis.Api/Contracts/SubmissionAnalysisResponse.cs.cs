using System;

namespace AntiPlagiarism.Analysis.Api.Contracts
{
    public sealed class SubmissionAnalysisResponse
    {
        public Guid SubmissionId { get; init; }

        public string WorkId { get; init; }

        public string StudentId { get; init; }

        public Guid FileId { get; init; }

        public string Status { get; init; }

        public Guid ReportId { get; init; }

        public bool IsPlagiarism { get; init; }

        public double SimilarityPercent { get; init; }

        public Guid? BaseSubmissionId { get; init; }

        public DateTime CreatedAtUtc { get; init; }

        public SubmissionAnalysisResponse(
            Guid submissionId,
            string workId,
            string studentId,
            Guid fileId,
            string status,
            Guid reportId,
            bool isPlagiarism,
            double similarityPercent,
            Guid? baseSubmissionId,
            DateTime createdAtUtc)
        {
            SubmissionId = submissionId;
            WorkId = workId;
            StudentId = studentId;
            FileId = fileId;
            Status = status;
            ReportId = reportId;
            IsPlagiarism = isPlagiarism;
            SimilarityPercent = similarityPercent;
            BaseSubmissionId = baseSubmissionId;
            CreatedAtUtc = createdAtUtc;
        }
    }
}
