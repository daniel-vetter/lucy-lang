using System;

namespace Lucy.DebugUI.Pages.Model
{
    public class Entry
    {
        DateTimeOffset Send { get; set; }
        public string Method { get; set; } = "";
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; } = "";
        public Direction Direction { get; set; }
    }

    public class RequestResponseEntry : Entry
    {
        public long Id { get; set; }
        public RequestResponse? Result { get; set; }
    }

    public class NotificationEntry : Entry
    {
    }

    public class RequestResponse
    {
        public DateTimeOffset Timestamp { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public enum Direction
    {
        Incomming,
        Outgoing
    }
}
