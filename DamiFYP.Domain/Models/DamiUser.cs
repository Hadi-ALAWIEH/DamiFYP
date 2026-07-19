namespace DamiFYP.Domain.Models;

public class DamiUser
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public BusinessRole Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string KeyCloakId { get; set; } = string.Empty;

    public BloodType BloodType { get; set; } = new();
    public ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();
    public ICollection<DonationPost> DonationPosts { get; set; } = new List<DonationPost>();
    public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    // public ICollection<Match> MatchesAsDonor { get; set; } = new List<Match>();
    // public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    // public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    // public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}

