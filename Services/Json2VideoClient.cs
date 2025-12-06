using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LanguageVideoGenerator.Api.Configuration;
using LanguageVideoGenerator.Api.Models.Json2Video;
using Microsoft.Extensions.Options;

namespace LanguageVideoGenerator.Api.Services;

/// <summary>
/// Interface for Json2Video API client
/// </summary>
public interface IJson2VideoClient
{
    /// <summary>
    /// Creates a new movie rendering job
    /// </summary>
    Task<MovieCreationResponse> CreateMovieAsync(MovieRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a movie rendering job
    /// </summary>
    Task<MovieStatusResponse> GetMovieStatusAsync(string projectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Http client for Json2Video API
/// </summary>
public class Json2VideoClient : IJson2VideoClient
{
    private readonly HttpClient _httpClient;
    private readonly Json2VideoSettings _settings;
    private readonly ILogger<Json2VideoClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public Json2VideoClient(
        HttpClient httpClient,
        IOptions<Json2VideoSettings> settings,
        ILogger<Json2VideoClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure HttpClient - ensure BaseUrl ends with / for proper path concatenation
        var baseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc/>
    public async Task<MovieCreationResponse> CreateMovieAsync(
        MovieRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var requestUrl = $"{_httpClient.BaseAddress}movies";
            var json = JsonSerializer.Serialize(request, _jsonOptions);

            // Log complete request details
            _logger.LogInformation(
                "[Json2Video] [{RequestId}] ========== OUTGOING REQUEST ==========\n" +
                "Method: POST\n" +
                "URL: {Url}\n" +
                "Headers:\n" +
                "  x-api-key: {ApiKey}\n" +
                "  Content-Type: application/json\n" +
                "  Accept: application/json\n" +
                "Body:\n{RequestBody}",
                requestId,
                requestUrl,
                MaskApiKey(_settings.ApiKey),
                json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("movies", content, cancellationToken);

            stopwatch.Stop();
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = FormatHeaders(response.Headers, response.Content.Headers);

            // Log complete response details
            _logger.LogInformation(
                "[Json2Video] [{RequestId}] ========== INCOMING RESPONSE ==========\n" +
                "Status: {StatusCode} ({StatusCodeInt})\n" +
                "Duration: {Duration}ms\n" +
                "Headers:\n{ResponseHeaders}\n" +
                "Body:\n{ResponseBody}",
                requestId,
                response.StatusCode,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseHeaders,
                responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[Json2Video] [{RequestId}] Request failed with status {StatusCode}. Response: {ResponseBody}",
                    requestId,
                    response.StatusCode,
                    responseContent);
            }

            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<MovieCreationResponse>(responseContent, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize response");

            _logger.LogInformation(
                "[Json2Video] [{RequestId}] Movie created successfully. Project ID: {ProjectId}",
                requestId,
                result.Project);

            return result;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[Json2Video] [{RequestId}] HTTP error after {Duration}ms. Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw new InvalidOperationException("Failed to create movie with Json2Video API", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[Json2Video] [{RequestId}] Request timed out after {Duration}ms",
                requestId,
                stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException("Json2Video API request timed out", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[Json2Video] [{RequestId}] Unexpected error after {Duration}ms. Type: {ExceptionType}, Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.GetType().Name,
                ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MovieStatusResponse> GetMovieStatusAsync(
        string projectId,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var requestUrl = $"{_httpClient.BaseAddress}movies?project={projectId}";

            // Log complete request details
            _logger.LogInformation(
                "[Json2Video] [{RequestId}] ========== OUTGOING REQUEST ==========\n" +
                "Method: GET\n" +
                "URL: {Url}\n" +
                "Headers:\n" +
                "  x-api-key: {ApiKey}\n" +
                "  Accept: application/json",
                requestId,
                requestUrl,
                MaskApiKey(_settings.ApiKey));

            var response = await _httpClient.GetAsync($"movies?project={projectId}", cancellationToken);

            stopwatch.Stop();
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = FormatHeaders(response.Headers, response.Content.Headers);

            // Log complete response details
            _logger.LogInformation(
                "[Json2Video] [{RequestId}] ========== INCOMING RESPONSE ==========\n" +
                "Status: {StatusCode} ({StatusCodeInt})\n" +
                "Duration: {Duration}ms\n" +
                "Headers:\n{ResponseHeaders}\n" +
                "Body:\n{ResponseBody}",
                requestId,
                response.StatusCode,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseHeaders,
                responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[Json2Video] [{RequestId}] Request failed with status {StatusCode}. Response: {ResponseBody}",
                    requestId,
                    response.StatusCode,
                    responseContent);
            }

            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<MovieStatusResponse>(responseContent, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize response");

            _logger.LogInformation(
                "[Json2Video] [{RequestId}] Movie status: {Status} for project: {ProjectId}",
                requestId,
                result.Movie?.Status ?? "unknown",
                projectId);

            return result;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[Json2Video] [{RequestId}] HTTP error after {Duration}ms for project {ProjectId}. Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                projectId,
                ex.Message);
            throw new InvalidOperationException($"Failed to get movie status for project {projectId}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[Json2Video] [{RequestId}] Request timed out after {Duration}ms for project {ProjectId}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                projectId);
            throw new InvalidOperationException($"Json2Video API request timed out for project {projectId}", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[Json2Video] [{RequestId}] Unexpected error after {Duration}ms for project {ProjectId}. Type: {ExceptionType}, Message: {Message}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                projectId,
                ex.GetType().Name,
                ex.Message);
            throw;
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
        var sb = new StringBuilder();
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
}