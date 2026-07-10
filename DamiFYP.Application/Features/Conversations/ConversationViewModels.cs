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

    public long OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserEmail { get; set; } = string.Empty;
    public BusinessRole OtherUserRole { get; set; } = BusinessRole.None;

    public string? LatestMessageContent { get; set; }
    public DateTime? LatestMessageSentAt { get; set; }
    public long? LatestMessageSenderUserId { get; set; }
    public string? LatestMessageSenderName { get; set; }
}

