using System.Data.Common;
using Microsoft.Extensions.AI;

namespace ChatApi.Services.ChatCompletion;

public static class ChatCompletionServiceExtensions
{
    public static void AddChatCompletionService(this IHostApplicationBuilder builder)
    {
        var pipeline = (ChatClientBuilder pipeline) => pipeline
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true);

        builder.AddOllamaChatClient(pipeline);
    }

    public static IServiceCollection AddOllamaChatClient(
        this IHostApplicationBuilder hostBuilder,
        Func<ChatClientBuilder, ChatClientBuilder>? builder = null,
        string? modelName = null)
    {
        if (modelName is null)
        {
            modelName = hostBuilder.Configuration["AI:CHATMODEL"];
            if (string.IsNullOrEmpty(modelName))
            {
                throw new InvalidOperationException($"No {nameof(modelName)} was specified.");
            }
        }

        if (hostBuilder.Configuration.GetValue<string>("AI:Type") is string type and "ollama")
        {
            var connectionString = hostBuilder.Configuration.GetConnectionString(type);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"No connection string named '{type}' was found. Ensure a corresponding Aspire service was registered.");
            }

            var connectionStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            var endpoint = (string?)connectionStringBuilder["endpoint"];

            return hostBuilder.Services.AddOllamaChatClient(modelName, new Uri(endpoint!));
        }

        throw new InvalidOperationException("Couldn't register AddOllamaChatClient.");
    }

    public static IServiceCollection AddOllamaChatClient(
        this IServiceCollection services,
        string modelName,
        Uri? uri = null)
    {
        uri ??= new Uri("http://localhost:11434");

        var chatClientBuilder = services.AddChatClient(serviceProvider =>
        {
            var httpClient = serviceProvider.GetService<HttpClient>() ?? new();
            var chatClient = new OllamaChatClient(uri, modelName, httpClient);
            
            return chatClient;
        });

        // Temporary workaround for Ollama issues
        chatClientBuilder.UsePreventStreamingWithFunctions();

        return services;
    }
}