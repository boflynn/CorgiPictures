using System.Collections.Generic;
using Newtonsoft.Json;

namespace CorgiPictures.ImportJob
{
    public class RootObject
    {
        [JsonProperty("data")]
        public RootChild Data { get; set; }

        public class RootChild
        {
            [JsonProperty("children")]
            public List<Child> Children { get; set; }

            public class Child
            {
                [JsonProperty("data")]
                public Data2 Data { get; set; }

                public class Data2
                {
                    [JsonProperty("domain")]
                    public string Domain { get; set; }

                    [JsonProperty("id")]
                    public string Id { get; set; }

                    [JsonProperty("url")]
                    public string Url { get; set; }

                    [JsonProperty("title")]
                    public string Title { get; set; }

                    [JsonProperty("created_utc")]
                    public long Created { get; set; }
                }
            }
        }
    }
}
