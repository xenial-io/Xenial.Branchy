using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer;

using Spectre.Console;
using Spectre.Console.Cli;

using static SimpleExec.Command;
using static Xenial.Branchy.Helpers.ConsoleUtils;

namespace Xenial.Branchy.Commands;

internal class CleanOrphanedBranchesCommand : AsyncCommand<CleanOrphanedBranchesCommand.CleanOrphanedBranchesCommandSettings>
{
    internal class CleanOrphanedBranchesCommandSettings : CommandSettings { }

    private static readonly string[] protectedBranches = new[]
    {
        "main",
        "master",
        "develop"
    };

    public async override Task<int> ExecuteAsync(CommandContext context, CleanOrphanedBranchesCommandSettings settings)
    {
        var (branchesString, errors) = await ReadAsync("git", "branch --merged", encoding: Encoding.UTF8);
        HandleError(errors);
        var (branch, error) = await ReadAsync("git", "branch --show-current", encoding: Encoding.UTF8);
        HandleError(error);

        branch = branch.Trim();

        var branches = branchesString
            .Split('\n')
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Select(b => b.Trim())
            .Where(b => !b.StartsWith('*')) //we don't need the current branch
            .Except(new[] { branch })
            .Except(protectedBranches)
            .ToArray();

        AnsiConsole.MarkupLine($"[grey]You are currently on branch [green]{branch.EscapeMarkup()}[/][/]");
        AnsiConsole.MarkupLine($"[grey]Press [silver]Ctrl+C[/] to exit at any time.[/]");
        AnsiConsole.WriteLine();

        if (branches.Length <= 0)
        {
            AnsiConsole.MarkupLine($"[grey]There are currently [silver]no orphaned branches[/]. So there is nothing to cleanup [silver]~(* . *)~[/][/]");
            return 0;
        }

        var choice = new MultiSelectionPrompt<string>()
                .Title("Which [green]orphaned branches[/] do you want to delete?")
                .NotRequired() // Not required to have a favorite fruit
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a branch, " +
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(
                    branches
        );

        foreach (var b in branches)
        {
            choice.Select(b);
        }

        var orphanedBranches = AnsiConsole.Prompt(choice);

        if (orphanedBranches is null || orphanedBranches.Count <= 0)
        {
            AnsiConsole.MarkupLine($"[grey]There is nothing to delete [silver]because nothing was selected[/]. So there is nothing to cleanup [silver]~(* . *)~[/][/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"You are about to delete [red]{"branch".ToQuantity(orphanedBranches.Count)}[/] ([green]local[/])");

        AnsiConsole.WriteLine();

        foreach (var orphanedBranch in orphanedBranches)
        {
            AnsiConsole.MarkupLine($"[grey strikethrough]\t{orphanedBranch}[/]");
        }

        AnsiConsole.WriteLine();

        var shouldDelete = AnsiConsole.Confirm("Are you sure?");

        if (!shouldDelete)
        {
            AnsiConsole.MarkupLine($"[grey]There is nothing to delete [silver]because you decided against[/]. So there is nothing to cleanup [silver]~(* . *)~[/][/]");
            return 0;
        }

        foreach (var orphanedBranch in orphanedBranches)
        {
            AnsiConsole.MarkupLine($"Deleting local branch [grey strikethrough]\t{orphanedBranch}[/]");
            await RunAsync("git", $"branch -d {orphanedBranch}");
        }

        AnsiConsole.MarkupLine($"[grey]Deleted [green]{orphanedBranches.Count}[/] local branches.[/]");

        AnsiConsole.MarkupLine("[green]\\(^ . ^)/[/]");

        return 0;
    }

}
