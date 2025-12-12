using System;
using System.Threading;
using System.Threading.Tasks;

namespace AntiPlagiarism.Analysis.Api.Services
{
    public interface IFileStorageClient
    {
        Task<byte[]> DownloadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}
