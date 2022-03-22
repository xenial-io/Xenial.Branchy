
using System.Text;

using Spectre.Console;

using static SimpleExec.Command;

try
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

    AnsiConsole.MarkupLine($"[grey]You are currently on branch [silver]{branch}[/][/]");
    AnsiConsole.MarkupLine($"[grey]Press [silver]Ctrl+C[/] to exit at any time.[/]");
    AnsiConsole.WriteLine();

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
        .Where(b => !string.IsNullOrWhiteSpace(b))
        .Select(b => b.Trim())
        .ToArray();

    var needStash = !string.IsNullOrEmpty(changesString);
    var shouldStash = false;
    if (needStash)
    {
        AnsiConsole.MarkupLine($"[yellow]It seams you have [green]{changes.Length} changes[/] in your working copy.[/]");
        shouldStash = AnsiConsole.Confirm("[yellow]Do you want to automatically stash and restore them?[/]");
    }

    return 0;
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}

static void HandleError(string? errors)
{
    if (!string.IsNullOrWhiteSpace(errors))
    {
        AnsiConsole.MarkupLine($"[red]{errors.EscapeMarkup()}[/]");
        Environment.Exit(1);
    }
}
