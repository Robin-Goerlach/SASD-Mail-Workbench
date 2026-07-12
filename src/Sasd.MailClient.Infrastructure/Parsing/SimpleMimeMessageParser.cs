using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Parsing;

/// <summary>
/// Einfacher Parser fuer einen robusten Projektstart ohne externe Pakete.
/// Diese Implementierung ist bewusst konservativ:
/// - sie verarbeitet Header sauber,
/// - sie unterstuetzt einfache Single-Part-Mails,
/// - sie kann einfacheres Multipart mit Boundary grob aufloesen,
/// - sie ignoriert komplexe MIME-Spezialfaelle noch.
/// 
/// Spaeter sollte diese Implementierung durch eine MimeKit-basierte Variante ersetzt werden.
/// Fuer den ersten lauffaehigen Systemkern ist sie jedoch sehr gut geeignet.
/// </summary>
public sealed class SimpleMimeMessageParser : IMessageParser
{
    public Task<ParsedMessage> ParseAsync(
        MailAccount account,
        RawMessage rawMessage,
        string rawContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        (Dictionary<string, string> headers, string body) = SplitHeadersAndBody(rawContent);

        ParsedMessage message = new ParsedMessage
        {
            Id = rawMessage.Id,
            AccountId = account.Id,
            HeaderMessageId = GetHeader(headers, "Message-ID"),
            Subject = GetHeader(headers, "Subject"),
            From = ParseAddress(GetHeader(headers, "From")),
            To = ParseAddressList(GetHeader(headers, "To")),
            Cc = ParseAddressList(GetHeader(headers, "Cc")),
            ReplyTo = ParseAddress(GetHeader(headers, "Reply-To")),
            SentAtUtc = TryParseDate(GetHeader(headers, "Date")),
            ReceivedAtUtc = rawMessage.RetrievedAtUtc,
            Headers = headers
        };

        string contentType = GetHeader(headers, "Content-Type");

        if (contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase))
        {
            ParseMultipartBody(contentType, body, message);
        }
        else
        {
            AssignSinglePartBody(contentType, body, message);
        }

        if (string.IsNullOrWhiteSpace(message.TextBody) && !string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            message.TextBody = StripHtml(message.HtmlBody);
        }

        message.NormalizedText = NormalizeText(
            string.IsNullOrWhiteSpace(message.TextBody)
                ? message.HtmlBody
                : message.TextBody);

        return Task.FromResult(message);
    }

    private static (Dictionary<string, string> Headers, string Body) SplitHeadersAndBody(string rawContent)
    {
        string normalizedContent = rawContent.Replace("\r\n", "\n");

        string separator = "\n\n";
        int separatorIndex = normalizedContent.IndexOf(separator, StringComparison.Ordinal);

        string headerText = separatorIndex >= 0
            ? normalizedContent[..separatorIndex]
            : normalizedContent;

        string bodyText = separatorIndex >= 0
            ? normalizedContent[(separatorIndex + separator.Length)..]
            : string.Empty;

        Dictionary<string, string> headers = ParseHeaders(headerText);

        return (headers, bodyText);
    }

    private static Dictionary<string, string> ParseHeaders(string headerText)
    {
        Dictionary<string, string> headers =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        string[] lines = headerText.Split('\n');
        string currentHeaderName = string.Empty;

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Fortgesetzte Headerzeilen beginnen mit Leerzeichen oder Tabulator.
            // Diese werden an den letzten Header angehaengt.
            if ((line.StartsWith(" ") || line.StartsWith("\t")) && !string.IsNullOrWhiteSpace(currentHeaderName))
            {
                headers[currentHeaderName] = $"{headers[currentHeaderName]} {line.Trim()}";
                continue;
            }

            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }

            string headerName = line[..colonIndex].Trim();
            string headerValue = line[(colonIndex + 1)..].Trim();

            headers[headerName] = headerValue;
            currentHeaderName = headerName;
        }

        return headers;
    }

    private static void AssignSinglePartBody(
        string contentType,
        string body,
        ParsedMessage message)
    {
        if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            message.HtmlBody = body.Trim();
        }
        else
        {
            message.TextBody = body.Trim();
        }
    }

    private static void ParseMultipartBody(
        string contentType,
        string body,
        ParsedMessage message)
    {
        string boundary = ExtractBoundary(contentType);

        if (string.IsNullOrWhiteSpace(boundary))
        {
            // Ohne Boundary kann multipart nicht sauber aufgeloest werden.
            // Wir bewahren deshalb wenigstens den Body als Text.
            message.TextBody = body.Trim();
            return;
        }

        string normalizedBody = body.Replace("\r\n", "\n");
        string marker = $"--{boundary}";

        string[] parts = normalizedBody.Split(marker, StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawPart in parts)
        {
            string part = rawPart.Trim();

            if (string.IsNullOrWhiteSpace(part) ||
                string.Equals(part, "--", StringComparison.Ordinal) ||
                part.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            (Dictionary<string, string> partHeaders, string partBody) = SplitHeadersAndBody(part);

            string partContentType = GetHeader(partHeaders, "Content-Type");

            if (partContentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(message.TextBody))
            {
                message.TextBody = partBody.Trim();
            }
            else if (partContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase) &&
                     string.IsNullOrWhiteSpace(message.HtmlBody))
            {
                message.HtmlBody = partBody.Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(message.TextBody) && string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            message.TextBody = body.Trim();
        }
    }

    private static string ExtractBoundary(string contentType)
    {
        Match match = Regex.Match(
            contentType,
            @"boundary\s*=\s*""?(?<boundary>[^"";]+)""?",
            RegexOptions.IgnoreCase);

        return match.Success
            ? match.Groups["boundary"].Value.Trim()
            : string.Empty;
    }

    private static string GetHeader(Dictionary<string, string> headers, string headerName)
    {
        return headers.TryGetValue(headerName, out string? value)
            ? value
            : string.Empty;
    }

    private static MailAddress? ParseAddress(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        Match angleMatch = Regex.Match(input, @"^(?<name>.*)<(?<address>[^>]+)>$");

        if (angleMatch.Success)
        {
            return new MailAddress
            {
                DisplayName = angleMatch.Groups["name"].Value.Trim().Trim('"'),
                Address = angleMatch.Groups["address"].Value.Trim()
            };
        }

        return new MailAddress
        {
            Address = input.Trim()
        };
    }

    private static List<MailAddress> ParseAddressList(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<MailAddress>();
        }

        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseAddress)
            .Where(address => address is not null)
            .Cast<MailAddress>()
            .ToList();
    }

    private static DateTimeOffset? TryParseDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(
            input,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out DateTimeOffset parsed))
        {
            return parsed.ToUniversalTime();
        }

        return null;
    }

    private static string StripHtml(string html)
    {
        string noTags = Regex.Replace(html, "<[^>]+>", " ");
        return NormalizeText(noTags);
    }

    private static string NormalizeText(string text)
    {
        string normalizedLineBreaks = text.Replace("\r\n", "\n").Replace('\r', '\n');
        string collapsedWhitespace = Regex.Replace(normalizedLineBreaks, @"[ \t]+", " ");
        string collapsedEmptyLines = Regex.Replace(collapsedWhitespace, @"\n{3,}", "\n\n");
        return collapsedEmptyLines.Trim();
    }
}
