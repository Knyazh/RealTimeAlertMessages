using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pustok.Database;
using Pustok.Database.Models;
using Pustok.Hubs;
using Pustok.Services.Concretes;
using Pustok.ViewModels;

namespace Pustok.Areas.Admin.Controllers;

[Authorize]
[Route("Manage/Notifications")]
[Area("Manage")]
public class SendNotificationsController : Controller
{
    private readonly PustokDbContext _pustokDbContext;
    private readonly UserService _userService;
    private readonly OnlineUserTracker _onlineUserTracker;
    private readonly IHubContext<AlertMessageHub> _logger;

    public SendNotificationsController(PustokDbContext pustokDbContext, UserService userService, OnlineUserTracker onlineUserTracker, IHubContext<AlertMessageHub> logger)
    {
        _pustokDbContext = pustokDbContext;
        _userService = userService;
        _onlineUserTracker = onlineUserTracker;
        _logger = logger;
    }


    public IActionResult Index()
    {
        var notifications = _pustokDbContext
            .UserNotifications
            .Include(x => x.Notification)
            .Include(x => x.SendingUser)
            .Include(x => x.RecievingUser)
            .ToList();

        return View(notifications);
    }

    [HttpGet("Send")]
    public IActionResult Send()
    {
        UserNotificationViewModel notificationViewModel = new UserNotificationViewModel();
        notificationViewModel.Users = _pustokDbContext.Users.ToList();
        return View(notificationViewModel);
    }

    [HttpPost("Send")]

    public IActionResult Send(UserNotificationViewModel userNotificationViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(userNotificationViewModel);
        }

        Notification notification = new()
        {
            Description = userNotificationViewModel.Description,
            Title = userNotificationViewModel.Title,
        };

        _pustokDbContext.Notifications.Add(notification);

        foreach (var userId in userNotificationViewModel.UserIds)
        {
            var user = _pustokDbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return NotFound();
            }

            UserNotification userNotification = new()
            {
                Notification = notification,
                SendingUserId = _userService.CurrentUser.Id,
                RecievingUserId = userId,

            };

            _pustokDbContext.UserNotifications.Add(userNotification);

            var connections = _onlineUserTracker.GetConnectionIds(user);
            if (connections.Any())
            {
                _logger
                    .Clients
                    .Clients(connections)
                    .SendAsync("UserNotificationFromAdmin",
                    new
                    {
                        Sender = _userService.CurrentUser.Name,
                        Reciever = user.Name,
                        Date = DateTime.Now,
                        notification.Title,
                        notification.Description

                    })
                    .Wait();
            }
        }

        _pustokDbContext.SaveChanges();

        return RedirectToAction("Index");
    }

}