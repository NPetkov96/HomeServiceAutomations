using System.Text.Json.Serialization;

public class NotificationBody
{
    [JsonPropertyName("message")]
    public AndroidMessage Message { get; set; }
}

public class AndroidMessage
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = "all";

    [JsonPropertyName("notification")]
    public NotificationContent Notification { get; set; }

    [JsonPropertyName("android")]
    public Android Android { get; set; } = new();

    [JsonPropertyName("data")]
    public Dictionary<string, string> Data { get; set; } = new()
    {
        { "navigate", "details" },
        { "click_action", "FLUTTER_NOTIFICATION_CLICK" }
    };
}

public class NotificationContent
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }
}

public class Android
{
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "high";
}
