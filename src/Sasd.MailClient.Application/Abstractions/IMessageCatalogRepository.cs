using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Verwaltet den lokalen Nachrichtenindex, geparste Nachrichten und Prozessorergebnisse.
/// Im aktuellen Projektstand wird dies ueber SQLite umgesetzt, damit die lokale Persistenz
/// bereits frueh stabil, transaktionssicher und spaeter gut erweiterbar ist.
/// </summary>
public interface IMessageCatalogRepository
{
    Task<bool> ExistsByExternalKeyAsync(
        string accountId,
        string externalKey,
        CancellationToken cancellationToken);

    Task SaveRawMessageAsync(
        RawMessage message,
        CancellationToken cancellationToken);

    Task<RawMessage?> GetRawMessageAsync(
        string messageId,
        CancellationToken cancellationToken);

    Task SaveParsedMessageAsync(
        ParsedMessage message,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ParsedMessage>> ListParsedMessagesAsync(
        CancellationToken cancellationToken);

    Task SaveProcessingResultAsync(
        ProcessingResult result,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProcessingResult>> ListProcessingResultsAsync(
        string messageId,
        CancellationToken cancellationToken);
}
