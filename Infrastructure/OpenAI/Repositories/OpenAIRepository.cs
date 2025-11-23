using Infrastructure.OpenAI.Interfaces;
using Infrastructure.OpenAI.Models;
using Infrastructure.OpenAI.Settings;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

public class OpenAIRepository : IOpenAIRepository
{
    private readonly IOpenAIService _openAIService;
    private readonly OpenAiSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenAIRepository(IOpenAIService openAIService, IOptions<OpenAiSettings> settings)
    {
        _openAIService = openAIService;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetDataAsync<T>(RequestPayload payload)
    {
        var response = await _openAIService.SendRequestAsync(payload, _settings.Endpoint, _settings.Auth);
        var text = response?.Output?.Where(o => o.Content != null && o.Content.Any(c => c.Text?.Length > 0)).FirstOrDefault()?.Content?.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(text))
            return default;

        return JsonSerializer.Deserialize<T>(text, _jsonOptions);
    }
}