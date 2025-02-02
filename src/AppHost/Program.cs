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

var ollama = builder.AddOllama("ollama")
        .WithImageTag("0.3.14")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume()
    //.WithOpenWebUI()
    ;

var allMinilmModel = ollama.AddModel("all-minilm", "all-minilm");
var llama32Model = ollama.AddModel("llama32", "llama3.2:1b");
var chatApi = builder.AddProject<Projects.ChatApi>("chat-api")
        .WithReference(postgres).WaitFor(postgres)
        .WithEnvironment("AI:Type", "ollama")
        // .WithEnvironment("AI:EMBEDDINGMODEL", "all-minilm")
        .WithEnvironment("AI:CHATMODEL", "llama3.2:1b")
        .WithReference(ollama).WaitFor(allMinilmModel).WaitFor(llama32Model)
    ;

builder.Build().Run();