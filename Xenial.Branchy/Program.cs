
using System.Globalization;

using Spectre.Console;
using Spectre.Console.Cli;

using Xenial.Branchy.Commands;

try
{
    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

    Console.CancelKeyPress += (s, e) => Environment.Exit(0);

    var app = new CommandApp<SwitchBranchCommand>();

    app.Configure(c =>
    {
        c.SetApplicationName("branchy");
        c.ValidateExamples();

        c.AddCommand<CleanOrphanedBranchesCommand>("cleanup");
    });

    return app.Run(args);

}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}

