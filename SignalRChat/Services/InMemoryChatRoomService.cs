using SignalRChat.Models;

namespace SignalRChat.Services
{
    public class InMemoryChatRoomService : IChatRoomService
    {
        private readonly Dictionary<Guid, ChatRoom> _roomInfo = new Dictionary<Guid, ChatRoom>();
        private readonly Dictionary<Guid, List<ChatHub.ChatMessage>> _messageHistory = new Dictionary<Guid, List<ChatHub.ChatMessage>>();
        public Task AddMessage(Guid roomId, ChatHub.ChatMessage message)
        {
            if (!_messageHistory.ContainsKey(roomId))
            {
                _messageHistory[roomId] = new List<ChatHub.ChatMessage>();
            }

            _messageHistory[roomId].Add(message);

            return Task.CompletedTask;
        }

        public Task<Guid> CreateRoom(string connectionId)
        {
            var id = Guid.NewGuid();
            _roomInfo[id] = new ChatRoom
            {
                OwnerConnectionId = connectionId
            };

            return Task.FromResult(id);
        }

        public Task<IEnumerable<ChatHub.ChatMessage>> GetMessageHistory(Guid roomId)
        {
            _messageHistory.TryGetValue(roomId, out var messages);
            messages = messages ?? new List<ChatHub.ChatMessage>();

            var sortedMessages = messages.OrderBy(x => x.SendAt).AsEnumerable();

            return Task.FromResult(sortedMessages);
        }

        public Task<Guid> GetRoomForConnectionId(string connectionId)
        {
            var foundRoom = _roomInfo.FirstOrDefault(x => x.Value.OwnerConnectionId == connectionId);

            if (foundRoom.Key == Guid.Empty)
                throw new ArgumentException("Invalid Connection Id.");

            return Task.FromResult(foundRoom.Key);
        }

        public Task SetRoomName(Guid roomId, string name)
        {
            if (!_roomInfo.ContainsKey(roomId))
                throw new ArgumentException("Invalid room Id.");

            _roomInfo[roomId].Name = name;

            return Task.CompletedTask;
        }
    }
}
