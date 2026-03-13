using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

public class EmbeddedService
{
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private const string ApiVersion = "2024-02-01";
    private const string ModelVersion = "2023-04-15";
    public EmbeddedService(string endpoint, string apiKey)
    {
        _endpoint = endpoint;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<float[]> VectorizeTextAsync(string text)
    {
        var url = $"{_endpoint}/computervision/retrieval:vectorizeText?api-version={ApiVersion}&model-version={ModelVersion}";

        var payload = JsonSerializer.Serialize(new { text });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await ParseVectorResponse(response);
    }
    private static async Task<float[]> ParseVectorResponse(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var vectorElement = doc.RootElement.GetProperty("vector");

        var vector = new float[vectorElement.GetArrayLength()];
        int i = 0;
        foreach (var element in vectorElement.EnumerateArray())
        {
        vector[i++] = element.GetSingle();
        }

        return vector;
    }
}