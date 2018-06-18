using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobUtility
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAzureBlobStorage
    {
        Task UploadAsync(string blobName, string filePath, string containername);
        Task UploadAsync(string blobName, Stream stream, string containerName);
        Task<MemoryStream> DownloadAsyncByContainer(string blobName, string containerName);
        Task<MemoryStream> DownloadAsync(string blobName);
        Task DownloadAsync(string blobName, string path);
        Task DeleteAsync(string blobName);
        Task DeleteAsync(string blobName, string containerName);
        Task<bool> ExistsAsync(string blobName);
        Task<List<AzureBlobItem>> ListAsync();
        Task<List<AzureBlobItem>> ListAsync(string rootFolder);
        Task<List<string>> ListFoldersAsync();
        Task<List<string>> ListFoldersAsync(string rootFolder);

        /// <summary>
        /// UploadFileAsync
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task UploadFileAsync(string shareName, string sourceFolder, string fileName, Stream stream);

        /// <summary>
        /// DownloadFileAsync
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="fileDownloadPath"></param>
        /// <returns></returns>
        Task DownloadFileAsync(string shareName, string sourceFolder, string fileName, string fileDownloadPath);

        /// <summary>
        /// DownloadFileAsyncStream
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<MemoryStream> DownloadFileAsyncStream(string shareName, string sourceFolder, string fileName);



        /// <summary>
        /// Return the url once uploaded on azure
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="stream"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task<string> GetUrlAfterUpload(string blobName, Stream stream, string containerName);

        /// <summary>
        /// GetUrlByBlobAndContainerName
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task<string> GetUrlByBlobAndContainerName(string blobName, string containerName);

        /// <summary>
        /// GetUrlByBlobContainerWithSomePeriod
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <param name="startTime"></param>
        /// <param name="expiryTime"></param>
        /// <returns></returns>
        Task<string> GetUrlByBlobContainerWithSomePeriod(string blobName, string containerName, DateTimeOffset startTime, DateTimeOffset expiryTime);

        /// <summary>
        /// DeleteFileAsync
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task DeleteFileAsync(string shareName, string sourceFolder, string fileName);
    }
}
