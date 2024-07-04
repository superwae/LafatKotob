
using Lafatkotob.Entities;
using Lafatkotob.Services;
using Lafatkotob.Services.ConversationService;
using Lafatkotob.Services.MessageService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Lafatkotob.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub, IChatHub
    {
        private readonly IUserConnectionsService _userConnectionsService;

        public ChatHub(IUserConnectionsService userConnectionsService)
        {
            _userConnectionsService = userConnectionsService;
        }

        public override async Task OnConnectedAsync()
        {
            // Get user id claim from context
            var id = Context.User?.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (id is null)
            {
                return;
            }

            _userConnectionsService.AddUserConnection(id, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var id = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (id is null)
            {
                return;
            }

            _userConnectionsService.RemoveUserConnection(id);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(
            MessageModel message,
            [FromServices] IMessageService messageService,
            [FromServices] IConversationService conversationService
        )
        {
            var response = await messageService.Post(message);

            var messageToSend = new
            {
                conversationId = message.ConversationId,
                senderUserId = message.SenderUserId,
                receiverUserId = message.ReceiverUserId,
                dateSent = message.DateSent,
                id = message.Id,
                isDeletedByReceiver = message.IsDeletedByReceiver,
                isDeletedBySender = message.IsDeletedBySender,
                isRead = message.IsRead,
                isReceived = message.IsReceived,
                messageText = message.MessageText,
            };

            var senderConnections = _userConnectionsService.GetUserConnections(message.SenderUserId);

            // Send message to the sender
            foreach (var connectionId in senderConnections)
            {
                await Clients.Client(connectionId).SendAsync("MessageSent", message.SenderUserId, messageToSend);

                await Clients.Client(connectionId).SendAsync("ConversationUpdated", new
                {
                    id = response.Data.Id,
                    lastMessage = response.Data.LastMessage,
                    lastMessageDate = response.Data.LastMessageDate,
                    userId = message.ReceiverUserId,
                    hasUnreadMessages = response.Data.HasUnreadMessages,
                    lastMessageSender = response.Data.LastMessageSender

                });
            }

            var conversationCount = await conversationService.ConversationCountWithUnreadMessages(message.ReceiverUserId);

            var receiverConnections = _userConnectionsService.GetUserConnections(message.ReceiverUserId);

            // Send message to the receiver
            foreach (var connectionId in receiverConnections)
            {
                await Clients.Client(connectionId).SendAsync("MessageSent", message.SenderUserId, messageToSend);

                await Clients.Client(connectionId).SendAsync("ConversationUpdated", new
                {
                    id = response.Data.Id,
                    lastMessage = response.Data.LastMessage,
                    lastMessageDate = response.Data.LastMessageDate,
                    userId = message.ReceiverUserId,
                    hasUnreadMessages = response.Data.HasUnreadMessages,
                    lastMessageSender = response.Data.LastMessageSender
                });

                await Clients.Client(connectionId).SendAsync("ConversationCountWithUnreadMessages", conversationCount.Data);
            }
        }

        public async Task MarkConversationAsRead
        (
           int conversationId,
           string UserId,
           [FromServices] IConversationService conversationService
        )
        {
            var conversation = await conversationService.MarkConversationAsRead(conversationId, UserId);

            var Count = await conversationService.ConversationCountWithUnreadMessages(UserId);

            var receiverConnections = _userConnectionsService.GetUserConnections(UserId);


            foreach (var connectionId in receiverConnections)
            {
                await Clients.Client(connectionId).SendAsync("ConversationCountWithUnreadMessages", Count.Data);

                if (conversation.Data != null)
                {
                    await Clients.Client(connectionId).SendAsync("updateConversationStatus", conversation.Data.Id);
                }
            }
        }

        public async Task SendNotification
        (
          Notification notification,
          List<string> userIds
        )
        {
            //var notificationModel = new
            //{
            //    id = notification.Id,
            //    message = notification.Message,
            //    dateSent = notification.DateSent,
            //    isRead = notification.IsRead,
            //    userId = notification.UserId,
            //    userName = notification.UserName,
            //    imgUrl = notification.imgUrl
            //};

            //foreach (var userid in userIds)
            //{
            //    if (string.IsNullOrEmpty(userid)) continue;

            //    if (_users.TryGetValue(userid, out List<string> receiverConnections))
            //    {
            //        if (receiverConnections == null) continue;

            //        // Skip if no connections are available
            //        foreach (var connectionId in receiverConnections)
            //        {
            //            if (string.IsNullOrEmpty(connectionId)) continue;

            //            _users.TryGetValue(userid, out List<string> receiverConnections2);

            //            await Clients.Client(connectionId).SendAsync("NotificationModel", notification);
            //        }
            //    }
            //}
        }
    }
}
