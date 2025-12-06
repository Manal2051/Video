using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanguageVideoGenerator.Api.Models.Json2Video;

/// <summary>
/// Movie request object for Json2Video API
/// Based on API v2 Specification
/// </summary>
public class MovieRequest
{
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = "full-hd";

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("quality")]
    public string Quality { get; set; } = "high";

    [JsonPropertyName("cache")]
    public bool Cache { get; set; } = true;

    /// <summary>
    /// Set to true to add a watermark to the movie (for free/draft mode)
    /// </summary>
    [JsonPropertyName("draft")]
    public bool? Draft { get; set; }

    /// <summary>
    /// Movie ID string. Must be unique per project
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("scenes")]
    public List<Scene> Scenes { get; set; } = new();

    [JsonPropertyName("elements")]
    public List<Element>? Elements { get; set; }

    [JsonPropertyName("variables")]
    public Dictionary<string, object>? Variables { get; set; }

    [JsonPropertyName("client-data")]
    public Dictionary<string, object>? ClientData { get; set; }

    /// <summary>
    /// Export configurations for the movie (FTP, webhook, email, etc.)
    /// </summary>
    [JsonPropertyName("exports")]
    public List<ExportConfig>? Exports { get; set; }
}

/// <summary>
/// Export configuration for movies
/// </summary>
public class ExportConfig
{
    [JsonPropertyName("destinations")]
    public List<ExportDestination>? Destinations { get; set; }
}

/// <summary>
/// Export destination (webhook, ftp, email, etc.)
/// </summary>
public class ExportDestination
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonPropertyName("content-type")]
    public string? ContentType { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Scene object
/// Based on API v2 Specification
/// </summary>
public class Scene
{
    /// <summary>
    /// ID of the scene
    /// </summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("comment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Comment { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; } = -1;

    [JsonPropertyName("background-color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("elements")]
    public List<Element> Elements { get; set; } = new();

    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Variables { get; set; }

    [JsonPropertyName("cache")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? Cache { get; set; }

    /// <summary>
    /// Condition to be met for the scene to be rendered.
    /// If the value is false or empty string, the scene is removed from the movie
    /// </summary>
    [JsonPropertyName("condition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Condition { get; set; }

    /// <summary>
    /// Iterate over a variable array to create multiple scenes
    /// </summary>
    [JsonPropertyName("iterate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Iterate { get; set; }
}

/// <summary>
/// Base element object
/// Based on API v2 Specification
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextElement), "text")]
[JsonDerivedType(typeof(VoiceElement), "voice")]
[JsonDerivedType(typeof(ImageElement), "image")]
[JsonDerivedType(typeof(VideoElement), "video")]
[JsonDerivedType(typeof(AudioElement), "audio")]
public class Element
{
    // Note: "type" is handled by JsonPolymorphic attribute - do not add a Type property here
    // to avoid duplicate "type" keys in JSON output

    /// <summary>
    /// ID of the element
    /// </summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("comment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Comment { get; set; }

    /// <summary>
    /// Duration in seconds. -1 = auto-calculate, -2 = match parent
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Duration { get; set; }

    [JsonPropertyName("start")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Start { get; set; }

    [JsonPropertyName("extra-time")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ExtraTime { get; set; }

    [JsonPropertyName("cache")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Cache { get; set; }

    [JsonPropertyName("fade-in")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FadeIn { get; set; }

    [JsonPropertyName("fade-out")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FadeOut { get; set; }

    [JsonPropertyName("z-index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ZIndex { get; set; }

    /// <summary>
    /// Condition to be met for the element to be rendered.
    /// If the value is false or empty string, the element is removed from the scene
    /// </summary>
    [JsonPropertyName("condition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Condition { get; set; }

    /// <summary>
    /// Local variables of the element
    /// </summary>
    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Text element
/// </summary>
public class TextElement : Element
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("style")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Style { get; set; }

    [JsonPropertyName("settings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Settings { get; set; }

    /// <summary>
    /// Position preset. Only set x/y when using "custom"
    /// </summary>
    [JsonPropertyName("position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Position { get; set; }

    /// <summary>
    /// Only used when position is "custom"
    /// </summary>
    [JsonPropertyName("x")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? X { get; set; }

    /// <summary>
    /// Only used when position is "custom"
    /// </summary>
    [JsonPropertyName("y")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Y { get; set; }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Height { get; set; }

    public TextElement()
    {
        // API spec: Text element default duration is -2 (match scene duration)
        Duration = -2;
    }
}

/// <summary>
/// Voice element for text-to-speech
/// Supports Azure, ElevenLabs, and ElevenLabs Flash v2.5 models
/// Based on API v2 Specification and AI integrations documentation
/// </summary>
public class VoiceElement : Element
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("voice")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Voice { get; set; }

    /// <summary>
    /// The model to use for voice generation:
    /// - "azure" (default): Microsoft Azure TTS
    /// - "elevenlabs": ElevenLabs TTS (costs 60 credits/minute)
    /// - "elevenlabs-flash-v2-5": ElevenLabs Flash v2.5 (faster, costs 60 credits/minute)
    /// </summary>
    [JsonPropertyName("model")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Model { get; set; }

    /// <summary>
    /// Connection ID to use for generation. If specified, the API Key in the connection will be used.
    /// Used for "Bring Your Own API Key" scenarios with ElevenLabs.
    /// </summary>
    [JsonPropertyName("connection")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Connection { get; set; }

    [JsonPropertyName("volume")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Volume { get; set; }

    [JsonPropertyName("muted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Muted { get; set; }

    /// <summary>
    /// Model settings that will be passed to the voice model.
    /// For ElevenLabs, supports settings like:
    /// - language_code: Language code for the voice
    /// - voice_settings.speed: Speed of the voice (0.7 to 1.2)
    /// </summary>
    [JsonPropertyName("model-settings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ModelSettings { get; set; }

    public VoiceElement()
    {
        // Type is handled by JsonPolymorphic attribute
    }
}

/// <summary>
/// Image element for displaying images
/// Based on API v2 Specification
/// </summary>
public class ImageElement : Element
{
    /// <summary>
    /// URL to the image file (JPG, PNG, GIF, etc.)
    /// </summary>
    [JsonPropertyName("src")]
    public string? Src { get; set; }

    /// <summary>
    /// Prompt to generate the image using AI (when using AI image generation)
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Model to use for AI image generation (e.g., "flux-pro")
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Connection ID for AI generation with your own API key
    /// </summary>
    [JsonPropertyName("connection")]
    public string? Connection { get; set; }

    /// <summary>
    /// Model settings for AI generation
    /// </summary>
    [JsonPropertyName("model-settings")]
    public Dictionary<string, object>? ModelSettings { get; set; }

    /// <summary>
    /// Aspect ratio for AI-generated images: horizontal, vertical, squared
    /// </summary>
    [JsonPropertyName("aspect-ratio")]
    public string? AspectRatio { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; } = "custom";

    [JsonPropertyName("x")]
    public int X { get; set; } = 0;

    [JsonPropertyName("y")]
    public int Y { get; set; } = 0;

    [JsonPropertyName("width")]
    public int Width { get; set; } = -1;

    [JsonPropertyName("height")]
    public int Height { get; set; } = -1;

    /// <summary>
    /// Resize mode: cover, fill, fit, contain
    /// </summary>
    [JsonPropertyName("resize")]
    public string? Resize { get; set; }

    /// <summary>
    /// Pan direction: left, top, right, bottom, top-left, top-right, bottom-left, bottom-right
    /// </summary>
    [JsonPropertyName("pan")]
    public string? Pan { get; set; }

    /// <summary>
    /// Zoom level percentage (-10 to 10)
    /// </summary>
    [JsonPropertyName("zoom")]
    public int? Zoom { get; set; }

    public ImageElement()
    {
        // Type is handled by JsonPolymorphic attribute
    }
}

/// <summary>
/// Video element for embedding video content
/// Based on API v2 Specification
/// </summary>
public class VideoElement : Element
{
    /// <summary>
    /// URL to the video file (MP4, MKV, MOV, etc.)
    /// </summary>
    [JsonPropertyName("src")]
    public string? Src { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; } = "custom";

    [JsonPropertyName("x")]
    public int X { get; set; } = 0;

    [JsonPropertyName("y")]
    public int Y { get; set; } = 0;

    [JsonPropertyName("width")]
    public int Width { get; set; } = -1;

    [JsonPropertyName("height")]
    public int Height { get; set; } = -1;

    /// <summary>
    /// Resize mode: cover, fill, fit, contain
    /// </summary>
    [JsonPropertyName("resize")]
    public string? Resize { get; set; }

    /// <summary>
    /// Number of loops (-1 for infinite, 1 for single play)
    /// </summary>
    [JsonPropertyName("loop")]
    public int? Loop { get; set; }

    /// <summary>
    /// Seek to specified time in seconds
    /// </summary>
    [JsonPropertyName("seek")]
    public double? Seek { get; set; }

    /// <summary>
    /// Mute the audio
    /// </summary>
    [JsonPropertyName("muted")]
    public bool Muted { get; set; } = false;

    /// <summary>
    /// Volume gain (1 = no gain)
    /// </summary>
    [JsonPropertyName("volume")]
    public double Volume { get; set; } = 1.0;

    /// <summary>
    /// Chroma key settings for green screen effect
    /// </summary>
    [JsonPropertyName("chroma-key")]
    public ChromaKeySettings? ChromaKey { get; set; }

    public VideoElement()
    {
        // Type is handled by JsonPolymorphic attribute
    }
}

/// <summary>
/// Audio element for audio playback
/// Based on API v2 Specification
/// </summary>
public class AudioElement : Element
{
    /// <summary>
    /// URL to the audio file (MP3, WAV, etc.)
    /// </summary>
    [JsonPropertyName("src")]
    public string? Src { get; set; }

    /// <summary>
    /// Number of loops (-1 for infinite, 1 for single play)
    /// </summary>
    [JsonPropertyName("loop")]
    public int? Loop { get; set; }

    /// <summary>
    /// Seek to specified time in seconds
    /// </summary>
    [JsonPropertyName("seek")]
    public double? Seek { get; set; }

    /// <summary>
    /// Mute the audio
    /// </summary>
    [JsonPropertyName("muted")]
    public bool Muted { get; set; } = false;

    /// <summary>
    /// Volume gain (1 = no gain, max 10)
    /// </summary>
    [JsonPropertyName("volume")]
    public double Volume { get; set; } = 1.0;

    public AudioElement()
    {
        // Type is handled by JsonPolymorphic attribute
    }
}

/// <summary>
/// Chroma key settings for green screen effect
/// </summary>
public class ChromaKeySettings
{
    /// <summary>
    /// Color to make transparent (e.g., "#00b140" for green)
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Tolerance for color selection (1-100)
    /// </summary>
    [JsonPropertyName("tolerance")]
    public int Tolerance { get; set; } = 25;
}

/// <summary>
/// Json2Video API response for movie creation
/// </summary>
public class MovieCreationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("project")]
    public string Project { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Json2Video API response for movie status
/// </summary>
public class MovieStatusResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("movie")]
    public MovieDetails? Movie { get; set; }

    [JsonPropertyName("remaining_quota")]
    public QuotaInfo? RemainingQuota { get; set; }
}

public class MovieDetails
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("project")]
    public string Project { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("ass")]
    public JsonElement? AssElement { get; set; }

    /// <summary>
    /// Gets the ASS subtitle URL if available (API returns false when not available, or a URL string)
    /// </summary>
    [JsonIgnore]
    public string? Ass => AssElement?.ValueKind == JsonValueKind.String ? AssElement?.GetString() : null;

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("ended_at")]
    public DateTime? EndedAt { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("rendering_time")]
    public int? RenderingTime { get; set; }
}

public class QuotaInfo
{
    [JsonPropertyName("time")]
    public int Time { get; set; }
}