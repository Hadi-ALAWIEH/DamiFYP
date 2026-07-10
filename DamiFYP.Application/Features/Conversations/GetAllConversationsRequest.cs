using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Conversations;

public class GetAllConversationsRequest : IRequest<AllConversationsViewModel>
{
    public long UserId { get; set; }
}

public class GetAllConversationsRequestHandler : IRequestHandler<GetAllConversationsRequest, AllConversationsViewModel>
{
    private readonly DamiContext _context;

    public GetAllConversationsRequestHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<AllConversationsViewModel> Handle(GetAllConversationsRequest request, CancellationToken cancellationToken)
    {
        var conversationIds = await _context.ConversationParticipants
            .AsNoTracking()
            .Where(participant => participant.UserId == request.UserId)
            .Select(participant => participant.ConversationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (conversationIds.Count == 0)
        {
            return new AllConversationsViewModel();
        }

        var rows = await _context.Conversations
            .AsNoTracking()
            .Where(conversation => conversationIds.Contains(conversation.Id))
            .Select(conversation => new ConversationSummaryRow
            {
                ConversationId = conversation.Id,
                MatchId = conversation.MatchId,
                DonationRequestId = conversation.Match.DonationRequestId,
                DonationPostId = conversation.Match.DonationPostId,
                MatchStatus = conversation.Match.Status,
                MatchCreatedAt = conversation.Match.CreatedAt,
                DonationRequestBloodTypeName = conversation.Match.DonationRequest.BloodTypeName,
                DonationRequestQuantity = conversation.Match.DonationRequest.Quantity,
                DonationPostBloodTypeName = conversation.Match.DonationPost.BloodTypeName,
                DonationPostQuantity = conversation.Match.DonationPost.Quantity,
                OtherUserId = _context.ConversationParticipants
                    .Where(participant => participant.ConversationId == conversation.Id && participant.UserId != request.UserId)
                    .Select(participant => participant.UserId)
                    .FirstOrDefault(),
                OtherUserName = _context.ConversationParticipants
                    .Where(participant => participant.ConversationId == conversation.Id && participant.UserId != request.UserId)
                    .Select(participant => participant.User.Name)
                    .FirstOrDefault() ?? string.Empty,
                OtherUserEmail = _context.ConversationParticipants
                    .Where(participant => participant.ConversationId == conversation.Id && participant.UserId != request.UserId)
                    .Select(participant => participant.User.Email)
                    .FirstOrDefault() ?? string.Empty,
                OtherUserRole = _context.ConversationParticipants
                    .Where(participant => participant.ConversationId == conversation.Id && participant.UserId != request.UserId)
                    .Select(participant => participant.User.Role)
                    .FirstOrDefault(),
                LatestMessageContent = _context.Messages
                    .Where(message => message.ConversationParticipant.ConversationId == conversation.Id)
                    .OrderByDescending(message => message.SentAt)
                    .Select(message => message.Content)
                    .FirstOrDefault(),
                LatestMessageSentAt = _context.Messages
                    .Where(message => message.ConversationParticipant.ConversationId == conversation.Id)
                    .OrderByDescending(message => message.SentAt)
                    .Select(message => (DateTime?)message.SentAt)
                    .FirstOrDefault(),
                LatestMessageSenderUserId = _context.Messages
                    .Where(message => message.ConversationParticipant.ConversationId == conversation.Id)
                    .OrderByDescending(message => message.SentAt)
                    .Select(message => (long?)message.ConversationParticipant.UserId)
                    .FirstOrDefault(),
                LatestMessageSenderName = _context.Messages
                    .Where(message => message.ConversationParticipant.ConversationId == conversation.Id)
                    .OrderByDescending(message => message.SentAt)
                    .Select(message => message.ConversationParticipant.User.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new AllConversationsViewModel
        {
            Conversations = rows
                .OrderByDescending(row => row.LatestMessageSentAt ?? row.MatchCreatedAt)
                .Select(row => new ConversationViewModel
                {
                    ConversationId = row.ConversationId,
                    MatchId = row.MatchId,
                    DonationRequestId = row.DonationRequestId,
                    DonationPostId = row.DonationPostId,
                    MatchStatus = row.MatchStatus,
                    MatchCreatedAt = row.MatchCreatedAt,
                    DonationRequestBloodTypeName = row.DonationRequestBloodTypeName.ToString(),
                    DonationRequestQuantity = row.DonationRequestQuantity,
                    DonationPostBloodTypeName = row.DonationPostBloodTypeName.ToString(),
                    DonationPostQuantity = row.DonationPostQuantity,
                    OtherUserId = row.OtherUserId,
                    OtherUserName = row.OtherUserName,
                    OtherUserEmail = row.OtherUserEmail,
                    OtherUserRole = row.OtherUserRole,
                    LatestMessageContent = row.LatestMessageContent,
                    LatestMessageSentAt = row.LatestMessageSentAt,
                    LatestMessageSenderUserId = row.LatestMessageSenderUserId,
                    LatestMessageSenderName = row.LatestMessageSenderName
                })
                .ToList()
        };
    }

    private sealed class ConversationSummaryRow
    {
        public long ConversationId { get; set; }
        public long MatchId { get; set; }
        public long DonationRequestId { get; set; }
        public long DonationPostId { get; set; }
        public BloodTypeName DonationRequestBloodTypeName { get; set; }
        public int? DonationRequestQuantity { get; set; }
        public BloodTypeName DonationPostBloodTypeName { get; set; }
        public int? DonationPostQuantity { get; set; }
        public string? MatchStatus { get; set; }
        public DateTime MatchCreatedAt { get; set; }
        public long OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string OtherUserEmail { get; set; } = string.Empty;
        public BusinessRole OtherUserRole { get; set; } = BusinessRole.None;
        public string? LatestMessageContent { get; set; }
        public DateTime? LatestMessageSentAt { get; set; }
        public long? LatestMessageSenderUserId { get; set; }
        public string? LatestMessageSenderName { get; set; }
    }
}

