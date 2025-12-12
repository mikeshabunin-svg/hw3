using System;

namespace AntiPlagiarism.Analysis.Api.Contracts
{
    public sealed class ReportResponse
    {
        public Guid ReportId { get; init; }

        public Guid SubmissionId { get; init; }

        public string WorkId { get; init; }

        public string StudentId { get; init; }

        public Guid FileId { get; init; }

        public bool IsPlagiarism { get; init; }

        public double SimilarityPercent { get; init; }

        public Guid? BaseSubmissionId { get; init; }

        public DateTime CreatedAtUtc { get; init; }

        public DateTime SubmittedAtUtc { get; init; }

        public ReportResponse(
            Guid reportId,
            Guid submissionId,
            string workId,
            string studentId,
            Guid fileId,
            bool isPlagiarism,
            double similarityPercent,
            Guid? baseSubmissionId,
            DateTime createdAtUtc,
            DateTime submittedAtUtc)
        {
            ReportId = reportId;
            SubmissionId = submissionId;
            WorkId = workId;
            StudentId = studentId;
            FileId = fileId;
            IsPlagiarism = isPlagiarism;
            SimilarityPercent = similarityPercent;
            BaseSubmissionId = baseSubmissionId;
            CreatedAtUtc = createdAtUtc;
            SubmittedAtUtc = submittedAtUtc;
        }
    }
}
