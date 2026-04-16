using DryMartiniMovies.Web.Components;
using DryMartiniMovies.Client.Services;
using DryMartiniMovies.Web.Services;
using ApexCharts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApexCharts();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var apiBaseAddress = builder.Configuration["ApiBaseAddress"]
    ?? throw new InvalidOperationException("ApiBaseAddress is not configured.");

builder.Services.AddHttpClient<MovieApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
    client.Timeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddSignalR(e => {
    e.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    e.EnableDetailedErrors = true;
});
builder.Services.AddHttpClient<ChatApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
    client.Timeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddScoped<ChatStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
