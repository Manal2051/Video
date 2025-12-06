using LanguageVideoGenerator.Api.Models;
using LanguageVideoGenerator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanguageVideoGenerator.Api.Controllers;

/// <summary>
/// Controller for language learning video generation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VideoGeneratorController : ControllerBase
{
    private readonly IVideoGeneratorService _videoGeneratorService;
    private readonly ILogger<VideoGeneratorController> _logger;

    public VideoGeneratorController(
        IVideoGeneratorService videoGeneratorService,
        ILogger<VideoGeneratorController> logger)
    {
        _videoGeneratorService = videoGeneratorService ?? throw new ArgumentNullException(nameof(videoGeneratorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a language learning video
    /// </summary>
    /// <param name="request">Video generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Video generation response with project ID</returns>
    /// <response code="200">Video generation started successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(VideoGenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VideoGenerationResponse>> GenerateVideo(
        [FromBody] VideoGenerationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Received video generation request for topic '{Topic}' with {Count} words",
                request.Topic, request.WordCount);

            var response = await _videoGeneratorService.GenerateVideoAsync(request, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Video generation failed",
                    Detail = response.Message
                });
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during video generation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",
                Detail = "An unexpected error occurred while generating the video"
            });
        }
    }

    /// <summary>
    /// Gets the status of a video generation job
    /// </summary>
    /// <param name="projectId">The project ID returned from the generate endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Video status information</returns>
    /// <response code="200">Status retrieved successfully</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("status/{projectId}")]
    [ProducesResponseType(typeof(VideoStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VideoStatusResponse>> GetVideoStatus(
        [FromRoute] string projectId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Checking status for project: {ProjectId}", projectId);

            var response = await _videoGeneratorService.GetVideoStatusAsync(projectId, cancellationToken);

            if (!response.Success)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Project not found",
                    Detail = response.Message
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video status for project: {ProjectId}", projectId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",
                Detail = "An unexpected error occurred while retrieving video status"
            });
        }
    }

    /// <summary>
    /// Gets supported language codes
    /// </summary>
    /// <returns>List of supported language codes</returns>
    /// <response code="200">Languages retrieved successfully</response>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(LanguagesResponse), StatusCodes.Status200OK)]
    public ActionResult<LanguagesResponse> GetSupportedLanguages()
    {
        var languages = new LanguagesResponse
        {
            Languages = new List<LanguageInfo>
            {
                new() { Code = "en", Name = "English" },
                new() { Code = "ar", Name = "Arabic" },
                new() { Code = "es", Name = "Spanish" },
                new() { Code = "fr", Name = "French" },
                new() { Code = "de", Name = "German" },
                new() { Code = "it", Name = "Italian" },
                new() { Code = "pt", Name = "Portuguese" },
                new() { Code = "ru", Name = "Russian" },
                new() { Code = "zh", Name = "Chinese" },
                new() { Code = "ja", Name = "Japanese" },
                new() { Code = "ko", Name = "Korean" },
                new() { Code = "hi", Name = "Hindi" },
                new() { Code = "nl", Name = "Dutch" },
                new() { Code = "pl", Name = "Polish" },
                new() { Code = "tr", Name = "Turkish" },
                new() { Code = "sv", Name = "Swedish" },
                new() { Code = "no", Name = "Norwegian" },
                new() { Code = "da", Name = "Danish" },
                new() { Code = "fi", Name = "Finnish" },
                new() { Code = "el", Name = "Greek" },
                new() { Code = "cs", Name = "Czech" },
                new() { Code = "hu", Name = "Hungarian" },
                new() { Code = "ro", Name = "Romanian" },
                new() { Code = "th", Name = "Thai" },
                new() { Code = "vi", Name = "Vietnamese" },
                new() { Code = "id", Name = "Indonesian" },
                new() { Code = "uk", Name = "Ukrainian" }
            }
        };

        return Ok(languages);
    }
}

/// <summary>
/// Response model for supported languages
/// </summary>
public class LanguagesResponse
{
    public List<LanguageInfo> Languages { get; set; } = new();
}

/// <summary>
/// Language information
/// </summary>
public class LanguageInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}