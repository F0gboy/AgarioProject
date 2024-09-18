using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private static List<MessageLog> _logs = new List<MessageLog>();

    [HttpPost]
    public IActionResult LogMessage([FromForm] string User, [FromForm] string Message, [FromForm] DateTime Timestamp)
    {
        var log = new MessageLog { User = User, Message = Message, Timestamp = Timestamp };
        _logs.Add(log);
        Console.WriteLine($"Log received: {log.User}: {log.Message} at {log.Timestamp}");
        return Ok();
    }

    [HttpGet]
    public IActionResult GetAllLogs()
    {
        return Ok(_logs);
    }
}

public class MessageLog
{
    public string User { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
