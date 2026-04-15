using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Neo4j;
using DryMartiniMovies.Infrastructure.Repositories;
using DryMartiniMovies.Infrastructure.Services;
using DryMartiniMovies.Application.Services;
using Scalar.AspNetCore;
using OpenAI.Chat;
using OpenAI;
using System.ClientModel;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json").AddJsonFile("appsettings.database.json", optional: true, reloadOnChange: true);
builder.Services.AddSingleton<ChatClient>(serviceProvider =>
{
    var apiKey = builder.Configuration["AiSettings:ApiKey"];
    var model = builder.Configuration["AiSettings:Model"];
    var endpoint = builder.Configuration["AiSettings:Endpoint"];

    ChatClient client;
    if (string.IsNullOrEmpty(endpoint))
    {
        client = new ChatClient(model, new ApiKeyCredential(apiKey));
    }
    else
    {
        client = new ChatClient(
            model,
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
        );
    }
    return client;
});
// dotnet 10 builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); //dotnet 9
builder.Services.AddSwaggerGen(); //dotnet 9
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<Neo4jContext>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddSingleton<ITmdbService, TmdbService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   //dotnet 10 app.MapOpenApi();
    app.UseSwagger(); //dotnet 9
    app.UseSwaggerUI(); //dotnet 9
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();




