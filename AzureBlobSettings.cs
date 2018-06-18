using System;

namespace AzureBlobUtility
{
    public class AzureBlobSettings
    {
        public AzureBlobSettings(string storageAccount,
                                    string storageKey,
                                    //string containerName,
                                    string connectionString)
        {
            if (string.IsNullOrEmpty(storageAccount))
                throw new ArgumentNullException("StorageAccount");

            if (string.IsNullOrEmpty(storageKey))
                throw new ArgumentNullException("StorageKey");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            //if (string.IsNullOrEmpty(containerName))
            //    throw new ArgumentNullException("ContainerName");

            this.StorageAccount = storageAccount;
            this.StorageKey = storageKey;
            //this.ContainerName = containerName;
            this.ConnectionString = connectionString;

        }

        public string StorageAccount { get; }
        public string StorageKey { get; }
        public string ContainerName { get; }
        public string ConnectionString { get; }
    }
}
