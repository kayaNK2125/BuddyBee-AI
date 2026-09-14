using BuddyBee.Api.Configuration;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Provider.Services;
using BuddyBee.Api.Services;
using BuddyBee.Api.Tools;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.Configure<FishAudioSettings>(
    builder.Configuration.GetSection("FishAudio"));

builder.Services.AddSingleton<MongoDbService>();
// Add services to the container.

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp
        .GetRequiredService<
            Microsoft.Extensions.Options.IOptions<MongoDbSettings>>()
        .Value;

    var client = new MongoClient(settings.ConnectionString);

    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BuddyBeeFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ITool, TimeTool>();
builder.Services.AddScoped<ITool, CalculatorTool>();
builder.Services.AddScoped<ITool, SearchTool>();

builder.Services.AddScoped<ToolRegistry>();

builder.Services.AddScoped<MathEngine>();
builder.Services.AddScoped<MathExpressionParser>();

builder.Services.AddHttpClient<ISearchService, TavilySearchService>();

builder.Services.AddScoped<CalculatorTool>();

builder.Services.AddScoped<GeminiProvider>();
builder.Services.AddScoped<OpenAIProvider>();

builder.Services.AddScoped<IMemoryService, MemoryService>();

builder.Services.AddScoped<IAIService, AIRouter>();

builder.Services.AddHttpClient<IVoiceService, FishAudioVoiceService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("BuddyBeeFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
