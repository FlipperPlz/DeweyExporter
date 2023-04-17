using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DeweyExporter.Models;
using Mono.Options;

namespace DeweyExporter;

public static class Program
{
    const string DeweyEndpoint = "https://getdewey.co/tweets/";

    public static async Task Main(string[] arguments)
    {
        string? cookiesString = null;
        string? authToken = null;
        var endpoint = new Uri(DeweyEndpoint);
        string? outputPath = null;
        var showHelp = false;
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0";
        var allTweets = new List<DeweyTweet>();

        var options = new OptionSet()
        {
            {
                "c|cookies=",
                "the {COOKIES} JSON file used to query DEWEY.",
                v => cookiesString = v
            },
            {
                "o|output=",
                "the output path to store collected tweets.",
                v => outputPath = v
            },
            {
                "auth|authtoken=",
                "the authorization token used to query DEWEY.",
                v =>
                {
                    if (v is not null) authToken = v;
                }
            },
            {
                "ep|endpoint=",
                $"the API Endpoint used to query DEWEY. (default is \"{DeweyEndpoint}\")",
                v =>
                {
                    if (v is not null) endpoint = new Uri(v);
                }
            },
            {
                "agent|useragent=",
                "the user-agent used to query DEWEY.",
                v =>
                {
                    if (v is not null) userAgent = v;
                }
            },
            {
                "h|help",
                "show this message and exit.",
                v => showHelp = v != null
            }
        };

        options.Parse(arguments);

        if (showHelp || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(cookiesString) || outputPath is null || endpoint is null)
        {
            options.WriteOptionDescriptions(Console.Out);
            return;
        }

        if (File.Exists(outputPath))
        {
            await Console.Out.WriteLineAsync("Output file already exists!");
            return;
        }

        try
        {
            using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Cookie", cookiesString);
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authToken);

            var currentPage = 1;
            var totalPages = -1;

            do
            {
                Console.WriteLine($"Querying endpoint {endpoint} for page {currentPage}/{totalPages}");
                var content = new StringContent
                (
                    JsonSerializer.Serialize
                    (
                        new 
                        {
                            all = true,
                            page_number = ++currentPage,
                            sort_direction = "-",
                            sort_field = "sort_order"
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    await Console.Out.WriteLineAsync("Attempting to backtrack (invalid response)");
                    currentPage--;
                    continue;
                }
                
                var modeledResponse = JsonSerializer.Deserialize<DeweyTweetQuery>(await response.Content.ReadAsStringAsync(), JsonSerializerOptions.Default);
                

                if (modeledResponse is null)
                {
                    await Console.Out.WriteLineAsync("Attempting to backtrack (failed to parse response)");
                    currentPage--;
                    continue;
                }
                
                if (totalPages == -1)
                {
                    if (int.TryParse(modeledResponse.total_pages, out totalPages))
                    {
                        await Console.Out.WriteLineAsync(
                            $"Failed to parser \"total_pages\" in first response. (got \"{modeledResponse.total_pages}\")");
                        
                        await Console.Out.WriteLineAsync("Attempting to backtrack");
                        currentPage--;
                        continue;
                    }
                }
                
                allTweets.AddRange(modeledResponse.tweets);
                await Console.Out.WriteLineAsync($"{modeledResponse.tweets.Count} were added to the current database.");
                await Console.Out.WriteLineAsync($"Tweets Gathered: {allTweets.Count}");
                await Console.Out.WriteLineAsync();
            } while (currentPage < totalPages);

            await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(allTweets, JsonSerializerOptions.Default));
        }
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(
                $"Failed to export all tweets. Exporting found...");
            await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(allTweets, JsonSerializerOptions.Default));

            Console.WriteLine(e);
            
            
            throw;
        }
        
    }

}