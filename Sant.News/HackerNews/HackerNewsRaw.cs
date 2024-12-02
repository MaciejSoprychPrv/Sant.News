using Newtonsoft.Json;


namespace Sant.News.HackerNews
{
    public class HackerNewsRaw
    {
        [JsonProperty("by")] public string By { get; set; }

        [JsonProperty("descendants")] public int Descendants { get; set; }

        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("kids")] public List<int> Kids { get; set; }

        [JsonProperty("score")] public int Score { get; set; }

        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("time")] public int Time { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("url")] public string Url { get; set; }
    }
}
