using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HorizonsAI.Models;

namespace HorizonsAI.Services;

public record NarratorResult(
    string?        Narration,
    List<SceneNpc> Add,
    List<string>   Remove);

public class NarratorService
{
    private readonly HttpClient _http;

    public NarratorService(HttpClient http) => _http = http;

    public async Task<NarratorResult?> EvaluateAsync(
        IEnumerable<ChatMessage> history,
        IEnumerable<string> activeNpcNames)
    {
        var settings = AppConfig.Current;
        if (!settings.NarratorEnabled) return null;

        var apiKey = settings.OpenRouterApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        var model = string.IsNullOrWhiteSpace(settings.NarratorModel)
            ? settings.DefaultModel
            : settings.NarratorModel;

        var rosterLine = "Active scene participants: " + string.Join(", ", activeNpcNames);

        var historyText = string.Join("\n", history
            .TakeLast(15)
            .Select(m => m.IsSummary        ? $"[Earlier summary: {m.Text}]"
                       : m.IsNarratorAction ? $"*{m.Text}*"
                       : string.IsNullOrEmpty(m.SenderName) ? m.Text
                       : $"{m.SenderName}: {m.Text}"));

        var apiMessages = new List<object>
        {
            new { role = "system", content = settings.NarratorSystemPrompt + "\n\n" + rosterLine },
            new { role = "user",   content = "Recent scene:\n" + historyText + "\n\nEvaluate the scene now." },
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("HTTP-Referer", "https://github.com/Kailex01/horizons-ai-roleplay");
            request.Headers.Add("X-Title", "Horizon's AI");
            request.Content = JsonContent.Create(new { model, messages = apiMessages, max_tokens = 500 });

            var resp = await _http.SendAsync(request);
            if (!resp.IsSuccessStatusCode) return null;

            var result = await resp.Content.ReadFromJsonAsync<OAIResponse>();
            var json   = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrEmpty(json)) return null;

            return ParseResult(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> GenerateCharacterPromptAsync(
        string name, string personality, IEnumerable<ChatMessage> recentHistory)
    {
        var settings = AppConfig.Current;
        var apiKey   = settings.OpenRouterApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) return "";

        var model = string.IsNullOrWhiteSpace(settings.NarratorModel)
            ? settings.DefaultModel
            : settings.NarratorModel;

        var sceneContext = string.Join("\n", recentHistory
            .TakeLast(10)
            .Where(m => !m.IsSummary)
            .Select(m => m.IsNarratorAction
                ? $"*{m.Text}*"
                : string.IsNullOrEmpty(m.SenderName) ? m.Text : $"{m.SenderName}: {m.Text}"));

        var apiMessages = new List<object>
        {
            new { role = "system", content =
                "You create concise roleplay character system prompts. " +
                "Write as direct instructions to the LLM that will roleplay as this character (e.g. 'You are...'). " +
                "3-5 sentences covering personality, speech style, motivations, and how they fit the scene. " +
                "Stay consistent with the scene context and world tone. Output only the system prompt, no preamble." },
            new { role = "user", content =
                $"Name: {name}\n" +
                $"Personality sketch: {personality}\n\n" +
                $"Recent scene context:\n{sceneContext}\n\n" +
                "Write a system prompt for this character." },
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("HTTP-Referer", "https://github.com/Kailex01/horizons-ai-roleplay");
            request.Headers.Add("X-Title", "Horizon's AI");
            request.Content = JsonContent.Create(new { model, messages = apiMessages, max_tokens = 300 });

            var resp = await _http.SendAsync(request);
            if (!resp.IsSuccessStatusCode) return "";

            var result = await resp.Content.ReadFromJsonAsync<OAIResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
        }
        catch
        {
            return "";
        }
    }

    private static NarratorResult? ParseResult(string json)
    {
        try
        {
            // Strip markdown code fences some models add around JSON
            var stripped = Regex.Replace(json, @"^```(?:json)?\s*|\s*```$", "",
                RegexOptions.Multiline).Trim();

            using var doc  = JsonDocument.Parse(stripped);
            var       root = doc.RootElement;

            string? narration = null;
            if (root.TryGetProperty("narration", out var narEl)
                && narEl.ValueKind == JsonValueKind.String)
            {
                var s = narEl.GetString();
                if (!string.IsNullOrWhiteSpace(s)) narration = s;
            }

            var add = new List<SceneNpc>();
            if (root.TryGetProperty("add", out var addEl)
                && addEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in addEl.EnumerateArray())
                {
                    var name        = item.TryGetProperty("name",        out var n) ? n.GetString() ?? "" : "";
                    var personality = item.TryGetProperty("personality", out var p) ? p.GetString() ?? "" : "";
                    if (!string.IsNullOrWhiteSpace(name))
                        add.Add(new SceneNpc { Name = name, Personality = personality });
                }
            }

            var remove = new List<string>();
            if (root.TryGetProperty("remove", out var remEl)
                && remEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in remEl.EnumerateArray())
                {
                    var name = item.GetString();
                    if (!string.IsNullOrWhiteSpace(name)) remove.Add(name!);
                }
            }

            if (narration == null && add.Count == 0 && remove.Count == 0)
                return null;

            return new NarratorResult(narration, add, remove);
        }
        catch
        {
            return null;
        }
    }

    private record OAIResponse(
        [property: JsonPropertyName("choices")] List<OAIChoice>? Choices);
    private record OAIChoice(
        [property: JsonPropertyName("message")] OAIMessage? Message);
    private record OAIMessage(
        [property: JsonPropertyName("content")] string? Content);
}
