public interface ITool
{
    string Name { get; }

    string Description { get; }

    Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments);
}