using Local.Assist.Infrastructure.HttpClients.Configuration;
using Local.Assist.Infrastructure.Resiliences;
using Polly;
using Polly.Registry;

namespace Local.Assist.Infrastructure.HttpClients.DelegateHandlers;

internal sealed class ResilienceDelegateHandler : DelegatingHandler
{
    private readonly ResiliencePipeline _retryPipeline;
    private readonly ResiliencePipeline _circuitBreakerPipeline;

    public ResilienceDelegateHandler(
        ResiliencePipelineProvider<string> resiliencePipelineProvider,
        HttpClientConfiguration httpClientConfiguration)
    {
        _retryPipeline = resiliencePipelineProvider
            .GetPipeline(ResilienceNames.HttpClientRetryPolicy(httpClientConfiguration.Name));

        _circuitBreakerPipeline = resiliencePipelineProvider
            .GetPipeline(ResilienceNames.HttpClientCircuitBreakerPolicy(httpClientConfiguration.Name));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _retryPipeline.ExecuteAsync(async retryToken =>
        {
            using var clonedRequest = await CloneRequestAsync(request);

            return await _circuitBreakerPipeline.ExecuteAsync(async circuitBreakerToken =>
            {
                return await base.SendAsync(clonedRequest, circuitBreakerToken);
            }, retryToken);
        }, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }
}
