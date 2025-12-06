namespace LanguageVideoGenerator.Api.Models;

/// <summary>
/// Request model for generating a language learning video
/// </summary>
public class VideoGenerationRequest
{
    /// <summary>
    /// Topic for word generation (e.g., "animals", "food", "travel")
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Number of words to generate
    /// </summary>
    public int WordCount { get; set; } = 10;

    /// <summary>
    /// Source language code (e.g., "en" for English)
    /// </summary>
    public string SourceLanguage { get; set; } = "en";

    /// <summary>
    /// Target language code (e.g., "ar" for Arabic)
    /// </summary>
    public string TargetLanguage { get; set; } = "ar";

    /// <summary>
    /// Duration in seconds to wait between words
    /// </summary>
    public double PauseBetweenWords { get; set; } = 1.0;

    /// <summary>
    /// Optional: Desired video duration in minutes. If set, overrides WordCount.
    /// </summary>
    public double? DurationMinutes { get; set; }

    /// <summary>
    /// Whether to repeat the secondary language word (total 3 times: 1 primary, 2 secondary).
    /// </summary>
    public bool UseSecondaryRepeat { get; set; } = false;

    /// <summary>
    /// Optional: Video resolution (default: "full-hd")
    /// </summary>
    public string Resolution { get; set; } = "full-hd";

    /// <summary>
    /// Optional: Background color in hex format (default: "#000000")
    /// </summary>
    public string BackgroundColor { get; set; } = "#000000";
}

/// <summary>
/// Response model for video generation
/// </summary>
public class VideoGenerationResponse
{
    public bool Success { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<WordPair> GeneratedWords { get; set; } = new();
}

/// <summary>
/// Response model for video status check
/// </summary>
public class VideoStatusResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? SubtitlesUrl { get; set; }
    public string? Message { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public double? Duration { get; set; }
    public long? Size { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? RenderingTime { get; set; }
}

/// <summary>
/// Represents a word pair (source and target language)
/// </summary>
public class WordPair
{
    public string SourceWord { get; set; } = string.Empty;
    public string TargetWord { get; set; } = string.Empty;
}