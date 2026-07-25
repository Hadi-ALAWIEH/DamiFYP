namespace DamiFYP.Infrastructure.BloodAvailability;

public sealed record BloodAvailabilityPredictionRequestDto(
    string Hospital,
    string HospitalSize,
    string Month,
    string BloodType,
    int OpeningStock,
    int DonationsReceived,
    int BloodRequests,
    bool HolidaySeason,
    bool SummerSeason);

public sealed record BloodAvailabilityPredictionResponseDto(
    string Availability,
    Dictionary<string, double> Probabilities);

public sealed record BloodAvailabilityMetadataDto(
    List<string> Hospitals,
    List<string> HospitalSizes,
    List<string> Months,
    List<string> BloodTypes,
    List<string> TargetClasses,
    double TestAccuracy);
