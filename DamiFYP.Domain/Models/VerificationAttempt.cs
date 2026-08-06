namespace DamiFYP.Domain.Models;

public class VerificationAttempt
{
    public long Id { get; set; }
    public long DamiUserId { get; set; }
    public DamiUser DamiUser { get; set; } = null!;

    // e.g. "Center,Left,Right,Up" — the randomized pose order presented to the user.
    public string PoseSequence { get; set; } = string.Empty;

    // Outcome of this specific attempt. Reuses VerificationStatus; expected to be
    // either Verified or Failed once the attempt has been processed server-side.
    public VerificationStatus Result { get; set; }

    // Short machine-readable reason when Result == Failed, e.g. "no_face_detected",
    // "pose_mismatch", "liveness_check_failed". Null when Result == Verified.
    // No captured images are retained here by design — see notes.md for the
    // biometric data retention decision.
    public string? FailureReason { get; set; }

    public DateTime AttemptedAt { get; set; }
}
