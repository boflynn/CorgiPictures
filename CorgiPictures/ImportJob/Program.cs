using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CorgiPictures.Model;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using Newtonsoft.Json;

namespace CorgiPictures.ImportJob
{
    internal static class Program
    {
        private static CorgiPicturesContext db;
        private static long lastUtc;
        private static CloudBlobContainer container;

        private const string Url = @"http://www.reddit.com/r/corgi/new/.json";

        private static void Main()
        {
            Initialize();

            DoStuff().Wait();
        }

        private static void Initialize()
        {
            lastUtc = GetEpochTime();
            db = new CorgiPicturesContext();

            InitializeStorage();
        }

        private static long GetEpochTime()
        {
            var date = DateTime.Now.AddHours(-4).ToUniversalTime();
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        private static void InitializeStorage()
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["CorgiPicturesStorage"].ToString());

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            container = blobClient.GetContainerReference("images");
        }

        private static async Task DoStuff()
        {
            RootObject stuff;

            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(Url);

                stuff = JsonConvert.DeserializeObject<RootObject>(json);
            }

            foreach (var d in stuff.Data.Children.Where(d => d.Data.Domain.Contains("i.imgur.com") && d.Data.Created > lastUtc))
            {
                var dd = d.Data;

                using (var httpClient = new HttpClient())
                {
                    var ext = dd.Url.Substring(dd.Url.LastIndexOf(".", StringComparison.Ordinal),
                        dd.Url.Length - dd.Url.LastIndexOf(".", StringComparison.Ordinal));
                    var blobName = string.Format("{0}{1}", dd.Id, ext);
                    var thumbName = string.Format("{0}-thumb{1}", dd.Id, ext);

                    Console.WriteLine(blobName);

                    if (db.Pictures.Any(p => p.FileName == blobName))
                    {
                        Console.WriteLine("Exists");
                        continue;
                    }

                    var content = await httpClient.GetByteArrayAsync(dd.Url);

                    var thumbnailContent = GenerateThumbnail(content);

                    var thumbnailBlob = container.GetBlockBlobReference(thumbName);

                    thumbnailBlob.UploadFromByteArray(thumbnailContent, 0, thumbnailContent.Length);

                    var blockBlob = container.GetBlockBlobReference(blobName);

                    blockBlob.UploadFromByteArray(content, 0, content.Length);

                    db.Pictures.Add(new Picture
                                    {
                                        Created = FromUnixTime(dd.Created),
                                        FileName = blobName,
                                        Title = dd.Title.Length > 97 ? dd.Title.Substring(0, 97) + "..." : dd.Title,
                                        PictureUrl = blockBlob.Uri.ToString(),
                                        ThumbnailUrl = thumbnailBlob.Uri.ToString()
                                    });
                }
            }

            db.SaveChanges();
        }

        private static byte[] GenerateThumbnail(byte[] content)
        {
            var stream = new MemoryStream(content);
            var img = Image.FromStream(stream);

            var thumbNail = ScaleImage(img, 100, 100);

            var b = ImageToByteArray(thumbNail);
            return b;
        }

        private static byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }

        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}