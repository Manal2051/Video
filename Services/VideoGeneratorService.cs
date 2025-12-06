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
            // Calculate WordCount from DurationMinutes if provided
            if (request.DurationMinutes.HasValue && request.DurationMinutes.Value > 0)
            {
                double totalSeconds = request.DurationMinutes.Value * 60;
                // Assuming 1 second per word + pause
                double secondsPerUnit = request.PauseBetweenWords + 1.0;
                int itemsPerPair = request.UseSecondaryRepeat ? 3 : 2;

                double calculatedPairs = totalSeconds / (itemsPerPair * secondsPerUnit);
                request.WordCount = (int)Math.Floor(calculatedPairs);

                // Clamp to valid range to pass validation
                if (request.WordCount < 1) request.WordCount = 1;
                if (request.WordCount > 100) request.WordCount = 100;

                _logger.LogInformation(
                    "Calculated WordCount: {WordCount} from Duration: {Duration} mins (Repeat: {Repeat})",
                    request.WordCount, request.DurationMinutes, request.UseSecondaryRepeat);
            }

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

        // Calculate timing
        // User logic: Each word is about 1 second + pause
        double wordDuration = 1.0;
        double pauseDuration = request.PauseBetweenWords;
        double itemDuration = wordDuration + pauseDuration;

        // Items per pair: Source + Target (+ Target if repeat)
        int itemsPerPair = request.UseSecondaryRepeat ? 3 : 2;
        double pairDuration = itemsPerPair * itemDuration;

        // Build a single scene with all word pairs sequenced
        var scene = new Scene
        {
            Comment = $"All words: {string.Join(", ", wordPairs.Select(w => w.SourceWord))}",
            BackgroundColor = request.BackgroundColor,
            Duration = wordPairs.Count * pairDuration, // Total duration
            Elements = new List<Element>()
        };

        for (int i = 0; i < wordPairs.Count; i++)
        {
            var wordPair = wordPairs[i];
            double startTime = i * pairDuration;

            // 1. Source Word
            double sourceStart = startTime;

            // Source text - visible during source audio
            scene.Elements.Add(new TextElement
            {
                Text = wordPair.SourceWord,
                Start = sourceStart,
                Duration = itemDuration,
                Settings = new Dictionary<string, object>
                {
                    { "font-size", "80px" },
                    { "color", "#FFFFFF" },
                    { "text-align", "center" }
                }
            });

            // Source voice
            scene.Elements.Add(new VoiceElement
            {
                Text = wordPair.SourceWord,
                Voice = sourceVoice,
                Model = "azure",
                Start = sourceStart
            });

            // 2. Target Word (First occurrence)
            double targetStart = sourceStart + itemDuration;

            // Target text - visible from start of target audio until end of pair
            // If repeating, it stays visible during the repeat too
            double targetTextDuration = pairDuration - itemDuration; // Remaining time in pair

            scene.Elements.Add(new TextElement
            {
                Text = wordPair.TargetWord,
                Start = targetStart,
                Duration = targetTextDuration,
                Settings = new Dictionary<string, object>
                {
                    { "font-size", "80px" },
                    { "color", "#FFD700" },
                    { "text-align", "center" }
                }
            });

            // Target voice 1
            scene.Elements.Add(new VoiceElement
            {
                Text = wordPair.TargetWord,
                Voice = targetVoice,
                Model = "azure",
                Start = targetStart
            });

            // 3. Target Word (Second occurrence - Optional)
            if (request.UseSecondaryRepeat)
            {
                double repeatStart = targetStart + itemDuration;

                // Target voice 2
                scene.Elements.Add(new VoiceElement
                {
                    Text = wordPair.TargetWord,
                    Voice = targetVoice,
                    Model = "azure",
                    Start = repeatStart
                });
            }
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