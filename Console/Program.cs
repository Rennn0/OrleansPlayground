using System.Text.Json;
using Azure.Data.Tables;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<AppSettings>().Bind(builder.Configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<SiloSettings>().Bind(builder.Configuration.GetSection("AppSettings:SiloSettings"))
    .ValidateDataAnnotations().ValidateOnStart();

ServiceProvider sProvider = builder.Services.BuildServiceProvider();
AppSettings appSettings = sProvider.GetRequiredService<IOptionsSnapshot<AppSettings>>().Value;
SiloSettings siloSettings = sProvider.GetRequiredService<IOptionsSnapshot<SiloSettings>>().Value;

builder.UseOrleansClient(client =>
{
    // client.UseAdoNetClustering(opt =>
    // {
    //     opt.ConnectionString =
    //         siloSettings.PgStorageConnection;
    //     opt.Invariant = siloSettings.PgStorageInvatiant;
    // });

    client.UseAzureStorageClustering(opt =>
    {
        opt.TableServiceClient =
            new TableServiceClient(siloSettings.AzureBlobConnection);
    });

    client.Configure<ClusterOptions>(opt => { opt.ClusterId = siloSettings.ClusterId; });
});

using IHost host = builder.Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
while (true)
{
    System.Console.WriteLine("Enter id");
    int id = int.Parse(System.Console.ReadLine() ?? "1");
    IWarehouseGrain grain = client.GetGrain<IWarehouseGrain>(id);
    System.Console.WriteLine("Actions: 1=get 2=create 3=update 4=exit");
    int action = int.Parse(System.Console.ReadLine() ?? "1");
    string location = "";
    string owner = "";
    long capacity = -1;
    switch (action)
    {
        case 1:
            WarehouseModel result = await grain.GetWarehouseAsync(new GetWarehouse());
            logger.LogInformation("res {0}", JsonSerializer.Serialize(result));
            break;
        case 2:
            System.Console.WriteLine("Location");
            location = System.Console.ReadLine() ?? "";
            System.Console.WriteLine("Owner");
            owner = System.Console.ReadLine() ?? "";
            System.Console.WriteLine("Capacity");
            capacity = long.Parse(System.Console.ReadLine() ?? "-1");
            await grain.CreateWarehouseAsync(new CreateWarehouse(location, owner, capacity));
            break;
        case 3:
            System.Console.WriteLine("Location");
            location = System.Console.ReadLine() ?? "";
            System.Console.WriteLine("Owner");
            owner = System.Console.ReadLine() ?? "";
            System.Console.WriteLine("Capacity");
            capacity = long.Parse(System.Console.ReadLine() ?? "-1");
            await grain.UpdateWarehouseAsync(new UpdateWarehouse(location, owner, capacity));
            break;
        case 4:
            Environment.Exit(0);
            break;
    }
}