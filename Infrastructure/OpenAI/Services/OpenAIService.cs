using Infrastructure.OpenAI.Interfaces;
using Infrastructure.OpenAI.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class OpenAiService : IOpenAIService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<OpenAiApiResponse?> SendRequestAsync(RequestPayload payload, string endpointUrl, string bearerToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(5);

        // Set Authorization header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        // Serialize payload to JSON
        request.Content = JsonContent.Create(payload);

        // Send the request
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorBody}");
        }

        // Deserialize response to OpenAiApiResponse
        var result = await response.Content.ReadFromJsonAsync<OpenAiApiResponse>();
        return result;
    }
}