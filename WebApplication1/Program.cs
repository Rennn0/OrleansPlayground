using Azure.Data.Tables;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orleans API",
        Version = "v1",
        Description = "API for Orleans client"
    });
});

builder.UseOrleansClient(client =>
{
    client.UseAzureStorageClustering(opt =>
    {
        opt.TableServiceClient =
            new TableServiceClient(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION") ??
                                   throw new Exception());
    }).UseConnectionRetryFilter(async (exception, token) =>
    {
        await Task.Delay(TimeSpan.FromSeconds(10), token);
        return true;
    });
});

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Orleans API v1"); });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();