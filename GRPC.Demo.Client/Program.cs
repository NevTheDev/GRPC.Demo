using GRPC.Demo.Client;
using GrpcDemo.Services;

IHost host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddGrpcClient<ReaderService.ReaderServiceClient>(options => 
        {
            options.Address = new Uri("https://localhost:7272/");
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

