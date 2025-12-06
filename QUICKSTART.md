# Quick Start Guide

Get your language learning video generator up and running in 5 minutes!

## Prerequisites

- .NET 9 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- Json2Video API Key ([Get Free Key](https://json2video.com))
- OpenAI API Key ([Get Key](https://platform.openai.com))

## Step 1: Clone & Navigate

```bash
git clone <repository-url>
cd LanguageVideoGenerator
```

## Step 2: Configure API Keys

Edit `LanguageVideoGenerator.Api/appsettings.json`:

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

## Step 3: Run the Application

```bash
cd LanguageVideoGenerator.Api
dotnet run
```

The API will start at `https://localhost:5001`

## Step 4: Test the API

Open your browser to: `https://localhost:5001` (Swagger UI will load)

Or use cURL:

```bash
# Generate a video
curl -X POST https://localhost:5001/api/VideoGenerator/generate \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "animals",
    "wordCount": 3,
    "sourceLanguage": "en",
    "targetLanguage": "es",
    "pauseBetweenWords": 1.5
  }'
```

## Step 5: Get Your Video

The response will include a `projectId`. Use it to check status:

```bash
curl https://localhost:5001/api/VideoGenerator/status/YOUR_PROJECT_ID
```

When `status` is `"done"`, download from the `videoUrl` field!

## Alternative: Docker

1. Create `.env` file:
```bash
JSON2VIDEO_API_KEY=your_key_here
OPENAI_API_KEY=your_key_here
```

2. Run:
```bash
docker-compose up -d
```

3. API available at `http://localhost:5000`

## What's Next?

- Explore the full [README](README.md) for detailed documentation
- Check [ExampleRequests.md](ExampleRequests.md) for more examples
- Try different topics: "food", "travel", "colors", "numbers"
- Experiment with different language pairs
- Adjust `pauseBetweenWords` for learning pace

## Troubleshooting

### "The type or namespace name 'Azure' could not be found"

Run: `dotnet restore`

### "Unauthorized" error from Json2Video

Check your API key is correct in `appsettings.json`

### "Invalid language code"

Use `/api/VideoGenerator/languages` to see supported codes

### Videos take a long time

- Normal: 30-60 seconds per word pair
- Json2Video renders scenes in parallel for speed
- Reduce `wordCount` for faster testing

## Support

Questions? Check the main [README](README.md) or create an issue!

## Video Generation Process

```
1. API Request → 2. Word Generation (OpenAI) → 3. Json2Video Submission
                                                          ↓
5. Download Video ← 4. Parallel Scene Rendering ← Status: Rendering
```

Happy learning! 🎓🎬