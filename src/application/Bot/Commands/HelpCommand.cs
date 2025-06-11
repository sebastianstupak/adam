using System.Reflection;
using System.Text;
using ADAM.Application.Objects;
using Microsoft.Bot.Builder;

namespace ADAM.Application.Bot.Commands;

[Command("Help", "@adam help", "Shows this message.")]
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
                    if (Attribute.GetCustomAttribute(command.GetType(), typeof(CommandAttribute))
                        is not CommandAttribute commandAttribute)
                    {
                        throw new Exception($"Command doesn't have the {typeof(CommandAttribute)} attribute");
                    }

                    sb.Append($"""
                               **{commandAttribute.Name}** => {commandAttribute.UsageExample}
                               """)
                        .AppendLine($"""

                                     {commandAttribute.Description}
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

    public override CommandMatchTargets GetCommandMatchTargets() => new()
    {
        CommandTargets = [CommandConstants.Help],
        SubcommandTargets = null
    };

    public static bool IsCommandsCacheInitialized() => _commands is not null;
    public static void InitCommandsCache(IEnumerable<ICommand> commands) => _commands = commands;
}