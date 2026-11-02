using Local.Assist.Domain.ValueObjects;

namespace Local.Assist.Application.Abstractions.Messaging;

public interface ICustomToolHandler<TCustomTool> where TCustomTool : ICustomTool
{
    Task<PromptResult> HandleAsync(TCustomTool tool, CancellationToken cancellationToken = default);
}
