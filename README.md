dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c ApplicationDbContext
dotnet run .\Server\bin\Debug\net8.0\Server.exe /seed  --project Server
