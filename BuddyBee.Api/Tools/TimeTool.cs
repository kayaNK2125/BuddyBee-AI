using System;

namespace BuddyBee.Api.Tools
{
    public class TimeTool : ITool
    {
        public string Name => "get_time";

        public string Description =>
            "Returns the current time. Optionally specify an IANA timezone (e.g. 'Asia/Kolkata' for India, 'America/New_York' for US Eastern).";

        public Task<ToolResult> ExecuteAsync(
            Dictionary<string, object> arguments)
        {
            string timeZoneId = null;

            if (arguments.TryGetValue("timezone", out var tzObj) && tzObj != null)
            {
                timeZoneId = tzObj.ToString()?.Trim();
            }

            DateTime utcNow = DateTime.UtcNow;
            DateTime resultTime = utcNow;
            string zoneDisplay = "UTC";

            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    resultTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
                    zoneDisplay = timeZoneId;
                }
                catch (TimeZoneNotFoundException)
                {
                    return Task.FromResult(new ToolResult
                    {
                        Success = false,
                        Output = "",
                        Error = $"Unknown timezone: {timeZoneId}. Use IANA IDs like 'Asia/Kolkata', 'America/New_York', 'Europe/London'."
                    });
                }
                catch (InvalidTimeZoneException)
                {
                    return Task.FromResult(new ToolResult
                    {
                        Success = false,
                        Output = "",
                        Error = $"Invalid timezone data for: {timeZoneId}."
                    });
                }
            }

            var output = resultTime.ToString("yyyy-MM-dd HH:mm:ss") + " (" + zoneDisplay + ")";

            return Task.FromResult(new ToolResult
            {
                Success = true,
                Output = output
            });
        }
    }
}