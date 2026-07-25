using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Features.BloodAvailability;

// Maps the domain's BloodTypeName enum to the "A+"/"O-" style codes the
// prediction service's trained model was fit on.
public static class BloodTypeNameCodeExtensions
{
    public static string ToBloodTypeCode(this BloodTypeName bloodTypeName) => bloodTypeName switch
    {
        BloodTypeName.APositive => "A+",
        BloodTypeName.ANegative => "A-",
        BloodTypeName.BPositive => "B+",
        BloodTypeName.BNegative => "B-",
        BloodTypeName.OPositive => "O+",
        BloodTypeName.ONegative => "O-",
        BloodTypeName.AbPositive => "AB+",
        BloodTypeName.AbNegative => "AB-",
        _ => throw new ArgumentOutOfRangeException(nameof(bloodTypeName), bloodTypeName, null)
    };
}
