using System;

namespace AntiPlagiarism.Analysis.Api.Models
{
    public sealed class Submission
    {
        public Guid Id { get; init; }

        public string WorkId { get; init; }

        public string StudentId { get; init; }

        public Guid FileId { get; init; }

        public DateTime SubmittedAtUtc { get; init; }

        public SubmissionStatus Status { get; set; }

        public string? ContentHash { get; set; }

        public Submission(
            Guid id,
            string workId,
            string studentId,
            Guid fileId,
            DateTime submittedAtUtc,
            SubmissionStatus status)
        {
            Id = id;
            WorkId = workId;
            StudentId = studentId;
            FileId = fileId;
            SubmittedAtUtc = submittedAtUtc;
            Status = status;
        }
    }
}
