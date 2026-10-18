namespace Local.Assist.Domain.Services;

public interface IMessageService
{
    Task ProcessAsync(string message, CancellationToken cancellationToken = default);
}
