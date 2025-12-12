using System;

namespace AntiPlagiarism.Gateway.Api.Contracts
{
    public sealed class UploadFileResponseDto
    {
        public Guid FileId { get; init; }

        public string FileName { get; init; }

        public long SizeBytes { get; init; }

        public string ContentType { get; init; }

        public string FileKind { get; init; }

        public DateTime UploadedAtUtc { get; init; }

        public UploadFileResponseDto(
            Guid fileId,
            string fileName,
            long sizeBytes,
            string contentType,
            string fileKind,
            DateTime uploadedAtUtc)
        {
            FileId = fileId;
            FileName = fileName;
            SizeBytes = sizeBytes;
            ContentType = contentType;
            FileKind = fileKind;
            UploadedAtUtc = uploadedAtUtc;
        }
    }
}
