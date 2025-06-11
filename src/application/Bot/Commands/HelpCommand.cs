using System.Text;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

public class HelpCommand : Command
{
    private static IEnumerable<ICommand>? _commands;

    protected override async Task HandleCommandAsync(ITurnContext context, string[] cmdParts, CancellationToken ct)
    {
        try
        {
            if (_commands is null)
                throw new Exception("Commands aren't initialized!");

            var sb = new StringBuilder();
            foreach (var command in _commands)
            {
                sb.Append($"""
                           **{command.GetCommandName()}** => {command.GetCommandUsageExample()}
                           {command.GetCommandDescription()}
                           """)
                    .AppendLine();
            }

            await context.SendActivityAsync(
                MessageFactory.Text(sb.ToString()), ct
            );
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

    public static bool IsCommandsCacheInitialized() => _commands is not null;
    public static void InitCommandsCache(IEnumerable<ICommand> commands) => _commands = commands;
}