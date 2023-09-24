using Pustok.Database.Base;

namespace Pustok.Database.Models
{
    public class UserNotification : IAuditable
    {
        public User RecievingUser { get; set; }
        public int RecievingUserId { get; set; }
        public User SendingUser { get; set; }
        public int SendingUserId { get; set; }
        public int NotificationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Notification Notification { get; set; }
    }
}
