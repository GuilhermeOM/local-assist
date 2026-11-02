using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;

namespace Local.Assist.Application.Abstractions.Behaviors;

public sealed class CustomToolInvoker : IToolInvoker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DefaultToolInvoker _defaultToolInvoker;

    public CustomToolInvoker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _defaultToolInvoker = new DefaultToolInvoker();
    }

    public async Task<ToolResult> InvokeAsync(
        Message.ToolCall toolCall,
        IEnumerable<object> tools,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(toolCall);

        var invocationResult = await _defaultToolInvoker
            .InvokeAsync(toolCall, tools, cancellationToken)
            .ConfigureAwait(false);

        var customTool = invocationResult.Result as ICustomTool ?? throw new InvalidOperationException($"The tool '{toolCall.Function?.Name}' did not return a valid ICustomTool instance.");
        var handlerContract = typeof(ICustomToolHandler<>).MakeGenericType(customTool.GetType());

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var handler = scope.ServiceProvider.GetRequiredKeyedService(handlerContract, customTool.GetType().Name);

            var handleMethod = handlerContract.GetMethod(nameof(ICustomToolHandler<>.HandleAsync))
                ?? throw new InvalidOperationException($"The handler contract '{handlerContract}' has no HandleAsync method.");

            var handleTask = (Task<PromptResult>?)handleMethod.Invoke(
                handler,
                [customTool, cancellationToken]) ?? throw new InvalidOperationException($"The handler for '{customTool.GetType().Name}' returned no task.");

            var customToolHandlerResult = await handleTask.ConfigureAwait(false);

            return new ToolResult(invocationResult.Tool, toolCall, customToolHandlerResult.ToJson());
        }
    }
}
