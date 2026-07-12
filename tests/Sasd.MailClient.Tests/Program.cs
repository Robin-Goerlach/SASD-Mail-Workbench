using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sasd.MailClient.Tests.Suites;
using Sasd.MailClient.Tests.Testing;

internal static class Program
{
    /// <summary>
    /// Kleiner Test-Runner fuer den ersten Projektwurf.
    /// Er fuehrt alle registrierten Testfaelle sequenziell aus und beendet sich mit Exit-Code 1,
    /// sobald mindestens ein Test fehlschlaegt.
    /// </summary>
    private static async Task<int> Main(string[] args)
    {
        List<TestCaseDefinition> testCases = new List<TestCaseDefinition>();
        testCases.AddRange(ParserTests.GetCases());
        testCases.AddRange(StorageTests.GetCases());
        testCases.AddRange(UseCaseTests.GetCases());

        Console.WriteLine("=== SASD MailClient Testlauf ===");
        Console.WriteLine($"Gefundene Testfaelle: {testCases.Count}");
        Console.WriteLine();

        int passed = 0;
        int failed = 0;

        foreach (TestCaseDefinition testCase in testCases)
        {
            Console.Write($"[RUN ] {testCase.Name} ... ");

            try
            {
                await testCase.ExecuteAsync();
                passed++;
                WriteColoredLine("OK", ConsoleColor.Green);
            }
            catch (Exception exception)
            {
                failed++;
                WriteColoredLine("FAIL", ConsoleColor.Red);
                Console.WriteLine($"       {exception.Message}");
                Console.WriteLine($"       {exception.StackTrace}");
                Console.WriteLine();
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Zusammenfassung ===");
        Console.WriteLine($"Bestanden : {passed}");
        Console.WriteLine($"Fehlgeschlagen : {failed}");

        return failed == 0 ? 0 : 1;
    }

    private static void WriteColoredLine(string text, ConsoleColor color)
    {
        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previousColor;
    }
}
