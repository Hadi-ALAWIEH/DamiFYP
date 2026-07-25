namespace DamiFYP.Domain.Models;

public class BloodType
{
    public long Id { get; set; }

    public long DamiUserId { get; set; }
    public DamiUser DamiUser { get; set; } = null!;

    public BloodTypeName BloodTypeName { get; set; } = BloodTypeName.APositive;
}

public enum BloodTypeName
{
    APositive,
    ANegative,
    BPositive,
    BNegative,
    OPositive,
    ONegative,
    AbPositive,
    AbNegative
}