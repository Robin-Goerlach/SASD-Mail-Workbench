namespace Sasd.MailWorkbench.ExtensionModel;

/// <summary>
/// Marker- und Metadatenvertrag für spätere vertrauenswürdige interne Erweiterungen.
/// Ein dynamischer Plugin-Lader ist ausdrücklich nicht Teil von Milestone 0.3.1.
/// </summary>
public interface IMessageProcessorExtension
{
    ExtensionDescriptor Descriptor { get; }
}
