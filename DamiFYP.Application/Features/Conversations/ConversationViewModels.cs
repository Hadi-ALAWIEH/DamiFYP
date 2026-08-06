using System;
using System.Collections.Generic;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Features.Conversations;

public class AllConversationsViewModel
{
    public List<ConversationViewModel> Conversations { get; set; } = new();
}

public class ConversationViewModel
{
    public long ConversationId { get; set; }
    public long MatchId { get; set; }
    public long DonationRequestId { get; set; }
    public long DonationPostId { get; set; }
    public string? MatchStatus { get; set; }
    public DateTime MatchCreatedAt { get; set; }

    public string? DonationRequestBloodTypeName { get; set; }
    public int? DonationRequestQuantity { get; set; }
    public string? DonationPostBloodTypeName { get; set; }
    public int? DonationPostQuantity { get; set; }

    public long DonorUserId { get; set; }
    public double? DonationPostLatitude { get; set; }
    public double? DonationPostLongitude { get; set; }
    public double? DonationRequestLatitude { get; set; }
    public double? DonationRequestLongitude { get; set; }

    public long OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserEmail { get; set; } = string.Empty;
    public BusinessRole OtherUserRole { get; set; } = BusinessRole.None;

    public string? LatestMessageContent { get; set; }
    public DateTime? LatestMessageSentAt { get; set; }
    public long? LatestMessageSenderUserId { get; set; }
    public string? LatestMessageSenderName { get; set; }

    // True when this conversation has at least one message from the OTHER
    // participant that the current user hasn't opened yet (Message.IsRead).
    // Persisted server-side, so it's correct on first load — including right
    // after logging back in — instead of being derived from client-only state.
    public bool IsUnread { get; set; }
}

// Pushed over SignalR (event "ConversationStarted") to both matched users the moment
// MatchService confirms a match, so their clients can surface the new chat immediately
// instead of having to poll GetAllConversations.
public class ConversationStartedNotification
{
    public long ConversationId { get; set; }
    public long MatchId { get; set; }
    public long OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
}

