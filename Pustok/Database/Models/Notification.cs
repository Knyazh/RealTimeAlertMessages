using Pustok.Database.Base;
namespace Pustok.Database.Models
{
    public class Notification : BaseEntity<int>, IAuditable
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<UserNotification> UserNotifications {  get; set; }
    }
}
