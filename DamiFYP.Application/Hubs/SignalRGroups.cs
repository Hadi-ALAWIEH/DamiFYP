// Central place for the SignalR group-naming scheme so DamiHub and any other
// server-side code that pushes notifications through it (e.g. MatchService,
// once a donor and seeker are matched) agree on the same group names.
public static class SignalRGroups
{
    // One group per conversation - everyone currently viewing that chat joins it.
    public static string ForConversation(long conversationId) => $"conversation-{conversationId}";

    // One group per DamiUser, joined automatically on connect, so server-side
    // code can notify a specific user (e.g. "you've been matched") without
    // needing to know which conversation groups they've joined yet.
    public static string ForUser(long userId) => $"user-{userId}";
}
