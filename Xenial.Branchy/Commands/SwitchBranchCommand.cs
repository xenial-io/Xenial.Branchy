using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectre.Console;
using Spectre.Console.Cli;

using static SimpleExec.Command;
using static Xenial.Branchy.Helpers.ConsoleUtils;

namespace Xenial.Branchy.Commands;

internal class SwitchBranchCommand : AsyncCommand<SwitchBranchCommand.SwitchBranchCommandSettings>
{

    internal class SwitchBranchCommandSettings : CommandSettings { }

    public async override Task<int> ExecuteAsync(CommandContext context, SwitchBranchCommandSettings settings)
    {
        var (branchesString, errors) = await ReadAsync("git", "branch", encoding: Encoding.UTF8);
        HandleError(errors);
        var (branch, error) = await ReadAsync("git", "branch --show-current", encoding: Encoding.UTF8);
        HandleError(error);

        branch = branch.Trim();

        var branches = branchesString
            .Split('\n')
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Select(b => b.Trim())
            .Where(b => !b.StartsWith('*')) //we don't need the current branch
            .ToArray();

        AnsiConsole.MarkupLine($"[grey]You are currently on branch [green]{branch.EscapeMarkup()}[/][/]");
        AnsiConsole.MarkupLine($"[grey]Press [silver]Ctrl+C[/] to exit at any time.[/]");
        AnsiConsole.WriteLine();

        if (branches.Length <= 0)
        {
            AnsiConsole.MarkupLine($"[grey]There is currently [silver]no other branches[/]. So there is nothing to switch to [silver]~(* . *)~[/][/]");
            return 0;
        }

        var newBranch = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("To which branch do you want to [green]change[/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .AddChoices(branches)
        );

        AnsiConsole.MarkupLine($"[grey]Switching to branch [green]{newBranch.EscapeMarkup()}[/][/]");

        var (changesString, changesError) = await ReadAsync("git", "status --porcelain", encoding: Encoding.UTF8);
        HandleError(changesError);

        var changes = changesString
            .Split('\n')
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToArray();

        var needStash = changes.Length > 0;
        var shouldStash = false;
        if (needStash)
        {
            AnsiConsole.MarkupLine($"[yellow]It seams you have [green]{changes.Length} changes[/] in your working copy.[/]");
            shouldStash = AnsiConsole.Confirm("[yellow]Do you want to automatically stash and restore them?[/]");
        }

        shouldStash = needStash && shouldStash;

        if (shouldStash)
        {
            AnsiConsole.MarkupLine($"[grey]Stashing changes...[/]");
            AnsiConsole.WriteLine();
            await RunAsync("git", "stash");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Stashed changes.[/]");
        }

        AnsiConsole.MarkupLine($"[grey]Switching to branch [silver]{newBranch.EscapeMarkup()}[/]...[/]");
        AnsiConsole.WriteLine();
        await RunAsync("git", $"checkout {newBranch}");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Switched to branch [silver]{newBranch.EscapeMarkup()}[/].[/]");

        if (shouldStash)
        {
            AnsiConsole.MarkupLine($"[grey]Popping stash.[/]");
            AnsiConsole.WriteLine();
            await RunAsync("git", "stash pop");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Popped stash.[/]");
        }

        AnsiConsole.MarkupLine($"[grey][green]Switched[/] to branch [green]{newBranch.EscapeMarkup()}[/][/]");

        AnsiConsole.MarkupLine("[green]\\(^ . ^)/[/]");

        return 0;
    }
}
