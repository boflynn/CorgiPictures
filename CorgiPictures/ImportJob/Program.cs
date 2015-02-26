using System;
using System.Configuration;
using CorgiPictures.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CorgiPictures.ImportJob
{
    class Program
    {
        internal static CorgiPicturesContext db;
        internal static long lastUtc;
        internal static CloudBlobContainer container;

        static void Main(string[] args)
        {
            Initialize();

            Functions.DoStuff().Wait();

            //var host = new JobHost();
            //host.RunAndBlock();
        }

        private static void Initialize()
        {
            lastUtc = GetEpochTime();
            db = new CorgiPicturesContext();

            InitializeStorage();
        }

        internal static long GetEpochTime()
        {
            var date = DateTime.Now.AddHours(-4).ToUniversalTime();
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        internal static void InitializeStorage()
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["CorgiPicturesStorage"].ToString());

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            container = blobClient.GetContainerReference("images");
        }
    }
}
