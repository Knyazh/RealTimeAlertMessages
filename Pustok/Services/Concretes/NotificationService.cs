using Microsoft.AspNetCore.SignalR;
using Pustok.Areas.Admin.ViewModels.AlertMessage;
using Pustok.Contracts;
using Pustok.Database;
using Pustok.Database.Models;
using Pustok.Exceptions;
using Pustok.Hubs;
using Pustok.Services.Abstracts;
using System.Text;

namespace Pustok.Services.Concretes;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly PustokDbContext _pustokDbContext;
    private readonly IUserService _userService;
    private readonly IHubContext<AlertMessageHub> _hubContext;
    private readonly IAlertMessageService _aletMessageService;

    public NotificationService(
        IEmailService emailService,
        PustokDbContext pustokDbContext,
        IUserService userService,
        IHubContext<AlertMessageHub> hubContext,
        IAlertMessageService aletMessageService)
    {
        _emailService = emailService;
        _pustokDbContext = pustokDbContext;
        _userService = userService;
        _hubContext = hubContext;
        _aletMessageService = aletMessageService;
    }

    public void SendOrderNotification(Order order)
    {
        switch (order.Status)
        {
            case OrderStatus.Created:
                SendOrderCreatedNotification(order);
                break;
            case OrderStatus.Approved:
                SendOrderApprovedNotification(order);
                break;
            case OrderStatus.Rejected:
                SendOrderRejectedNotification(order);
                break;
            case OrderStatus.Sent:
                SendOrderSentNotification(order);
                break;
            case OrderStatus.Completed:
                SendOrderCompletedNotification(order);
                break;
            default:
                throw new NotificationNotImplementedException();
        }
    }

    public void SendOrderCreatedNotification(Order order)
    {
        var content = PrepareOrderCreatedAlertMessageContent(order);
        var staffMembers = _userService.GetAllStaffMembers();

        foreach (var staffMember in staffMembers)
        {
            var alertMessage = new AlertMessage
            {
                Title = AlertMessageTemplates.Order.TITLE,
                Content = content,
                UserId = staffMember.Id
            };

            _pustokDbContext.AlertMessages.Add(alertMessage);

            var connectIds = _aletMessageService.GetConnectionIds(staffMember);

            var alerMessageViewModel = new AlertMessageViewModel
            {
                Title = alertMessage.Title,
                Content = alertMessage.Content,
                CreatedAt = DateTime.Now
            };

            _hubContext.Clients
                .Clients(connectIds)
                .SendAsync("ReceiveAlertMessage", alerMessageViewModel)
                .Wait();
        }
    }
    public string PrepareOrderCreatedAlertMessageContent(Order order)
    {
        var templateBuilder = new StringBuilder(AlertMessageTemplates.Order.CREATED)
            .Replace("{order_number}", order.TrackingCode);

        return templateBuilder.ToString();
    }



    public void SendOrderApprovedNotification(Order order)
    {
        var message = PrepareOrderApprovedMessage(order);
        _emailService.SendEmail(EmailTemplates.Order.SUBJECT, message, order.User.Email);
    }
    private string PrepareOrderApprovedMessage(Order order)
    {
        var templayeBuilder = new StringBuilder(EmailTemplates.Order.APPROVED)
            .Replace("{firstName}", order.User.Name)
            .Replace("{lastName}", order.User.LastName)
            .Replace("{order_number}", order.TrackingCode);

        return templayeBuilder.ToString();
    }


    public void SendOrderCompletedNotification(Order order)
    {
        var message = PrepareOrderCompletedMessage(order);
        _emailService.SendEmail(EmailTemplates.Order.SUBJECT, message, order.User.Email);
    }
    private string PrepareOrderCompletedMessage(Order order)
    {
        var templayeBuilder = new StringBuilder(EmailTemplates.Order.COMPLETED)
            .Replace("{firstName}", order.User.Name)
            .Replace("{lastName}", order.User.LastName)
            .Replace("{order_number}", order.TrackingCode);

        return templayeBuilder.ToString();
    }


    public void SendOrderRejectedNotification(Order order)
    {
        var message = PrepareOrderRejectedMessage(order);
        _emailService.SendEmail(EmailTemplates.Order.SUBJECT, message, order.User.Email);
    }
    private string PrepareOrderRejectedMessage(Order order)
    {
        var templayeBuilder = new StringBuilder(EmailTemplates.Order.REJECTED)
            .Replace("{firstName}", order.User.Name)
            .Replace("{lastName}", order.User.LastName)
            .Replace("{order_number}", order.TrackingCode);

        return templayeBuilder.ToString();
    }


    public void SendOrderSentNotification(Order order)
    {
        var message = PrepareOrderRejectedMessage(order);
        _emailService.SendEmail(EmailTemplates.Order.SUBJECT, message, order.User.Email);
    }
    private string PrepareOrderSentMessage(Order order)
    {
        var templayeBuilder = new StringBuilder(EmailTemplates.Order.SENT)
            .Replace("{firstName}", order.User.Name)
            .Replace("{lastName}", order.User.LastName)
            .Replace("{order_number}", order.TrackingCode);

        return templayeBuilder.ToString();
    }
}
