public class TimeTool : ITool
{
    public string Name => "get_time";

    public string Description =>
        "Returns the current server time.";

    public Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments)
    {
        var time = DateTime.Now.ToString(
            "yyyy-MM-dd HH:mm:ss");

        return Task.FromResult(
            new ToolResult
            {
                Success = true,
                Output = time
            });
    }
}