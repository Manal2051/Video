# Example API Requests for Language Video Generator

## 1. Generate English to Arabic Video

### Request
POST http://localhost:5000/api/VideoGenerator/generate
Content-Type: application/json

{
  "topic": "animals",
  "wordCount": 5,
  "sourceLanguage": "en",
  "targetLanguage": "ar",
  "pauseBetweenWords": 1.5,
  "resolution": "full-hd",
  "backgroundColor": "#000000"
}

---

## 2. Generate English to Spanish Video (Food Topic)

### Request
POST http://localhost:5000/api/VideoGenerator/generate
Content-Type: application/json

{
  "topic": "food",
  "wordCount": 10,
  "sourceLanguage": "en",
  "targetLanguage": "es",
  "pauseBetweenWords": 2.0,
  "resolution": "full-hd",
  "backgroundColor": "#1a1a2e"
}

---

## 3. Generate French to English Video (Colors)

### Request
POST http://localhost:5000/api/VideoGenerator/generate
Content-Type: application/json

{
  "topic": "colors",
  "wordCount": 7,
  "sourceLanguage": "fr",
  "targetLanguage": "en",
  "pauseBetweenWords": 1.0,
  "resolution": "full-hd",
  "backgroundColor": "#0f3460"
}

---

## 4. Check Video Status

### Request
GET http://localhost:5000/api/VideoGenerator/status/{projectId}

Replace {projectId} with the actual project ID returned from the generate endpoint.

---

## 5. Get Supported Languages

### Request
GET http://localhost:5000/api/VideoGenerator/languages

---

## Using cURL

### Generate Video
```bash
curl -X POST http://localhost:5000/api/VideoGenerator/generate \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "travel",
    "wordCount": 5,
    "sourceLanguage": "en",
    "targetLanguage": "de",
    "pauseBetweenWords": 1.5
  }'
```

### Check Status
```bash
curl http://localhost:5000/api/VideoGenerator/status/YOUR_PROJECT_ID
```

### Get Languages
```bash
curl http://localhost:5000/api/VideoGenerator/languages
```

---

## Using PowerShell

### Generate Video
```powershell
$body = @{
    topic = "nature"
    wordCount = 8
    sourceLanguage = "en"
    targetLanguage = "it"
    pauseBetweenWords = 1.5
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/VideoGenerator/generate" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Check Status
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/VideoGenerator/status/YOUR_PROJECT_ID" `
    -Method Get
```

---

## Expected Response (Generate Video)

```json
{
  "success": true,
  "projectId": "JkGxEoPRF9EgRb32",
  "message": "Video generation started successfully",
  "timestamp": "2024-12-04T10:30:00.000Z",
  "generatedWords": [
    {
      "sourceWord": "cat",
      "targetWord": "قطة"
    },
    {
      "sourceWord": "dog",
      "targetWord": "كلب"
    },
    {
      "sourceWord": "bird",
      "targetWord": "طائر"
    },
    {
      "sourceWord": "fish",
      "targetWord": "سمكة"
    },
    {
      "sourceWord": "horse",
      "targetWord": "حصان"
    }
  ]
}
```

## Expected Response (Status - Processing)

```json
{
  "success": true,
  "status": "running",
  "message": null,
  "createdAt": "2024-12-04T10:30:00.000Z"
}
```

## Expected Response (Status - Complete)

```json
{
  "success": true,
  "status": "done",
  "videoUrl": "https://assets.json2video.com/clients/xxxxx/renders/2024-12-04-12345.mp4",
  "subtitlesUrl": "https://assets.json2video.com/clients/xxxxx/renders/2024-12-04-12345.ass",
  "message": null,
  "createdAt": "2024-12-04T10:30:00.000Z",
  "endedAt": "2024-12-04T10:35:30.000Z",
  "duration": 45.5,
  "size": 5242880,
  "width": 1920,
  "height": 1080,
  "renderingTime": 330
}
```

## Tips

1. **Save the Project ID**: After generating a video, save the `projectId` to check status later
2. **Polling**: Videos typically take 30-60 seconds per word. Poll status every 10-15 seconds
3. **Error Handling**: If status is "error", check the `message` field for details
4. **Language Codes**: Use standard language codes (en, ar, es, fr, etc.)
5. **Word Count**: Start with 3-5 words for testing, increase for production