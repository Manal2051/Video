using LanguageVideoGenerator.Api.Configuration;
using LanguageVideoGenerator.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure settings
builder.Services.Configure<Json2VideoSettings>(
    builder.Configuration.GetSection(Json2VideoSettings.SectionName));
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection(OpenAISettings.SectionName));

// Register HttpClient for Json2Video with Polly retry policies
builder.Services.AddHttpClient<IJson2VideoClient, Json2VideoClient>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    });

// Register services
builder.Services.AddScoped<IWordGeneratorService, WordGeneratorService>();
builder.Services.AddScoped<IVideoGeneratorService, VideoGeneratorService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Language Video Generator API",
        Version = "v1",
        Description = "API for generating language learning videos using Json2Video",
        Contact = new OpenApiContact
        {
            Name = "Language Video Generator",
            Email = "support@example.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in both Development and Production
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Language Video Generator API v1");
    options.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();