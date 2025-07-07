using AIFileAnalizator.Api.Dto;
using AIFileAnalizator.Api.Dto.Response;
using AIFileAnalizator.Api.Options;
using AIFileAnalizator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AIFileAnalizator.Api.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaService(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string?> AskAsync(string prompt)
    {
        var payload = new
        {
            model = _options.Model,
            prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync($"{_options.BaseUrl}/api/generate", payload);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return content?.Response;
    }
}
