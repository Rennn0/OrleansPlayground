using StackExchange.Redis;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<AppSettings>().Bind(builder.Configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<SiloSettings>().Bind(builder.Configuration.GetSection("AppSettings:SiloSettings"))
    .ValidateDataAnnotations().ValidateOnStart();

ServiceProvider sProvider = builder.Services.BuildServiceProvider();
AppSettings appSettings = sProvider.GetRequiredService<IOptionsSnapshot<AppSettings>>().Value;
SiloSettings siloSettings = sProvider.GetRequiredService<IOptionsSnapshot<SiloSettings>>().Value;

builder.Environment.EnvironmentName = appSettings.Environment;

builder.UseOrleans(silo =>
{
    silo.UseAdoNetClustering(opt =>
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
            })
        ;

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

    silo.Configure<ClusterOptions>(opt =>
    {
        opt.ClusterId = siloSettings.ClusterId;
        opt.ServiceId = siloSettings.ServiceId;
    });

    if (appSettings.SiloSettings.UseDashboard)
        silo.UseDashboard(opt => { opt.Port = siloSettings.DashboardPort; });

    silo.ConfigureEndpoints(
        siloSettings.SiloPort,
        siloSettings.GatewayPort
    );
});

await builder.Build().RunAsync();