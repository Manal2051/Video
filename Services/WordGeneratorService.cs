using LanguageVideoGenerator.Api.Configuration;
using LanguageVideoGenerator.Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LanguageVideoGenerator.Api.Services;

/// <summary>
/// Interface for word generation service
/// </summary>
public interface IWordGeneratorService
{
    /// <summary>
    /// Generates word pairs based on topic and languages
    /// </summary>
    Task<List<WordPair>> GenerateWordPairsAsync(
        string topic,
        int count,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for generating words using OpenAI
/// </summary>
public class WordGeneratorService : IWordGeneratorService
{
    private readonly OpenAISettings _settings;
    private readonly ILogger<WordGeneratorService> _logger;

    public WordGeneratorService(
        IOptions<OpenAISettings> settings,
        ILogger<WordGeneratorService> logger)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<List<WordPair>> GenerateWordPairsAsync(
        string topic,
        int count,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "[OpenAI] [{RequestId}] Starting word generation: {Count} pairs for topic '{Topic}' ({SourceLang} -> {TargetLang})",
                requestId, count, topic, sourceLanguage, targetLanguage);

            var prompt = BuildPrompt(topic, count, sourceLanguage, targetLanguage);
            var requestUrl = "https://api.openai.com/v1/chat/completions";

            // Use HttpClient to call OpenAI API directly
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful language learning assistant that generates vocabulary words with translations." },
                    new { role = "user", content = prompt }
                },
                max_tokens = _settings.MaxTokens,
                temperature = 0.7
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonContent = JsonSerializer.Serialize(requestBody, jsonOptions);

            // Log complete request details
            _logger.LogInformation(
                "[OpenAI] [{RequestId}] ========== OUTGOING REQUEST ==========\n" +
                "Method: POST\n" +
                "URL: {Url}\n" +
                "Headers:\n" +
                "  Authorization: Bearer {ApiKey}\n" +
                "  Content-Type: application/json\n" +
                "Body:\n{RequestBody}",
                requestId,
                requestUrl,
                MaskApiKey(_settings.ApiKey),
                jsonContent);

            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(requestUrl, httpContent, cancellationToken);

            stopwatch.Stop();
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = FormatHeaders(response.Headers, response.Content.Headers);

            // Log complete response details
            _logger.LogInformation(
                "[OpenAI] [{RequestId}] ========== INCOMING RESPONSE ==========\n" +
                "Status: {StatusCode} ({StatusCodeInt})\n" +
                "Duration: {Duration}ms\n" +
                "Headers:\n{ResponseHeaders}\n" +
                "Body:\n{ResponseBody}",
                requestId,
                response.StatusCode,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseHeaders,
                responseJson);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[OpenAI] [{RequestId}] Request failed with status {StatusCode}. Response: {ResponseBody}",
                    requestId,
                    response.StatusCode,
                    responseJson);
            }

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            // Log usage information if available
            if (doc.RootElement.TryGetProperty("usage", out var usage))
            {
                _logger.LogInformation(
                    "[OpenAI] [{RequestId}] Token usage - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
                    requestId,
                    usage.GetProperty("prompt_tokens").GetInt32(),
                    usage.GetProperty("completion_tokens").GetInt32(),
                    usage.GetProperty("total_tokens").GetInt32());
            }

            _logger.LogInformation(
                "[OpenAI] [{RequestId}] Extracted content from response:\n{Content}",
                requestId,
                content);

            var wordPairs = ParseWordPairs(content);

            if (wordPairs.Count < count)
            {
                _logger.LogWarning(
                    "[OpenAI] [{RequestId}] Generated only {ActualCount} word pairs instead of requested {RequestedCount}",
                    requestId,
                    wordPairs.Count, count);
            }

            _logger.LogInformation(
                "[OpenAI] [{RequestId}] Successfully generated {Count} word pairs in {Duration}ms",
                requestId,
                wordPairs.Count,
                stopwatch.ElapsedMilliseconds);

            return wordPairs;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[OpenAI] [{RequestId}] HTTP error after {Duration}ms. Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw new InvalidOperationException("Failed to generate word pairs - HTTP error", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[OpenAI] [{RequestId}] Request timed out after {Duration}ms",
                requestId,
                stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException("OpenAI API request timed out", ex);
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[OpenAI] [{RequestId}] JSON parsing error after {Duration}ms. Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw new InvalidOperationException("Failed to parse OpenAI response", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[OpenAI] [{RequestId}] Unexpected error after {Duration}ms. Type: {ExceptionType}, Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.GetType().Name,
                ex.Message);
            throw new InvalidOperationException("Failed to generate word pairs", ex);
        }
    }

    /// <summary>
    /// Masks the API key for logging (shows first 4 and last 4 characters)
    /// </summary>
    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 12)
            return "***";
        return $"{apiKey[..4]}...{apiKey[^4..]}";
    }

    /// <summary>
    /// Formats HTTP headers for logging
    /// </summary>
    private static string FormatHeaders(
        System.Net.Http.Headers.HttpResponseHeaders responseHeaders,
        System.Net.Http.Headers.HttpContentHeaders contentHeaders)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var header in responseHeaders)
        {
            sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }
        foreach (var header in contentHeaders)
        {
            sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }
        return sb.ToString().TrimEnd();
    }

    private string BuildPrompt(string topic, int count, string sourceLanguage, string targetLanguage)
    {
        return $@"Generate exactly {count} common vocabulary words related to the topic ""{topic}"" 
in {GetLanguageName(sourceLanguage)} with their translations in {GetLanguageName(targetLanguage)}.

Requirements:
1. Words should be commonly used and relevant to the topic
2. Keep words simple and suitable for language learning
3. Return ONLY a JSON array in this exact format (no additional text):

[
  {{""source"": ""word1"", ""target"": ""translation1""}},
  {{""source"": ""word2"", ""target"": ""translation2""}}
]

Topic: {topic}
Source language: {GetLanguageName(sourceLanguage)}
Target language: {GetLanguageName(targetLanguage)}
Number of words: {count}";
    }

    private List<WordPair> ParseWordPairs(string content)
    {
        try
        {
            // Extract JSON from the response (in case there's extra text)
            var jsonStart = content.IndexOf('[');
            var jsonEnd = content.LastIndexOf(']');

            if (jsonStart == -1 || jsonEnd == -1)
            {
                throw new InvalidOperationException("No JSON array found in response");
            }

            var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonContent, options);

            if (parsed == null || parsed.Count == 0)
            {
                throw new InvalidOperationException("Failed to parse word pairs from response");
            }

            return parsed.Select(item => new WordPair
            {
                SourceWord = item["source"],
                TargetWord = item["target"]
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing word pairs from content: {Content}", content);
            throw new InvalidOperationException("Failed to parse word pairs", ex);
        }
    }

    private static string GetLanguageName(string languageCode)
    {
        var languageNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", "English" },
            { "ar", "Arabic" },
            { "es", "Spanish" },
            { "fr", "French" },
            { "de", "German" },
            { "it", "Italian" },
            { "pt", "Portuguese" },
            { "ru", "Russian" },
            { "zh", "Chinese" },
            { "ja", "Japanese" },
            { "ko", "Korean" },
            { "hi", "Hindi" },
            { "nl", "Dutch" },
            { "pl", "Polish" },
            { "tr", "Turkish" },
            { "sv", "Swedish" },
            { "no", "Norwegian" },
            { "da", "Danish" },
            { "fi", "Finnish" },
            { "el", "Greek" },
            { "cs", "Czech" },
            { "hu", "Hungarian" },
            { "ro", "Romanian" },
            { "th", "Thai" },
            { "vi", "Vietnamese" },
            { "id", "Indonesian" },
            { "uk", "Ukrainian" }
        };

        var baseCode = languageCode.Split('-')[0];
        return languageNames.TryGetValue(baseCode, out var name) ? name : languageCode;
    }
}