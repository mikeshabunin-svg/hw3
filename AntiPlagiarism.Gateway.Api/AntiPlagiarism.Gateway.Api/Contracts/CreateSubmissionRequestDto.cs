using System;

namespace AntiPlagiarism.Gateway.Api.Contracts
{
    public sealed class CreateSubmissionRequestDto
    {
        public string WorkId { get; init; }

        public string StudentId { get; init; }

        public Guid FileId { get; init; }

        public DateTime? SubmittedAtUtc { get; init; }

        public CreateSubmissionRequestDto(
            string workId,
            string studentId,
            Guid fileId,
            DateTime? submittedAtUtc)
        {
            WorkId = workId;
            StudentId = studentId;
            FileId = fileId;
            SubmittedAtUtc = submittedAtUtc;
        }
    }
}
