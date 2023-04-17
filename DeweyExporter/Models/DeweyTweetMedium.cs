namespace DeweyExporter.Models;

public class DeweyTweetMedium
{
        
    public string media_url { get; set; }
    public string link { get; set; }
    public string type { get; set; }
    public List<DeweyVideoSrc> video_src { get; set; }
}
