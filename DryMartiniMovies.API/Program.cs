using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Infrastructure.Neo4j;
using DryMartiniMovies.Infrastructure.Repositories;
using DryMartiniMovies.Infrastructure.Services;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json").AddJsonFile("appsettings.database.json", optional: true, reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<Neo4jContext>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddSingleton<TmdbService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();




