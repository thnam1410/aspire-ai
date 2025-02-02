using System.Runtime.CompilerServices;
using ChatApi.Domain.Commands;
using Microsoft.Extensions.AI;
using Shared.Endpoint;

namespace ChatApi.UseCases.Chat;

//Track this: https://github.com/dotnet/aspnetcore/issues/50501

public class GetChatCompletionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("chat", async (
            GetChatCompletionCommand command, 
            ISender sender, 
            ILogger<GetChatCompleteQueryHandler> logger,
            HttpResponse response) =>
        {
            response.ContentType = "text/plain"; // Use "text/event-stream" for SSE
            response.StatusCode = 200;

            var stream = await sender.Send(command);
            await foreach (var chunk in stream)
            {
                logger.LogInformation("Received chunk text: {Text}", chunk);
                await response.WriteAsync(chunk);
                await response.Body.FlushAsync(); // Ensure real-time streaming
            }
        });
    }
}

internal class GetChatCompletionCommandValidator : AbstractValidator<GetChatCompletionCommand>
{
    public GetChatCompletionCommandValidator()
    {
        RuleFor(x => x.Input).NotEmpty();
    }
}

internal class GetChatCompleteQueryHandler(
    IChatClient chatClient
) : IRequestHandler<GetChatCompletionCommand, IAsyncEnumerable<string>>
{
    public Task<IAsyncEnumerable<string>> Handle(GetChatCompletionCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(StreamResponse(cancellationToken));

        async IAsyncEnumerable<string> StreamResponse([EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (var chunkDto in chatClient.CompleteStreamingAsync(request.Input, cancellationToken: ct))
            {
                yield return chunkDto.ToString();
            }
        }
    }
}