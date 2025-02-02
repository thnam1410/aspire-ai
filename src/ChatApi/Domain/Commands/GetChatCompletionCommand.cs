namespace ChatApi.Domain.Commands;

public class GetChatCompletionCommand: IRequest<IAsyncEnumerable<string>>
{
    public string Input { get; set; }
}