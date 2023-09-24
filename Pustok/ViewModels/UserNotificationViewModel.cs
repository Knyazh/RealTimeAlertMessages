using Pustok.Database.Models;
using System.Diagnostics.CodeAnalysis;

namespace Pustok.ViewModels;

public class UserNotificationViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int[] UserIds { get; set; }
    public List<User> Users { get; set; }
}
