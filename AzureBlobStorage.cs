using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobUtility
{
    /// <summary>
    /// AzureBlobStorge clase where we implemented all needed methods which we used for blob storage.
    /// </summary>
    public class AzureBlobStorage : IAzureBlobStorage
    {
        #region Public Methods

        /// <summary>
        /// Constructor where we pass azure blob settings
        /// </summary>
        /// <param name="settings"></param>
        public AzureBlobStorage(AzureBlobSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Upload async method with blob name and image path 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task UploadAsync(string blobName, string filePath)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Upload
            using (var fileStream = System.IO.File.Open(filePath, FileMode.Open))
            {
                fileStream.Position = 0;
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }

        public async Task UploadAsync(string blobName, string filePath, string containerName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(string.Concat(blobName, Path.GetExtension(filePath)), containerName);

            //Upload
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                fileStream.Position = 0;
                blockBlob.Properties.ContentType = GetFileContentTypes(Path.GetExtension(filePath));
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }


        /// <summary>
        /// Upload async method whics used Blobname, image stream and container name 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="stream"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task UploadAsync(string blobName, Stream stream, string containerName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName, containerName);

            //Upload
            stream.Position = 0;
            await blockBlob.UploadFromStreamAsync(stream);
        }


        /// <summary>
        /// Download method from blob name only 
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<MemoryStream> DownloadAsync(string blobName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Download
            using (var stream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(stream);
                return stream;
            }
        }

        /// <summary>
        /// Download image/video from blob by using name of container 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task<MemoryStream> DownloadAsyncByContainer(string blobName, string containerName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName, containerName);

            //Download
            using (var stream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(stream);
                return stream;
            }
        }


        /// <summary>
        /// Download async by blob name and path only 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task DownloadAsync(string blobName, string path)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Download
            await blockBlob.DownloadToFileAsync(path, FileMode.Create);
        }

        /// <summary>
        /// Delete method of blob by blobname 
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string blobName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Delete
            await blockBlob.DeleteAsync();
        }

        /// <summary>
        /// Delete async method by blobname(image/video name) and continer name 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string blobName, string containerName)
        {
            //get storage acount
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);
            //Get blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //Get Container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            //Get blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
            //Delete Blob
            await blockBlob.DeleteAsync();
        }

        /// <summary>
        /// Is image exist or not ExistAsync only 
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string blobName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Exists
            return await blockBlob.ExistsAsync();
        }

        /// <summary>
        /// Listing of imaged from blob 
        /// </summary>
        /// <returns></returns>
        public async Task<List<AzureBlobItem>> ListAsync()
        {
            return await GetBlobListAsync();
        }

        /// <summary>
        /// List async method
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        public async Task<List<AzureBlobItem>> ListAsync(string rootFolder)
        {
            if (rootFolder == "*") return await ListAsync(); //All Blobs
            if (rootFolder == "/") rootFolder = "";          //Root Blobs

            var list = await GetBlobListAsync();
            return list.Where(i => i.Folder == rootFolder).ToList();
        }

        /// <summary>
        /// List folder async 
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> ListFoldersAsync()
        {
            var list = await GetBlobListAsync();
            return list.Where(i => !string.IsNullOrEmpty(i.Folder))
                       .Select(i => i.Folder)
                       .Distinct()
                       .OrderBy(i => i)
                       .ToList();
        }

        /// <summary>
        /// List folder async by root path
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        public async Task<List<string>> ListFoldersAsync(string rootFolder)
        {
            if (rootFolder == "*" || rootFolder == "") return await ListFoldersAsync(); //All Folders

            var list = await GetBlobListAsync();
            return list.Where(i => i.Folder.StartsWith(rootFolder))
                       .Select(i => i.Folder)
                       .Distinct()
                       .OrderBy(i => i)
                       .ToList();
        }

        /// <summary>
        /// DeleteFileAsync
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string shareName, string sourceFolder, string fileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);
            CloudFileClient cloudFileClient = storageAccount.CreateCloudFileClient();
            CloudFileShare cloudFileShare = null;
            cloudFileShare = cloudFileClient.GetShareReference(shareName);
            CloudFileDirectory rootDirectory = cloudFileShare.GetRootDirectoryReference();
            CloudFileDirectory fileDirectory = null;
            CloudFile cloudFile = null;
            fileDirectory = rootDirectory.GetDirectoryReference(sourceFolder);
            cloudFile = fileDirectory.GetFileReference(fileName);
            await cloudFile.DeleteAsync();
        }

        /// <summary>
        /// Save document file to Azure File
        /// </summary>
        /// <param name="shareName">Define share name</param>
        /// <param name="sourceFolder">Define folder name which created under share</param>
        /// <param name="fileName">Define file name which resided under sourcefolder </param>
        /// <param name="stream">File stream</param>
        /// <returns></returns>
        public async Task UploadFileAsync(string shareName, string sourceFolder, string fileName, Stream stream)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);

            // Create a CloudFileClient object for credentialed access to Azure Files.
            CloudFileClient cloudFileClient = storageAccount.CreateCloudFileClient();

            CloudFileShare cloudFileShare = null;
            cloudFileShare = cloudFileClient.GetShareReference(shareName);

            await cloudFileShare.CreateIfNotExistsAsync();

            // First, get a reference to the root directory, because that's where you're going to put the new directory.
            CloudFileDirectory rootDirectory = cloudFileShare.GetRootDirectoryReference();

            CloudFileDirectory fileDirectory = null;
            CloudFile cloudFile = null;

            // Set a reference to the file directory.
            // If the source folder is null, then use the root folder.
            // If the source folder is specified, then get a reference to it.
            if (string.IsNullOrWhiteSpace(sourceFolder))
            {
                // There is no folder specified, so return a reference to the root directory.
                fileDirectory = rootDirectory;
            }
            else
            {
                // There was a folder specified, so return a reference to that folder.
                fileDirectory = rootDirectory.GetDirectoryReference(sourceFolder);
                await fileDirectory.CreateIfNotExistsAsync();
            }

            // Set a reference to the file.
            cloudFile = fileDirectory.GetFileReference(fileName);

            await cloudFile.UploadFromStreamAsync(stream);
        }

        /// <summary>
        /// DownloadFileAsync which using shareNname, sourcefoldername, file namd and filedownload path 
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="fileDownloadPath"></param>
        /// <returns></returns>
        public async Task DownloadFileAsync(string shareName, string sourceFolder, string fileName, string fileDownloadPath)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);

            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            CloudFileShare fileShare = fileClient.GetShareReference(shareName);
            if (await fileShare.ExistsAsync())
            {
                CloudFileDirectory rootDirectory = fileShare.GetRootDirectoryReference();
                if (await rootDirectory.ExistsAsync())
                {
                    CloudFileDirectory customDirectory = rootDirectory.GetDirectoryReference(sourceFolder);
                    if (await customDirectory.ExistsAsync())
                    {
                        CloudFile cloudfile = customDirectory.GetFileReference(fileName);
                        if (await cloudfile.ExistsAsync())
                        {
                            await cloudfile.DownloadToFileAsync(fileDownloadPath, System.IO.FileMode.OpenOrCreate);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// DownloadFileAsyncStream
        /// </summary>
        /// <param name="shareName"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<MemoryStream> DownloadFileAsyncStream(string shareName, string sourceFolder, string fileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);

            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            CloudFileShare fileShare = fileClient.GetShareReference(shareName);
            if (await fileShare.ExistsAsync())
            {
                CloudFileDirectory rootDirectory = fileShare.GetRootDirectoryReference();
                if (await rootDirectory.ExistsAsync())
                {
                    CloudFileDirectory customDirectory = rootDirectory.GetDirectoryReference(sourceFolder);
                    if (await customDirectory.ExistsAsync())
                    {
                        CloudFile cloudfile = customDirectory.GetFileReference(fileName);
                        if (await cloudfile.ExistsAsync())
                        {
                            using (var stream = new MemoryStream())
                            {
                                await cloudfile.DownloadToStreamAsync(stream);
                                return stream;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Return the url once uploaded on azure
        /// Image is displayed by ULR with some time span, it will return url also once image uploaded. 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="stream"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task<string> GetUrlAfterUpload(string blobName, Stream stream, string containerName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName, containerName);

            //Upload
            stream.Position = 0;

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(10);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            string sasBlobToken = blockBlob.GetSharedAccessSignature(sasConstraints);
            await blockBlob.UploadFromStreamAsync(stream);
            var url = blockBlob?.Uri.ToString();
            url = url + sasBlobToken;
            return url;
        }

        /// <summary>
        /// GetUrlByBlobAndContainerName with some time 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task<string> GetUrlByBlobAndContainerName(string blobName, string containerName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName, containerName);

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(10);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            string sasBlobToken = blockBlob.GetSharedAccessSignature(sasConstraints);
            var url = blockBlob?.Uri.ToString();
            url = url + sasBlobToken;
            return url;
        }

        /// <summary>
        /// GetUrlByBlobContainerWithSomePeriod
        /// This method is configurable of time period. For how much time you want to displayed your image/video on public domain. 
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <param name="startTime"></param>
        /// <param name="expiryTime"></param>
        /// <returns></returns>
        public async Task<string> GetUrlByBlobContainerWithSomePeriod(string blobName, string containerName, DateTimeOffset startTime, DateTimeOffset expiryTime)
        {
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName, containerName);
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = startTime;
            sasConstraints.SharedAccessExpiryTime = expiryTime;
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            string sasBlobToken = blockBlob.GetSharedAccessSignature(sasConstraints);
            var url = blockBlob?.Uri.ToString();
            url = url + sasBlobToken;
            return url;
        }

        #endregion

        #region Private Methods

        private readonly AzureBlobSettings settings;

        /// <summary>
        /// Getting container name
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private async Task<CloudBlobContainer> GetContainerAsync(string containerName)
        {
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(settings.StorageAccount, settings.StorageKey), false);
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);

            //Client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();
            //await blobContainer.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });

            return blobContainer;
        }

        /// <summary>
        /// Get Block blob
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private async Task<CloudBlockBlob> GetBlockBlobAsync(string blobName, string containerName = null)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync(containerName);

            //Blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);

            return blockBlob;
        }

        private async Task<CloudBlockBlob> GetBlockBlobAsync(string blobName)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync();

            //Blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);

            return blockBlob;
        }

        private async Task<CloudBlobContainer> GetContainerAsync()
        {
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(settings.StorageAccount, settings.StorageKey), false);

            //Client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(settings.StorageAccount);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }

        /// <summary>
        /// Get block blol 
        /// </summary>
        /// <param name="useFlatListing"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private async Task<List<AzureBlobItem>> GetBlobListAsync(bool useFlatListing = true, string containerName = null)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync(containerName);

            //List
            var list = new List<AzureBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment =
                    await blobContainer.ListBlobsSegmentedAsync("", useFlatListing, new BlobListingDetails(), null, token, null, null);
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    list.Add(new AzureBlobItem(item));
                }
            } while (token != null);

            return list.OrderBy(i => i.Folder).ThenBy(i => i.Name).ToList();
        }

        /// <summary>
        /// Method to get content type based on extension
        /// </summary>
        /// <param name="extenstion"></param>
        /// <returns>content type string</returns>
        private string GetFileContentTypes(string extension)
        {
            string ContentType = string.Empty;
            string Extension = extension.ToLower();

            switch (Extension)
            {
                case "pdf":
                    ContentType = "application/pdf";
                    break;
                case "txt":
                    ContentType = "text/plain";
                    break;
                case "bmp":
                    ContentType = "image/bmp";
                    break;
                case "gif":
                    ContentType = "image/gif";
                    break;
                case "png":
                    ContentType = "image/png";
                    break;
                case "jpg":
                    ContentType = "image/jpeg";
                    break;
                case "jpeg":
                    ContentType = "image/jpeg";
                    break;
                case "xls":
                    ContentType = "application/vnd.ms-excel";
                    break;
                case "xml":
                    ContentType = "text/xml";
                    break;
                default:
                    ContentType = "application/octet-stream";
                    break;

            }
            return ContentType;
        }

        #endregion
    }
}
