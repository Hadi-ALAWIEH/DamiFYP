namespace DamiFYP.Domain.Models;

public class BloodType
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
}