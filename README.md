# HeronKV

A Redis like in-memory key value data store built in C#

- Makes use of a TCP socket server to listen for connections
- Implements a subset of RESP
- Uses a simple Append Only File for data persistence
- Currently uses a dictionary to hold data in memory (this will soon be replaced)



### To run on windows

- Clone repository
- Then either run through Visual Studio or from command line:
```
dotnet run
```

Currently defaults to listen on 0.0.0.0:6379, this should allow the redis-cli to connect to it (To make use of redis-cli on windows requires [WSL2](https://redis.io/docs/latest/operate/oss_and_stack/install/install-redis/install-redis-on-windows/)) 



### TODO

- Improve this readme (especially the above "instructions")
- Need to add proper error handling
- Tidy Server.cs
- Replace Dictionary as data store
- Allow setting of ip address / port from appsettings
- Add support for more RESP (Add COMMAND so user can more easily find all currently supported commands)

