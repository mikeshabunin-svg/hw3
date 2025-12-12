using System;

namespace AntiPlagiarism.Analysis.Api.Models
{
    public sealed class Report
    {
        public Guid Id { get; init; }

        public Guid SubmissionId { get; init; }

        public bool IsPlagiarism { get; init; }

        public double SimilarityPercent { get; init; }

        public Guid? BaseSubmissionId { get; init; }

        public DateTime CreatedAtUtc { get; init; }

        public Report(
            Guid id,
            Guid submissionId,
            bool isPlagiarism,
            double similarityPercent,
            Guid? baseSubmissionId,
            DateTime createdAtUtc)
        {
            Id = id;
            SubmissionId = submissionId;
            IsPlagiarism = isPlagiarism;
            SimilarityPercent = similarityPercent;
            BaseSubmissionId = baseSubmissionId;
            CreatedAtUtc = createdAtUtc;
        }
    }
}
