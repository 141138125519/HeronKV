using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HeronKV;
using HeronKV.Persistence;
using HeronKV.Data.Serialiser;
using HeronKV.Data.Parser;
using HeronKV.CommandHandler;

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

builder.Services.AddSingleton<IRESPParser, RESPParser>();
builder.Services.AddSingleton<IRESPSerialiser, RESPSerialiser>();
builder.Services.AddSingleton<ICommandsHandler, CommandsHandler>();

// Make sure AOF serrvice is set up in sucha a way that its interface can be injected into Server.
builder.Services.AddSingleton<IAOF, AOF>();
builder.Services.AddHostedService(services => (AOF) services.GetService<IAOF>()!);

builder.Services.AddHostedService<Server>();

var host = builder.Build();
host.Run();
