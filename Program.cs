using BuddyBee.Api.Configuration;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Provider.Services;
using BuddyBee.Api.Services;
using BuddyBee.Api.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<MongoDbService>();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options => //Let this frontend send requests to my API
{
    options.AddPolicy("BuddyBeeFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
//builder.Services.AddScoped<IAIService, GeminiProvider>(); //if part of project want to use IAIService, it will use AIService implementation (Dependency Injection)
builder.Services.AddScoped<ITool, TimeTool>();
builder.Services.AddScoped<MathEngine>();
builder.Services.AddScoped<ITool, CalculatorTool>();
builder.Services.AddScoped<ToolRegistry>();


builder.Services.AddScoped<GeminiProvider>();
builder.Services.AddScoped<OpenAIProvider>();

builder.Services.AddScoped<IAIService, AIRouter>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
