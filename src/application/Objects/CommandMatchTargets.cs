namespace ADAM.Application.Objects;

public class CommandMatchTargets
{
    public required HashSet<string> CommandTargets { get; set; }
    public HashSet<string>? SubcommandTargets { get; set; }
}