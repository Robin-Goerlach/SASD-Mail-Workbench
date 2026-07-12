using Sasd.MailWorkbench.Domain.Enums;

namespace Sasd.MailWorkbench.Application.Contracts.Import;

/// <summary>
/// Eingabe für einen providerneutralen Rohmail-Importlauf.
/// </summary>
public sealed record ImportRawMessagesRequest(
    string AccountId,
    ImportMode Mode);
