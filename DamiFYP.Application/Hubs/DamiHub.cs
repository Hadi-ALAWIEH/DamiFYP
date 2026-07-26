using System.Security.Claims;
using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.Conversations;
using DamiFYP.Application.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

// A matched donor and seeker chat here in real time. Only signed-in users with a
// donor/seeker business role may connect at all; who may join or post into any
// given conversation is then enforced per-call by SendMessageCommand and
// GetConversationMessagesQuery, which check the caller is one of the two
// ConversationParticipants MatchService created when the match was confirmed.
[Authorize(Policy = AuthorizationPolicies.CanAccessConversations)]
public class DamiHub : Hub
{
    private readonly ILogger<DamiHub> _logger;
    private readonly IMediator _mediator;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public DamiHub(ILogger<DamiHub> logger, IMediator mediator,
        ICurrentUserProfileService currentUserProfileService)
    {
        _logger = logger;
        _mediator = mediator;
        _currentUserProfileService = currentUserProfileService;
    }

    public override async Task OnConnectedAsync()
    {
        var profile = await GetCurrentUserProfileAsync();
        if (profile != null)
        {
            // Joined once per connection so other server-side code (MatchService) can
            // push "you've been matched" notifications straight to this user.
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroups.ForUser(profile.UserId));
        }

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

    // Joins the caller to the conversation's group and hands back its message
    // history in the same call, so the client can render the chat immediately
    // without a separate REST round trip. Throws if the caller isn't one of the
    // two matched participants.
    public async Task<List<MessageViewModel>> JoinConversation(long conversationId)
    {
        var profile = await RequireCurrentUserProfileAsync();

        var history = await SendGuardedAsync(() => _mediator.Send(new GetConversationMessagesQuery
        {
            ConversationId = conversationId,
            RequestingUserId = profile.UserId
        }));

        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroups.ForConversation(conversationId));

        _logger.LogInformation(
            "Connection {ConnectionId} joined conversation {ConversationId}",
            Context.ConnectionId,
            conversationId);

        return history;
    }

    public async Task LeaveConversation(long conversationId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            SignalRGroups.ForConversation(conversationId));

        _logger.LogInformation(
            "Connection {ConnectionId} left conversation {ConversationId}",
            Context.ConnectionId,
            conversationId);
    }

    // Persists the message, then fans it out to everyone currently in the
    // conversation's group (including the sender's other open tabs/devices).
    public async Task<MessageViewModel> SendMessage(long conversationId, string content)
    {
        var profile = await RequireCurrentUserProfileAsync();

        var messageViewModel = await SendGuardedAsync(() => _mediator.Send(new SendMessageCommand
        {
            ConversationId = conversationId,
            SenderUserId = profile.UserId,
            Content = content
        }));

        await Clients.Group(SignalRGroups.ForConversation(conversationId))
            .SendAsync("ReceiveMessage", messageViewModel);

        _logger.LogInformation(
            "Message sent to conversation {ConversationId}",
            conversationId);

        return messageViewModel;
    }

    // MediatR handlers throw UnauthorizedAccessException/ArgumentException for bad
    // requests; those need translating to HubException so the message actually
    // reaches the client instead of being swallowed as a generic connection error.
    private static async Task<TResult> SendGuardedAsync<TResult>(Func<Task<TResult>> send)
    {
        try
        {
            return await send();
        }
        catch (UnauthorizedAccessException)
        {
            throw new HubException("You are not a participant of this conversation.");
        }
        catch (ArgumentException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    private async Task<UserProfile> RequireCurrentUserProfileAsync()
    {
        var profile = await GetCurrentUserProfileAsync();
        if (profile == null)
        {
            throw new HubException("Could not resolve the current user.");
        }

        return profile;
    }

    // Context.User (populated from the JWT presented during the connection handshake)
    // is the reliable way to read the caller's identity inside a Hub method - unlike
    // IHttpContextAccessor.HttpContext, it isn't guaranteed to flow through SignalR's
    // invocation pipeline, so CurrentUserProfileService.GetCurrentAsync() is not used here.
    private Task<UserProfile?> GetCurrentUserProfileAsync()
    {
        var keycloakUserId = Context.User?.FindFirstValue("sub")
                              ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return Task.FromResult<UserProfile?>(null);
        }

        var email = Context.User?.FindFirstValue("email") ?? string.Empty;
        return _currentUserProfileService.GetByUserIdAsync(keycloakUserId, email, Context.ConnectionAborted);
    }
}
