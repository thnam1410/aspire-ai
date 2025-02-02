var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres("postgresql")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    // .WithHealthCheck()
    .WithPgWeb();
var postgres = postgresql.AddDatabase("postgres");

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    // .WithHealthCheck()
    .WithRedisCommander();

var chatApi = builder.AddProject<Projects.ChatApi>("chat-api")
    .WithReference(postgres).WaitFor(postgres);

builder.Build().Run();