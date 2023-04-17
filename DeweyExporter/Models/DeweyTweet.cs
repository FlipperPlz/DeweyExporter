namespace DeweyExporter.Models;

public class DeweyTweet
{
    public int id { get; set; }
    public int user { get; set; }
    public string tweet_date { get; set; }
    public string posted_by { get; set; }
    public string posted_by_profile_pic { get; set; }
    public string posted_by_profile_url { get; set; }
    public string tweet_content { get; set; }
    public string posted_by_nickname { get; set; }
    public string tweet_url { get; set; }
    public List<DeweyTweetMedium> tweet_media { get; set; }
    public object labels { get; set; }
    public string tweet_id { get; set; }
    public string conversation_id { get; set; }
    public int reply_count { get; set; }
    public int retweet_count { get; set; }
    public object sort_order { get; set; }
    public bool is_quoted_tweet { get; set; }
    public object quoted_tweet { get; set; }
    public bool verified { get; set; }
    public bool is_self_thread { get; set; }
    public bool is_archived { get; set; }
    public object notes { get; set; }
}