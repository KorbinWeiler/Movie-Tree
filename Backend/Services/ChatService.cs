using OpenAI;
using OpenAI.Chat;

public class ChatService
{
    private readonly ChatClient _client;

    public ChatService(string apiKey)
    {
        _client = new OpenAIClient(apiKey).GetChatClient("gpt-5-mini-2025-08-07");
    }

    public async Task<string> GetChatResponse(string userInput)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage(userInput)
        };

        ChatCompletion completion = await _client.CompleteChatAsync(messages);
        return completion.Content[0].Text;
    }
}