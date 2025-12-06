using LanguageVideoGenerator.Api.Configuration;
using LanguageVideoGenerator.Api.Models;
using LanguageVideoGenerator.Api.Models.Json2Video;

namespace LanguageVideoGenerator.Api.Services;

/// <summary>
/// Interface for video generator service
/// </summary>
public interface IVideoGeneratorService
{
    /// <summary>
    /// Generates a language learning video
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a video generation job
    /// </summary>
    Task<VideoStatusResponse> GetVideoStatusAsync(
        string projectId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for orchestrating video generation
/// </summary>
public class VideoGeneratorService : IVideoGeneratorService
{
    private readonly IWordGeneratorService _wordGenerator;
    private readonly IJson2VideoClient _json2VideoClient;
    private readonly ILogger<VideoGeneratorService> _logger;

    public VideoGeneratorService(
        IWordGeneratorService wordGenerator,
        IJson2VideoClient json2VideoClient,
        ILogger<VideoGeneratorService> logger)
    {
        _wordGenerator = wordGenerator ?? throw new ArgumentNullException(nameof(wordGenerator));
        _json2VideoClient = json2VideoClient ?? throw new ArgumentNullException(nameof(json2VideoClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<VideoGenerationResponse> GenerateVideoAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting video generation for topic '{Topic}' with {Count} words",
                request.Topic, request.WordCount);

            // Validate request
            ValidateRequest(request);

            // Step 1: Generate word pairs
            var wordPairs = await _wordGenerator.GenerateWordPairsAsync(
                request.Topic,
                request.WordCount,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken);

            // Step 2: Build movie JSON following Json2Video best practices
            var movieRequest = BuildMovieRequest(request, wordPairs);

            // Step 3: Submit to Json2Video API
            var creationResponse = await _json2VideoClient.CreateMovieAsync(movieRequest, cancellationToken);

            // Step 4: Return response
            return new VideoGenerationResponse
            {
                Success = creationResponse.Success,
                ProjectId = creationResponse.Project,
                Message = "Video generation started successfully",
                Timestamp = creationResponse.Timestamp,
                GeneratedWords = wordPairs
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video");
            return new VideoGenerationResponse
            {
                Success = false,
                Message = $"Failed to generate video: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc/>
    public async Task<VideoStatusResponse> GetVideoStatusAsync(
        string projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statusResponse = await _json2VideoClient.GetMovieStatusAsync(projectId, cancellationToken);

            return new VideoStatusResponse
            {
                Success = statusResponse.Success,
                Status = statusResponse.Movie?.Status ?? "unknown",
                VideoUrl = statusResponse.Movie?.Url,
                SubtitlesUrl = statusResponse.Movie?.Ass,
                Message = statusResponse.Movie?.Message,
                CreatedAt = statusResponse.Movie?.CreatedAt,
                EndedAt = statusResponse.Movie?.EndedAt,
                Duration = statusResponse.Movie?.Duration,
                Size = statusResponse.Movie?.Size,
                Width = statusResponse.Movie?.Width,
                Height = statusResponse.Movie?.Height,
                RenderingTime = statusResponse.Movie?.RenderingTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video status for project: {ProjectId}", projectId);
            return new VideoStatusResponse
            {
                Success = false,
                Status = "error",
                Message = $"Failed to get video status: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Builds the movie request following Json2Video best practices
    /// Using single scene approach to avoid concatenation issues
    /// </summary>
    private MovieRequest BuildMovieRequest(VideoGenerationRequest request, List<WordPair> wordPairs)
    {
        var movieRequest = new MovieRequest
        {
            Comment = $"Language learning video: {request.Topic} ({request.SourceLanguage} to {request.TargetLanguage})",
            Resolution = request.Resolution,
            Quality = "high",
            Cache = false,
            Scenes = new List<Scene>()
        };

        // Get voices for the languages
        var sourceVoice = LanguageVoiceMapping.GetVoice(request.SourceLanguage);
        var targetVoice = LanguageVoiceMapping.GetVoice(request.TargetLanguage);

        _logger.LogDebug(
            "Using voices: Source={SourceVoice}, Target={TargetVoice}",
            sourceVoice, targetVoice);

        // Build a single scene with all word pairs sequenced
        // Each word pair takes 5 seconds
        double secondsPerWord = 5.0;
        var scene = new Scene
        {
            Comment = $"All words: {string.Join(", ", wordPairs.Select(w => w.SourceWord))}",
            BackgroundColor = request.BackgroundColor,
            Duration = wordPairs.Count * secondsPerWord, // Total duration
            Elements = new List<Element>()
        };

        for (int i = 0; i < wordPairs.Count; i++)
        {
            var wordPair = wordPairs[i];
            double startTime = i * secondsPerWord;
            double targetVoiceStart = startTime + 2.5; // When target voice plays

            // Source text - visible only until target word appears (first 2.5 seconds)
            scene.Elements.Add(new TextElement
            {
                Text = wordPair.SourceWord,
                Start = startTime,
                Duration = 2.5, // Disappears when target voice/text starts
                Settings = new Dictionary<string, object>
                {
                    { "font-size", "80px" },
                    { "color", "#FFFFFF" },
                    { "text-align", "center" }
                }
            });

            // Target text - appears synced with target voice (at 2.5s into word segment)
            scene.Elements.Add(new TextElement
            {
                Text = wordPair.TargetWord,
                Start = targetVoiceStart, // Synced with target voice
                Duration = secondsPerWord - 2.5, // Visible for remaining time
                Settings = new Dictionary<string, object>
                {
                    { "font-size", "80px" },
                    { "color", "#FFD700" },
                    { "text-align", "center" }
                }
            });

            // Source voice - plays at start of word segment
            scene.Elements.Add(new VoiceElement
            {
                Text = wordPair.SourceWord,
                Voice = sourceVoice,
                Model = "azure",
                Start = startTime
            });

            // Target voice - plays at 2.5 seconds (synced with target text)
            scene.Elements.Add(new VoiceElement
            {
                Text = wordPair.TargetWord,
                Voice = targetVoice,
                Model = "azure",
                Start = targetVoiceStart
            });
        }

        movieRequest.Scenes.Add(scene);
        return movieRequest;
    }

    private void ValidateRequest(VideoGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Topic))
        {
            throw new ArgumentException("Topic is required", nameof(request.Topic));
        }

        if (request.WordCount < 1 || request.WordCount > 100)
        {
            throw new ArgumentException("Word count must be between 1 and 100", nameof(request.WordCount));
        }

        if (string.IsNullOrWhiteSpace(request.SourceLanguage))
        {
            throw new ArgumentException("Source language is required", nameof(request.SourceLanguage));
        }

        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new ArgumentException("Target language is required", nameof(request.TargetLanguage));
        }

        if (!LanguageVoiceMapping.IsSupported(request.SourceLanguage))
        {
            throw new ArgumentException(
                $"Source language '{request.SourceLanguage}' is not supported",
                nameof(request.SourceLanguage));
        }

        if (!LanguageVoiceMapping.IsSupported(request.TargetLanguage))
        {
            throw new ArgumentException(
                $"Target language '{request.TargetLanguage}' is not supported",
                nameof(request.TargetLanguage));
        }

        if (request.PauseBetweenWords < 0 || request.PauseBetweenWords > 10)
        {
            throw new ArgumentException(
                "Pause between words must be between 0 and 10 seconds",
                nameof(request.PauseBetweenWords));
        }
    }
}