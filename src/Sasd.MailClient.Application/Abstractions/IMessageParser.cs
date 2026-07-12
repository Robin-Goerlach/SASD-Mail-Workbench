using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Wandelt Rohinhalte in eine fachlich nutzbare Struktur um.
/// Der Parser ist absichtlich vom Abruf und von der GUI entkoppelt.
/// </summary>
public interface IMessageParser
{
    Task<ParsedMessage> ParseAsync(
        MailAccount account,
        RawMessage rawMessage,
        string rawContent,
        CancellationToken cancellationToken);
}
