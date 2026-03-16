using Google.GenAI;
using Google.GenAI.Types;
public class ChatService
{
    private const string ModelName = "gemini-2.0-flash";
    private readonly Client _client;

    public ChatService(string apiKey)
    {
        _client = new Client(apiKey: apiKey);
    }

    public async Task<string> GetChatResponse(string userInput)
    {
        var response = await _client.Models.GenerateContentAsync(
            model: ModelName,
            contents: userInput,
            config: new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts =
                    [
                        new Part { 
                            Text = """
                                    You are a helpful assistant.
                                    """ 
                        }
                    ]
                }
            }
        );

        if (response.Candidates is null || response.Candidates.Count == 0)
        {
            return string.Empty;
        }

        return response.Candidates[0].Content?.Parts?[0].Text ?? string.Empty;
    }

    public async Task<string> GeneralizeMovieDescriptions(string[] descriptions){
        var response = await _client.Models.GenerateContentAsync(
            model: ModelName,
            contents: string.Join("\n", descriptions),
            config: new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts =
                    [
                        new Part { 
                            Text = """
                                    You are a helpful assistant that generalizes movie descriptions.
                                    Take the following movie descriptions and generalize them into a single description that captures the essence of all the movies.
                                    Only provide the generalized description without any additional commentary.
                                    """ 
                        }
                    ]
                }
            }
        );

        if (response.Candidates is null || response.Candidates.Count == 0)
        {
            return string.Empty;
        }

        return response.Candidates[0].Content?.Parts?[0].Text ?? string.Empty;
    }
}
