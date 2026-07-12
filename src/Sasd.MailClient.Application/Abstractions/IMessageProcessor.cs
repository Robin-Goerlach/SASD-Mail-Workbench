using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Ein Verarbeitungsschritt, der auf bereits geparste Nachrichten arbeitet.
/// Prozessoren sollen weder Mailquellen noch GUI kennen.
/// </summary>
public interface IMessageProcessor
{
    string Name { get; }

    Task<ProcessingResult> ProcessAsync(
        ParsedMessage message,
        CancellationToken cancellationToken);
}
