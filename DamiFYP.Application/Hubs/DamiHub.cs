using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class DamiHub : Hub
{
    private readonly ILogger<DamiHub> _logger;

    public DamiHub(ILogger<DamiHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "Client connected: {ConnectionId}",
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Client disconnected: {ConnectionId}",
            Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogError(
                exception,
                "Client disconnected due to an error.");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(long conversationId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            conversationId.ToString());

        _logger.LogInformation(
            "Connection {ConnectionId} joined conversation {ConversationId}",
            Context.ConnectionId,
            conversationId);
    }

    public async Task LeaveConversation(long conversationId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            conversationId.ToString());

        _logger.LogInformation(
            "Connection {ConnectionId} left conversation {ConversationId}",
            Context.ConnectionId,
            conversationId);
    }

    public async Task SendMessage(long conversationId, string message)
    {
        await Clients.Group(conversationId.ToString())
            .SendAsync(
                "ReceiveMessage",
                Context.ConnectionId,
                message);

        _logger.LogInformation(
            "Message sent to conversation {ConversationId}",
            conversationId);
    }
}
