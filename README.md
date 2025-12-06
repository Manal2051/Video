# Language Video Generator API

A .NET Core 9 API that generates language learning videos using the Json2Video API. This service creates videos where words are displayed and spoken in both source and target languages, ideal for vocabulary learning.

## Features

- 🎯 **Topic-based word generation** using OpenAI GPT-4
- 🗣️ **Multi-language support** with 27+ languages
- 🎬 **Automatic video creation** via Json2Video API
- ⚡ **Optimized rendering** following Json2Video best practices
- 📊 **Real-time status tracking** for video generation jobs
- 🔄 **Configurable timing** between words

## Architecture

The application follows clean architecture principles with the following layers:

```
LanguageVideoGenerator.Api/
├── Controllers/           # API endpoints
├── Services/             # Business logic
│   ├── VideoGeneratorService    # Main orchestration
│   ├── WordGeneratorService     # AI word generation
│   └── Json2VideoClient         # API client
├── Models/               # Data models
│   ├── VideoModels              # Request/Response models
│   └── Json2Video/              # Json2Video API models
└── Configuration/        # Settings and mappings
```

## How It Works

### Video Generation Flow

1. **Word Generation**: Uses OpenAI GPT-4 to generate topic-relevant vocabulary
2. **Translation**: Generates word pairs in source and target languages
3. **Scene Construction**: Creates separate scenes for each word pair (optimal for parallel rendering)
4. **Element Composition**: Each scene contains:
   - Source language text (top, white)
   - Source language voice-over
   - Target language text (bottom, gold)
   - Target language voice-over
   - Configurable pause between words
5. **Video Rendering**: Submits to Json2Video API for rendering
6. **Status Tracking**: Monitor rendering progress

### Scene Structure (Following Json2Video Best Practices)

According to Json2Video documentation, the rendering engine is designed to render scenes in **parallel**. Therefore, we split each word pair into its own scene for optimal performance:

```json
{
  "scenes": [
    {
      "comment": "word1 -> translation1",
      "elements": [
        { "type": "text", "text": "word1" },      // Source text
        { "type": "voice", "text": "word1" },     // Source voice
        { "type": "text", "text": "translation1" }, // Target text (starts after source)
        { "type": "voice", "text": "translation1" } // Target voice (with pause)
      ]
    },
    {
      "comment": "word2 -> translation2",
      // ... next word pair
    }
  ]
}
```

**Key Design Decisions:**

- ✅ **Separate scenes per word** → Enables parallel rendering (faster)
- ✅ **Azure voice model** → Free, included in all plans
- ✅ **Auto-calculated durations** → Uses `duration: -1` for voices
- ✅ **Sequential element timing** → Uses `start: -1` to chain elements
- ✅ **Extra-time for pauses** → Adds configurable delays between words

## Prerequisites

- .NET 9 SDK
- Json2Video API Key ([Get free API key](https://json2video.com))
- OpenAI API Key

## Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd LanguageVideoGenerator
   ```

2. **Configure API Keys**
   
   Edit `appsettings.json`:
   ```json
   {
     "Json2Video": {
       "ApiKey": "YOUR_JSON2VIDEO_API_KEY"
     },
     "OpenAI": {
       "ApiKey": "YOUR_OPENAI_API_KEY"
     }
   }
   ```

3. **Restore dependencies**
   ```bash
   cd LanguageVideoGenerator.Api
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001` (or the port shown in console)

## API Endpoints

### 1. Generate Video

**POST** `/api/VideoGenerator/generate`

Creates a new language learning video.

**Request Body:**
```json
{
  "topic": "animals",
  "wordCount": 5,
  "sourceLanguage": "en",
  "targetLanguage": "ar",
  "pauseBetweenWords": 1.5,
  "resolution": "full-hd",
  "backgroundColor": "#000000"
}
```

**Response:**
```json
{
  "success": true,
  "projectId": "abc123xyz",
  "message": "Video generation started successfully",
  "timestamp": "2024-12-04T10:30:00Z",
  "generatedWords": [
    {
      "sourceWord": "cat",
      "targetWord": "قطة"
    },
    {
      "sourceWord": "dog",
      "targetWord": "كلب"
    }
  ]
}
```

### 2. Check Video Status

**GET** `/api/VideoGenerator/status/{projectId}`

Retrieves the status of a video generation job.

**Response (Processing):**
```json
{
  "success": true,
  "status": "running",
  "message": null
}
```

**Response (Complete):**
```json
{
  "success": true,
  "status": "done",
  "videoUrl": "https://assets.json2video.com/...",
  "subtitlesUrl": "https://assets.json2video.com/...",
  "createdAt": "2024-12-04T10:30:00Z",
  "endedAt": "2024-12-04T10:35:00Z",
  "duration": 45.5,
  "size": 5242880,
  "width": 1920,
  "height": 1080,
  "renderingTime": 300
}
```

### 3. Get Supported Languages

**GET** `/api/VideoGenerator/languages`

Returns list of supported languages.

**Response:**
```json
{
  "languages": [
    { "code": "en", "name": "English" },
    { "code": "ar", "name": "Arabic" },
    { "code": "es", "name": "Spanish" }
    // ... more languages
  ]
}
```

## Supported Languages

The API supports 27+ languages with Azure Text-to-Speech voices:

- English (en)
- Arabic (ar)
- Spanish (es)
- French (fr)
- German (de)
- Italian (it)
- Portuguese (pt)
- Russian (ru)
- Chinese (zh)
- Japanese (ja)
- Korean (ko)
- Hindi (hi)
- Dutch (nl)
- Polish (pl)
- Turkish (tr)
- And more...

See the `/api/VideoGenerator/languages` endpoint for the complete list.

## Configuration Options

### VideoGenerationRequest Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `topic` | string | required | Topic for word generation (e.g., "animals", "food") |
| `wordCount` | int | 10 | Number of words to generate (1-100) |
| `sourceLanguage` | string | "en" | Source language code |
| `targetLanguage` | string | "ar" | Target language code |
| `pauseBetweenWords` | double | 1.0 | Seconds to pause between words (0-10) |
| `resolution` | string | "full-hd" | Video resolution (sd, hd, full-hd, squared) |
| `backgroundColor` | string | "#000000" | Background color in hex format |

## Usage Examples

### cURL Example

```bash
# Generate video
curl -X POST https://localhost:5001/api/VideoGenerator/generate \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "fruits",
    "wordCount": 3,
    "sourceLanguage": "en",
    "targetLanguage": "es",
    "pauseBetweenWords": 2.0
  }'

# Check status
curl https://localhost:5001/api/VideoGenerator/status/abc123xyz
```

### C# Example

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Generate video
var request = new
{
    topic = "colors",
    wordCount = 5,
    sourceLanguage = "en",
    targetLanguage = "fr",
    pauseBetweenWords = 1.5
};

var response = await client.PostAsJsonAsync("/api/VideoGenerator/generate", request);
var result = await response.Content.ReadFromJsonAsync<VideoGenerationResponse>();

Console.WriteLine($"Project ID: {result.ProjectId}");

// Check status
var status = await client.GetFromJsonAsync<VideoStatusResponse>(
    $"/api/VideoGenerator/status/{result.ProjectId}");

Console.WriteLine($"Status: {status.Status}");
if (status.Status == "done")
{
    Console.WriteLine($"Video URL: {status.VideoUrl}");
}
```

### JavaScript/TypeScript Example

```typescript
// Generate video
const response = await fetch('https://localhost:5001/api/VideoGenerator/generate', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    topic: 'animals',
    wordCount: 10,
    sourceLanguage: 'en',
    targetLanguage: 'ar',
    pauseBetweenWords: 1.5
  })
});

const result = await response.json();
console.log('Project ID:', result.projectId);

// Poll for status
const checkStatus = async (projectId: string) => {
  const statusResponse = await fetch(
    `https://localhost:5001/api/VideoGenerator/status/${projectId}`
  );
  const status = await statusResponse.json();
  
  if (status.status === 'done') {
    console.log('Video ready:', status.videoUrl);
  } else if (status.status === 'error') {
    console.error('Error:', status.message);
  } else {
    // Still processing, check again in 10 seconds
    setTimeout(() => checkStatus(projectId), 10000);
  }
};

checkStatus(result.projectId);
```

## Json2Video Integration Details

### Following Best Practices

This implementation strictly follows Json2Video documentation guidelines:

1. **Scene Structure** (from `Optimizing_rendering.txt`):
   - ✅ Split movie into scenes for parallel rendering
   - ✅ Each significant visual change = new scene
   - ✅ Avoids single long scene with timed elements

2. **Voice Elements** (from `Voice_element.txt`):
   - ✅ Uses Azure model (free, no credit consumption)
   - ✅ Proper voice selection per language
   - ✅ Auto-calculated duration with `duration: -1`

3. **Text Elements** (from `Text_element.txt`):
   - ✅ Proper positioning with `vertical-position` and `horizontal-position`
   - ✅ Font settings for multilingual support
   - ✅ Fade-in effects for smooth appearance

4. **Timing** (from `Duration_and_timing.txt`):
   - ✅ Uses `start: -1` to chain elements sequentially
   - ✅ Uses `extra-time` for pauses between words
   - ✅ Scene duration auto-calculated from elements

5. **Caching** (from `Caching_system.txt`):
   - ✅ Cache enabled by default for efficiency
   - ✅ Can be disabled if fresh render needed

### Voice Cost Information

According to Json2Video documentation:

| Model | Cost |
|-------|------|
| Azure | 0 credits/minute (FREE) |
| ElevenLabs | 60 credits/minute |
| ElevenLabs Flash | 60 credits/minute |

**This API uses Azure model exclusively** to avoid credit consumption.

## Troubleshooting

### Common Issues

**1. "API key is missing or invalid"**
- Verify your Json2Video API key in `appsettings.json`
- Get a free key at https://json2video.com

**2. "Language not supported"**
- Check supported languages via `/api/VideoGenerator/languages`
- Use base language codes (e.g., "en" not "en-US")

**3. "Video status shows 'error'"**
- Check the error message in the status response
- Verify your Json2Video account has sufficient quota
- Review logs for detailed error information

**4. "OpenAI API error"**
- Verify your OpenAI API key is valid
- Ensure you have sufficient credits in your OpenAI account

## Development

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

### Docker Support

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LanguageVideoGenerator.Api.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LanguageVideoGenerator.Api.dll"]
```

## Performance Considerations

- **Rendering Time**: Videos typically take 30-60 seconds per word pair
- **Parallel Rendering**: Using separate scenes enables Json2Video's parallel processing
- **API Rate Limits**: Respect Json2Video API rate limits
- **Caching**: Reusing identical requests leverages Json2Video's caching system

## Security

- 🔒 API keys stored in configuration (use environment variables in production)
- 🔐 HTTPS enforced in production
- 🛡️ Input validation on all endpoints
- 📝 Comprehensive logging for audit trails

## License

MIT License - feel free to use in your projects

## Support

For issues related to:
- **This API**: Create an issue in this repository
- **Json2Video**: Visit https://json2video.com/docs
- **OpenAI**: Visit https://platform.openai.com/docs

## Acknowledgments

- [Json2Video](https://json2video.com) - Video generation API
- [OpenAI](https://openai.com) - Word generation
- [Azure Text-to-Speech](https://azure.microsoft.com/en-us/products/cognitive-services/text-to-speech/) - Voice synthesis

## Deployment with Portainer

Use one of the two approaches below on your AWS Lightsail VM running Portainer.

### Option A: Git-based Stack (Portainer builds from Dockerfile)

- Repo path: `LanguageVideoGenerator/docker-compose.yml`
- Branch: `main`
- Environment variables in Portainer:
  - `JSON2VIDEO_API_KEY` (required)
  - `OPENAI_API_KEY` (optional if only Json2Video is used)
  - `ASPNETCORE_ENVIRONMENT=Production`
- Exposes HTTP on host `5000` by default (maps to container `80`).

Steps in Portainer:
- Stacks -> Add Stack -> Git repository
- Repository URL: `https://github.com/Manal2051/Video`
- Compose path: `LanguageVideoGenerator/docker-compose.yml`
- Set env vars and deploy

### Option B: Image-based Stack (recommended for faster deploys)

1) Enable GitHub Actions to publish the image to GHCR (already configured by `.github/workflows/docker-build-push.yml`). On push to `main`, it publishes:
- `ghcr.io/<owner>/languagevideogenerator-api:latest`

2) Use the production compose:
- Compose path: `LanguageVideoGenerator/docker-compose.prod.yml`
- Provide a `.env` in the repo or set the variables in Portainer:
  - `IMAGE=ghcr.io/<owner>/languagevideogenerator-api:latest`
  - `JSON2VIDEO_API_KEY=...`
  - `OPENAI_API_KEY=...`
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `HOST_HTTP_PORT=5000`

Note: If your GHCR image is private, either make the package public or add a Registry credential in Portainer for `ghcr.io` with a Personal Access Token that has `read:packages`.

### Local quickstart with Docker Compose

Create `.env` from sample and run:

```bash
copy LanguageVideoGenerator\.env.sample LanguageVideoGenerator\.env
docker compose -f LanguageVideoGenerator/docker-compose.yml --project-directory LanguageVideoGenerator up --build -d
```

Then open: `http://localhost:5000`