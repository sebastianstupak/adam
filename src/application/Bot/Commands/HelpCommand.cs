using System.Text;
using ADAM.Application.Objects;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class HelpCommand : Command
{
    private static IEnumerable<ICommand>? _commands;
    private static Microsoft.Bot.Schema.Activity? _helpMessage;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            if (_commands is null)
                throw new Exception("Commands aren't initialized!");

            if (_helpMessage is null)
            {
                var sb = new StringBuilder();
                foreach (var command in _commands)
                {
                    sb.Append($"""
                               **{command.GetCommandName()}** => {command.GetCommandUsageExample()}
                               """)
                        .AppendLine($"""

                                     {command.GetCommandDescription()}
                                     """)
                        .AppendLine();
                }

                _helpMessage = MessageFactory.Text(sb.ToString());
            }

            await context.SendActivityAsync(_helpMessage, ct);
        }
        catch (Exception e)
        {
            await context.SendActivityAsync(
                MessageFactory.Text($"❌ Error printing help: {e.Message}"), ct
            );
        }
    }

    public override string GetCommandName() => "Help";
    public override string GetCommandUsageExample() => "@adam help";
    public override string GetCommandDescription() => "Shows this message.";

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        Targets = [CommandConstants.Help],
        SubcommandTargets = null
    };

    public static bool IsCommandsCacheInitialized() => _commands is not null;
    public static void InitCommandsCache(IEnumerable<ICommand> commands) => _commands = commands;
}