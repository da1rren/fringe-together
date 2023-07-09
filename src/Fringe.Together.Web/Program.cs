using Azure.Storage.Blobs;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Fringe.Together.Web;
using Fringe.Together.Web.Data;
using Fringe.Together.Web.Models;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<CosmosClient>(_ => new CosmosClient(
    accountEndpoint: builder.Configuration["COSMOS_ENDPOINT"]!,
    authKeyOrResourceToken: builder.Configuration["COSMOS_KEY"]!,
    new CosmosClientOptions 
    { 
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    })
);

builder.Services
    .AddBlazorise( options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons()
    .AddBlazoredLocalStorage();

builder.Services
    .AddSingleton<AppState>()
    .AddSingleton(_ => new BlobServiceClient(builder.Configuration["AZURE_STORAGE_CONNECTION_STRING"]))
    .AddScoped<ShowService>()
    .AddScoped<AvailabilityService>()
    .AddScoped<ProfileService>()
    .AddScoped<InterestedService>()
    .AddHttpClient()
    .AddHttpClient(WellKnown.Http.Availability, client =>
    {
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
