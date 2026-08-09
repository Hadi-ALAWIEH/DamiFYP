namespace DamiFYP.Infrastructure.FaceVerification;

public sealed record FaceVerificationFrameDto(string Pose, string ImageBase64);

public sealed record FaceVerificationRequestDto(List<FaceVerificationFrameDto> Frames);

public sealed record FaceVerificationResponseDto(bool Passed, string? FailureReason);
