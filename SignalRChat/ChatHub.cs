using Microsoft.AspNetCore.SignalR;
using SignalRChat.Services;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;

        public ChatHub(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }
        public record ChatMessage(string SenderName, string Text, DateTimeOffset SendAt);
        public async Task SendMessage(string name, string text)
        { 
            // To work with groups (rooms)
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);

            var message =  new ChatMessage(name, text, DateTimeOffset.UtcNow);

            // Storing message in Service
            await _chatRoomService.AddMessage(roomId, message);

            // Broadcast to all clients
            // First parameter is the method name
            // This will invoke the ReceivedMessage method in any connected client.
            //await Clients.All.SendAsync(
            //                            "ReceivedMessage",
            //                            message.SenderName,
            //                            message.Text,
            //                            message.SendAt);

            // Broadcast to specific groups (rooms)
            await Clients.Group(roomId.ToString()).SendAsync(
                                                             "ReceivedMessage",
                                                             message.SenderName,
                                                             message.Text,
                                                             message.SendAt);
        }

        // To Send message to client that just connected, not all clients.
        public override async Task OnConnectedAsync()
        {
            var roomId = await _chatRoomService.CreateRoom(Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            await Clients.Caller.SendAsync(
                                            "ReceivedMessage",
                                            "Chat Bot",
                                            "Hello! What can we help you with today?",
                                            DateTimeOffset.UtcNow);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SetName(string visitorName)
        {
            var roomName = $"Chat with {visitorName} from the web";
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);
            await _chatRoomService.SetRoomName(roomId, roomName);
        }
    }
}
