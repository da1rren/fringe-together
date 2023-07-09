using Azure.Storage.Blobs;
using Fringe.Together.Api;
using Fringe.Together.Api.Queries;
using Fringe.Together.Api.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddTypeExtension<ShowExtensions>();

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
    .AddSingleton(_ => new BlobServiceClient(builder.Configuration["AZURE_STORAGE_CONNECTION_STRING"]))
    .AddScoped<ShowService>()
    .AddScoped<AvailabilityService>()
    .AddHttpClient()
    .AddHttpClient(WellKnown.Http.Availability, client =>
    {
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    });

var app = builder.Build();

app.MapGraphQL();

app.Run();
