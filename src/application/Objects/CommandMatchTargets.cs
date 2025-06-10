namespace ADAM.Application.Objects;

public class CommandMatchTargets
{
    public required HashSet<string> Targets { get; set; }
    public HashSet<string>? SubcommandTargets { get; set; }
}