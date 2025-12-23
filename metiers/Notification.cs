using System;

namespace Sahty.Metiers
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
    }
}
