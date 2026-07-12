namespace Sasd.MailWorkbench.Application.Contracts.Import;

/// <summary>
/// Erklärbarer Fehler eines einzelnen Importobjekts.
/// </summary>
public sealed record ImportRunError(
    string SourceKey,
    string Code,
    string Message);
