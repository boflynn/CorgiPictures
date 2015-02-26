using System;

namespace CorgiPictures.Model
{
    public class Picture
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime Created { get; set; }

        public string FileName { get; set; }

        public string PictureUrl { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
