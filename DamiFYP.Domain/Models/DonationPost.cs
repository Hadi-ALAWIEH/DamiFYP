namespace DamiFYP.Domain.Models;

public class DonationPost
{
    public long Id { get; set; }
    public long UserId { get; set; }

    public BloodTypeName BloodTypeName { get; set; }
    public int? Quantity { get; set; }


    public ICollection<Match> Matches { get; set; } = new List<Match>();
}