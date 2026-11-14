using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Local.Assist.Infrastructure.HttpClients.DelegateHandlers;

internal sealed class HttpRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpRequestLoggingHandler> _logger;

    public HttpRequestLoggingHandler(ILogger<HttpRequestLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation(
                "HTTP {HttpMethod} {RequestUri} responded {StatusCode} in {ElapsedMilliseconds} ms",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "HTTP {HttpMethod} {RequestUri} was cancelled after {ElapsedMilliseconds} ms",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "HTTP {HttpMethod} {RequestUri} failed after {ElapsedMilliseconds} ms",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
