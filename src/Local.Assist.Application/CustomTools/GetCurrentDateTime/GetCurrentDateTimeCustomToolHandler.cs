using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Application.CustomTools.GetCurrentDateTime;
using Local.Assist.Domain.ValueObjects;

internal sealed class GetCurrentDateTimeCustomToolHandler : ICustomToolHandler<GetCurrentDateTimeCustomTool>
{
    public Task<PromptResult> HandleAsync(GetCurrentDateTimeCustomTool tool, CancellationToken cancellationToken)
    {
        return Task.FromResult(PromptResult.Success($"Current date and time: {DateTime.Now}"));
    }
}
