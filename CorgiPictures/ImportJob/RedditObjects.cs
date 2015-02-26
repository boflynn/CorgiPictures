using System.Collections.Generic;

namespace CorgiPictures.ImportJob
{
    public class Data2
    {
        public string domain { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public long created_utc { get; set; }
    }

    public class Child
    {
        public Data2 data { get; set; }
    }

    public class Data
    {
        public List<Child> children { get; set; }
    }

    public class RootObject
    {
        public Data data { get; set; }
    }
}
