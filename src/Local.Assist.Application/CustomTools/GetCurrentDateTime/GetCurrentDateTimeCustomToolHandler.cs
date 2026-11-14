using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Domain.ValueObjects;

namespace Local.Assist.Application.CustomTools.GetCurrentDateTime;

internal sealed class GetCurrentDateTimeCustomToolHandler : ICustomToolHandler<GetCurrentDateTimeCustomTool>
{
    public Task<PromptResult> HandleAsync(GetCurrentDateTimeCustomTool tool, CancellationToken cancellationToken)
    {
        return Task.FromResult(PromptResult.Success($"Current date and time: {DateTime.Now}"));
    }
}
