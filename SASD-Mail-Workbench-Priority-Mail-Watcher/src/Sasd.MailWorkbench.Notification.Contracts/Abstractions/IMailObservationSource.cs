using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Contracts.Abstractions;

/// <summary>
/// Liefert neue, normalisierte Mailbeobachtungen aus der gemeinsamen Mail-Workbench-Infrastruktur.
/// </summary>
/// <remarks>
/// Diese Schnittstelle ist ein Notification-spezifischer Eingangsport und keine zweite
/// IMAP-/POP3-Abstraktion. Ein späterer Adapter muss die bestehenden gemeinsamen Mailzugriffe
/// verwenden und deren Ergebnisse auf <see cref="MailObservation"/> abbilden.
/// </remarks>
public interface IMailObservationSource
{
    /// <summary>Liest seit dem letzten bestätigten Check neu erkannte Beobachtungen.</summary>
    Task<IReadOnlyList<MailObservation>> GetNewObservationsAsync(
        CancellationToken cancellationToken = default);
}
