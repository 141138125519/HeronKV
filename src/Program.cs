using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HeronKV;
using src;

Console.WriteLine(@"
    Starting HeronKV!
    Good Luck
");

/*
 
To test this locally on windows:
- wsl to use ubutu
- then redis-cli -h 172.18.0.1 -p 6379 should connect
to this server when hosted at 0.0.0.0:6379
 
 */


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<RESPParser>();
builder.Services.AddSingleton<RESPSerialiser>();

builder.Services.AddHostedService<Server>();

var host = builder.Build();
host.Run();
