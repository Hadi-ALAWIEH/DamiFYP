namespace DamiFYP.Application.Features.BloodAvailability;

public class BloodAvailabilityPredictionViewModel
{
    public string Availability { get; set; } = string.Empty;
    public Dictionary<string, double> Probabilities { get; set; } = new();
}

public class BloodAvailabilityMetadataViewModel
{
    public List<string> Hospitals { get; set; } = new();
    public List<string> HospitalSizes { get; set; } = new();
    public List<string> Months { get; set; } = new();
    public List<string> BloodTypes { get; set; } = new();
    public List<string> TargetClasses { get; set; } = new();
    public double TestAccuracy { get; set; }
}
