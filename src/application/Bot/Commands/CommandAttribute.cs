namespace ADAM.Application.Bot.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute(string name, string? usageExample = null, string? description = null) : Attribute
{
    public string Name { get; set; } = name;
    public string? UsageExample { get; set; } = usageExample;
    public string? Description { get; set; } = description;
}