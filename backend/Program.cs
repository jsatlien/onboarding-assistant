using Microsoft.Extensions.Logging;
using OnboardingAssistant.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policies
builder.Services.AddCors(options =>
{
    // Development policy - allows all origins
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Production policy - configurable from appsettings.json
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                         new[] { "https://localhost:3000" }; // Fallback
    
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Enable if you need cookies/auth
    });
});

// Configure OpenAI settings from appsettings.json
builder.Services.Configure<OpenAISettings>(options => {
    options.ApiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey configuration is missing");
    options.AssistantId = builder.Configuration["OpenAI:AssistantId"] ?? throw new ArgumentNullException("OpenAI:AssistantId configuration is missing");
    options.VerboseLogging = builder.Configuration.GetValue<bool>("OpenAI:VerboseLogging");
});

// Register our services
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<RouteContextService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comment out HTTPS redirection for local development
// app.UseHttpsRedirection();

// Use CORS - allow all origins for now
app.UseCors("AllowAll");
Console.WriteLine("Using permissive CORS policy (AllowAll) - all origins allowed");

app.UseAuthorization();

app.MapControllers();

app.Run();
