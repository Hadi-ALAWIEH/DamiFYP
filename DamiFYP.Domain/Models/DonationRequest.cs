namespace DamiFYP.Domain.Models;

public class DonationRequest
{
    public long Id { get; set; }
    public long UserId { get; set; }

    public BloodTypeName BloodTypeName { get; set; }
    public int? Quantity { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? UrgencyLevel { get; set; }
    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? NeededByDate { get; set; }

    public User User { get; set; }
    public ICollection<Match> Matches { get; set; } = new List<Match>();

    // public ICollection<Message> Messages { get; set; } = new List<Message>();
    // public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}

