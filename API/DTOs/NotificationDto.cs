using System;

namespace Sahty.API.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
