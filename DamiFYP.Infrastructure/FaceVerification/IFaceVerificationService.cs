namespace DamiFYP.Infrastructure.FaceVerification;

// A single captured frame from the client's pose-capture sequence.
// Pose is one of "Center" | "Left" | "Right" | "Up", kept as a plain string
// rather than an enum so the frontend's randomized sequence order doesn't
// need to stay in lockstep with a backend enum definition.
public sealed class VerificationPoseFrame
{
    public string Pose { get; set; } = string.Empty;

    // Decoded image bytes for this frame. Only ever held in memory for the
    // duration of the check - nothing here gets persisted (VerificationAttempt
    // stores the pass/fail result, not the image itself; see notes.md for the
    // retention decision).
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
}

public sealed class FaceVerificationResult
{
    public bool Passed { get; set; }

    // Machine-readable reason when Passed == false, e.g. "no_face_detected",
    // "pose_mismatch", "liveness_check_failed". Mirrors VerificationAttempt.FailureReason.
    public string? FailureReason { get; set; }
}

// Server-side face/pose re-verification. The frontend's own MediaPipe check
// only decides *when* to auto-capture a frame - it is never trusted as proof
// the pose sequence was actually followed, since a malicious client could
// skip the camera entirely and POST fabricated "success" data.
//
// Lives in DamiFYP.Infrastructure (not Application.Helpers) to mirror
// IBloodAvailabilityServiceClient's placement - DamiFYP.Application has a
// ProjectReference to DamiFYP.Infrastructure (not the reverse), so handlers
// in Application consume this interface directly via that reference, the
// same way GetBloodAvailabilityMetadataQueryHandler consumes
// IBloodAvailabilityServiceClient.
public interface IFaceVerificationService
{
    Task<FaceVerificationResult> VerifyAsync(
        IReadOnlyList<VerificationPoseFrame> frames,
        CancellationToken cancellationToken = default);
}
