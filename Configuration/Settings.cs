namespace LanguageVideoGenerator.Api.Configuration;

/// <summary>
/// Configuration settings for Json2Video API
/// </summary>
public class Json2VideoSettings
{
    public const string SectionName = "Json2Video";

    /// <summary>
    /// Json2Video API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Json2Video API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.json2video.com/v2";

    /// <summary>
    /// Timeout in seconds for API requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 1000;
}

/// <summary>
/// Configuration settings for OpenAI (for word generation)
/// </summary>
public class OpenAISettings
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// OpenAI API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use for word generation
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Maximum tokens for completion
    /// </summary>
    public int MaxTokens { get; set; } = 500;
}

/// <summary>
/// Language voice mapping for Azure TTS
/// Based on Json2Video documentation for Azure voices
/// </summary>
public static class LanguageVoiceMapping
{
    private static readonly Dictionary<string, string> VoiceMap = new()
    {
        // English voices
        { "en", "en-US-EmmaMultilingualNeural" },
        { "en-US", "en-US-EmmaMultilingualNeural" },
        { "en-GB", "en-GB-SoniaNeural" },
        { "en-AU", "en-AU-NatashaNeural" },
        
        // Arabic
        { "ar", "ar-SA-ZariyahNeural" },
        { "ar-SA", "ar-SA-ZariyahNeural" },
        { "ar-EG", "ar-EG-SalmaNeural" },
        
        // Spanish
        { "es", "es-ES-ElviraNeural" },
        { "es-ES", "es-ES-ElviraNeural" },
        { "es-MX", "es-MX-DaliaNeural" },
        
        // French
        { "fr", "fr-FR-DeniseNeural" },
        { "fr-FR", "fr-FR-DeniseNeural" },
        { "fr-CA", "fr-CA-SylvieNeural" },
        
        // German
        { "de", "de-DE-KatjaNeural" },
        { "de-DE", "de-DE-KatjaNeural" },
        { "de-CH", "de-CH-LeniNeural" },
        
        // Italian
        { "it", "it-IT-ElsaNeural" },
        { "it-IT", "it-IT-ElsaNeural" },
        
        // Portuguese
        { "pt", "pt-BR-FranciscaNeural" },
        { "pt-BR", "pt-BR-FranciscaNeural" },
        { "pt-PT", "pt-PT-RaquelNeural" },
        
        // Russian
        { "ru", "ru-RU-SvetlanaNeural" },
        
        // Chinese
        { "zh", "zh-CN-XiaoxiaoNeural" },
        { "zh-CN", "zh-CN-XiaoxiaoNeural" },
        { "zh-TW", "zh-TW-HsiaoChenNeural" },
        
        // Japanese
        { "ja", "ja-JP-NanamiNeural" },
        { "ja-JP", "ja-JP-NanamiNeural" },
        
        // Korean
        { "ko", "ko-KR-SunHiNeural" },
        { "ko-KR", "ko-KR-SunHiNeural" },
        
        // Hindi
        { "hi", "hi-IN-SwaraNeural" },
        
        // Dutch
        { "nl", "nl-NL-ColetteNeural" },
        
        // Polish
        { "pl", "pl-PL-ZofiaNeural" },
        
        // Turkish
        { "tr", "tr-TR-EmelNeural" },
        
        // Swedish
        { "sv", "sv-SE-SofieNeural" },
        
        // Norwegian
        { "no", "nb-NO-PernilleNeural" },
        
        // Danish
        { "da", "da-DK-ChristelNeural" },
        
        // Finnish
        { "fi", "fi-FI-NooraNeural" },
        
        // Greek
        { "el", "el-GR-AthinaNeural" },
        
        // Czech
        { "cs", "cs-CZ-VlastaNeural" },
        
        // Hungarian
        { "hu", "hu-HU-NoemiNeural" },
        
        // Romanian
        { "ro", "ro-RO-AlinaNeural" },
        
        // Thai
        { "th", "th-TH-PremwadeeNeural" },
        
        // Vietnamese
        { "vi", "vi-VN-HoaiMyNeural" },
        
        // Indonesian
        { "id", "id-ID-GadisNeural" },
        
        // Ukrainian
        { "uk", "uk-UA-PolinaNeural" }
    };

    /// <summary>
    /// Gets the Azure voice name for a given language code
    /// </summary>
    public static string GetVoice(string languageCode)
    {
        // Try exact match first
        if (VoiceMap.TryGetValue(languageCode.ToLower(), out var voice))
        {
            return voice;
        }

        // Try base language code (e.g., "en" from "en-US")
        var baseCode = languageCode.Split('-')[0].ToLower();
        if (VoiceMap.TryGetValue(baseCode, out voice))
        {
            return voice;
        }

        // Default to English if not found
        return VoiceMap["en"];
    }

    /// <summary>
    /// Checks if a language code is supported
    /// </summary>
    public static bool IsSupported(string languageCode)
    {
        var baseCode = languageCode.Split('-')[0].ToLower();
        return VoiceMap.ContainsKey(languageCode.ToLower()) || VoiceMap.ContainsKey(baseCode);
    }
}