
using Spectre.Console;
using Spectre.Console.Cli;

using Xenial.Branchy.Commands;

try
{
    Console.CancelKeyPress += (s, e) => Environment.Exit(0);

    var app = new CommandApp<SwitchBranchCommand>();

    app.Configure(c =>
    {
        c.SetApplicationName("branchy");
        c.ValidateExamples();

        c.AddCommand<CleanOrphanedBranchesCommand>("cleanup");
        //c.AddCommand<Xenial.Cli.Commands.ModelCommand>("model");
    });

    return app.Run(args);

}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}

