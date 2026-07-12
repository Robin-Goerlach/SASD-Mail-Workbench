using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Persistiert den unveraenderten Rohinhalt einer Nachricht.
/// Die bewusste Trennung von Rohdaten und geparster Sicht ist zentral fuer Reprocessing.
/// </summary>
public interface IRawMessageStore
{
    Task<string> SaveAsync(
        RawMessage message,
        string rawContent,
        CancellationToken cancellationToken);

    Task<string> LoadAsync(
        RawMessage message,
        CancellationToken cancellationToken);
}
