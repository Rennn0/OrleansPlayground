using Azure.Data.Tables;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.UseOrleansClient(client =>
{
    client.UseAzureStorageClustering(opt =>
    {
        opt.TableServiceClient =
            new TableServiceClient(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION") ??
                                   throw new Exception());
    }).UseConnectionRetryFilter((exception, token) => Task.FromResult(true));
});

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();