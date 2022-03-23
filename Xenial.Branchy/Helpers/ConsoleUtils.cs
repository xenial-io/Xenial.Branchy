using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;

namespace Xenial.Branchy.Helpers;

internal static class ConsoleUtils
{

    internal static void HandleError(string? errors)
    {
        if (!string.IsNullOrWhiteSpace(errors))
        {
            AnsiConsole.MarkupLine($"[red]{errors.EscapeMarkup()}[/]");
            Environment.Exit(1);
        }
    }
}
