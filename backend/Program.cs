using OnboardingAssistant.Services;

var builder = WebApplication.CreateBuilder(args);

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

// Register our services
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<EmbeddingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS - allow all origins for now
app.UseCors("AllowAll");
Console.WriteLine("Using permissive CORS policy (AllowAll) - all origins allowed");

app.UseAuthorization();

app.MapControllers();

app.Run();
