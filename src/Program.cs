using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KVDB;

Console.WriteLine(@"
    Starting KVDB!
    Good Luck
");


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Server>();

var host = builder.Build();
host.Run();
