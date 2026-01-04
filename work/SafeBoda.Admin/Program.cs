using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using SafeBoda.Admin;
using SafeBoda.Admin.Services;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Blazored LocalStorage for persistent token storage
builder.Services.AddBlazoredLocalStorage();

// Register a single HttpClient for the API
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri("http://localhost:5228") }; // API base URL
    return new ApiClient(client);
});

// Register AuthService
builder.Services.AddScoped<AuthService>();

// Optional: general-purpose HttpClient for frontend requests
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5007") });

// Register other app services
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<RiderService>();
builder.Services.AddScoped<DriverService>();

// Build the host
var host = builder.Build();

// Initialize AuthService to load any token from localStorage into HttpClient
var auth = host.Services.GetRequiredService<AuthService>();
await auth.InitializeAsync();

// Run the application
await host.RunAsync();
