using System.Net.Http.Headers;
using HorizonsAI.Models;

namespace HorizonsAI.Services;

public class OpenRouterService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://openrouter.ai/api/v1";

    public OpenRouterService(HttpClient http) => _http = http;

    public async Task<List<string>> ChatAsync(Character character, IEnumerable<ChatMessage> history, string userMessage)
    {
        var apiKey = AppConfig.Current.OpenRouterApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            return ["(No OpenRouter API key set — open Settings to add one.)"];

        var model    = string.IsNullOrWhiteSpace(character.Model) ? AppConfig.Current.DefaultModel : character.Model;
        var messages = BuildMessages(character, history, userMessage);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "https://github.com/Kailex01/horizons-ai-roleplay");
        request.Headers.Add("X-Title", "Horizon's AI");
        request.Content = JsonContent.Create(new { model, messages });

        var resp = await _http.SendAsync(request);
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<OAIResponse>();
        var text   = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";

        if (string.IsNullOrEmpty(text)) return ["…"];

        // Split on blank lines — each paragraph becomes its own bubble
        return text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => s.Trim())
                   .Where(s => !string.IsNullOrEmpty(s))
                   .ToList();
    }

    private static List<object> BuildMessages(Character character, IEnumerable<ChatMessage> history, string userMessage)
    {
        var msgs = new List<object>();

        if (!string.IsNullOrWhiteSpace(character.SystemPrompt))
            msgs.Add(new { role = "system", content = character.SystemPrompt });

        foreach (var msg in history)
            msgs.Add(new { role = msg.IsPlayer ? "user" : "assistant", content = msg.Text });

        msgs.Add(new { role = "user", content = userMessage });
        return msgs;
    }

    private record OAIResponse(
        [property: JsonPropertyName("choices")] List<OAIChoice>? Choices);
    private record OAIChoice(
        [property: JsonPropertyName("message")] OAIMessage? Message);
    private record OAIMessage(
        [property: JsonPropertyName("content")] string? Content);
}
