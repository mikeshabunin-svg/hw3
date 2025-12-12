using System;

namespace AntiPlagiarism.Analysis.Api.Contracts
{
    public sealed class CreateSubmissionRequest
    {
        public string WorkId { get; init; }

        public string StudentId { get; init; }

        public Guid FileId { get; init; }

        public DateTime? SubmittedAtUtc { get; init; }

        public CreateSubmissionRequest(
            string workId,
            string studentId,
            Guid fileId,
            DateTime? submittedAtUtc = null)
        {
            WorkId = workId;
            StudentId = studentId;
            FileId = fileId;
            SubmittedAtUtc = submittedAtUtc;
        }
    }
}
