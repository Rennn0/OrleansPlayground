using Azure.Data.Tables;
using Azure.Storage.Blobs;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<AppSettings>().Bind(builder.Configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<SiloSettings>().Bind(builder.Configuration.GetSection("AppSettings:SiloSettings"))
    .ValidateDataAnnotations().ValidateOnStart();

ServiceProvider sp = builder.Services.BuildServiceProvider();
AppSettings appSettings = sp.GetRequiredService<IOptionsSnapshot<AppSettings>>().Value;
SiloSettings siloSettings = sp.GetRequiredService<IOptionsSnapshot<SiloSettings>>().Value;

builder.Environment.EnvironmentName = appSettings.Environment;

builder.UseOrleans(silo =>
{
#if CLOUD
    silo.UseAzureStorageClustering(opt =>
    {
        opt.TableServiceClient = new TableServiceClient(siloSettings.AzureBlobConnection);
    });

    silo.UseAzureTableReminderService(opt =>
    {
        opt.TableServiceClient = new TableServiceClient(siloSettings.AzureBlobConnection);
    });

    silo.AddAzureBlobGrainStorage(siloSettings.AzureStorage,
        opt => { opt.BlobServiceClient = new BlobServiceClient(siloSettings.AzureBlobConnection); });

#elif LOCAL_INFRASTRUCTURE
    silo.UseAdoNetClustering(opt =>
    {
        opt.Invariant = siloSettings.PgStorageInvatiant;
        opt.ConnectionString = siloSettings.PgStorageConnection;
    });

    silo.UseAdoNetReminderService(opt =>
    {
        opt.Invariant = siloSettings.PgStorageInvatiant;
        opt.ConnectionString = siloSettings.PgStorageConnection;
    });

    silo.AddAdoNetGrainStorage(
        siloSettings.AdoNetStorage,
        opt =>
        {
            opt.Invariant = siloSettings.PgStorageInvatiant;
            opt.ConnectionString = siloSettings.PgStorageConnection;
        });

    silo.AddRedisGrainStorage(siloSettings.RedisStorage,
        opt =>
        {
            opt.ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints = { siloSettings.RedisStorageConnection },
                AbortOnConnectFail = siloSettings.RedisAbortOnConnectFail,
                DefaultDatabase = siloSettings.RedisDbNumber,
                Password = siloSettings.RedisStoragePassword,
                User = siloSettings.RedisStorageUser,
                ConnectRetry = siloSettings.RedisConnectRetry
            };
        });
#endif

    silo.Configure<ClusterOptions>(opt =>
    {
        opt.ClusterId = siloSettings.ClusterId;
        opt.ServiceId = siloSettings.ServiceId;
    });

    // silo.Configure<EndpointOptions>(opt =>
    // {
    //     opt.SiloPort = siloSettings.SiloPort;
    //     opt.GatewayPort = siloSettings.GatewayPort;
    //     opt.AdvertisedIPAddress =
    //         string.IsNullOrEmpty(siloSettings.AdvertiseIpAddress)
    //             ? IPAddress.Loopback
    //             : IPAddress.Parse(siloSettings.AdvertiseIpAddress);
    //     opt.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Loopback, 40_000);
    //     opt.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 50_000);
    // });

    
    if (appSettings.SiloSettings.UseDashboard)
        silo.UseDashboard(opt => { opt.Port = siloSettings.DashboardPort; });
});

await builder.Build().RunAsync();