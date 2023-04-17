namespace DeweyExporter.Models;

public class DeweyTweetQuery
{
    public List<DeweyTweet> tweets { get; set; }
    public string total_pages { get; set; }
    public int current_page { get; set; }
    public int total_number_of_tweets { get; set; }
    public object folder_id { get; set; }
    public int total_tweet_count { get; set; }

}