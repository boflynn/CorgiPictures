﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CorgiPictures.Model;

using Newtonsoft.Json;

namespace CorgiPictures.ImportJob
{
    internal class Functions
    {
        internal static async Task DoStuff()
        {
            RootObject stuff;

            using (var httpClient = new HttpClient())
            {
                var url = @"http://www.reddit.com/r/corgi/new/.json";
                var json = await httpClient.GetStringAsync(url);

                stuff = JsonConvert.DeserializeObject<RootObject>(json);
            }

            foreach (var d in stuff.Data.Children.Where(d => d.Data.Domain.Contains("i.imgur.com") && d.Data.Created > Program.lastUtc))
            {
                var dd = d.Data;

                using (var httpClient = new HttpClient())
                {
                    var ext = dd.Url.Substring(dd.Url.LastIndexOf("."), dd.Url.Length - dd.Url.LastIndexOf("."));
                    var blobName = string.Format("{0}{1}", dd.Id, ext);
                    var thumbName = string.Format("{0}-thumb{1}", dd.Id, ext);

                    Console.WriteLine(blobName);

                    if (Program.db.Pictures.Any(p => p.FileName == blobName))
                    {
                        Console.WriteLine("Exists");
                        continue;
                    }

                    var content = await httpClient.GetByteArrayAsync(dd.Url);

                    var thumbnailContent = GenerateThumbnail(content);

                    var thumbnailBlob = Program.container.GetBlockBlobReference(thumbName);

                    thumbnailBlob.UploadFromByteArray(thumbnailContent, 0, thumbnailContent.Length);

                    var blockBlob = Program.container.GetBlockBlobReference(blobName);

                    blockBlob.UploadFromByteArray(content, 0, content.Length);

                    Program.db.Pictures.Add(new Picture
                    {
                        Created = FromUnixTime(dd.Created),
                        FileName = blobName,
                        Title = dd.Title.Length > 97 ? dd.Title.Substring(0, 97) + "..." : dd.Title,
                        PictureUrl = blockBlob.Uri.ToString(),
                        ThumbnailUrl = thumbnailBlob.Uri.ToString()
                    });
                }
            }

            Program.db.SaveChanges();
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
