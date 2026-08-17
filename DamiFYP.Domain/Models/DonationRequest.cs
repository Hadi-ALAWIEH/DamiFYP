namespace DamiFYP.Domain.Models;

public class DonationRequest
{
    public long Id { get; set; }
    public long DamiUserId { get; set; }

    public BloodTypeName BloodTypeName { get; set; }
    public int? Quantity { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Address { get; set; }

    public DonationRequestUrgency Urgency { get; set; }
    public DonationRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? NeededByDate { get; set; }

    public DamiUser DamiUser { get; set; }
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public DonorFeedback? DonorFeedback { get; set; }

    // public ICollection<Message> Messages { get; set; } = new List<Message>();
    // public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}

public enum DonationRequestStatus
{
    Pending,
    Matched,
    Completed,
    Cancelled
}
public enum DonationRequestUrgency
{
    Low,
    Medium,
    High
}