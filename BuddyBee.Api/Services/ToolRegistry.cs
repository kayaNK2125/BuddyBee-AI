public class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools;

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        _tools = tools.ToDictionary(
            tool => tool.Name);
    }

    public ITool? GetTool(string name)
    {
        _tools.TryGetValue(name, out var tool);

        return tool;
    }
}