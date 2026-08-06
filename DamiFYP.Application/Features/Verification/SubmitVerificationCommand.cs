using System.Text.Json.Serialization;
using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Infrastructure.FaceVerification;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DamiFYP.Application.Features.Verification;

public sealed class SubmitVerificationPoseFrame
{
    // "Center" | "Left" | "Right" | "Up" - must match the sequence the
    // frontend was actually given for this attempt.
    public string Pose { get; set; } = string.Empty;

    // Base64-encoded JPEG/PNG for the single auto-captured frame for this
    // pose. Accepts either a raw base64 string or a full data: URL - the
    // handler strips the "data:image/...;base64," prefix if present.
    public string ImageBase64 { get; set; } = string.Empty;
}

public sealed class SubmitVerificationCommand : IRequest<SubmitVerificationViewModel>
{
    public List<SubmitVerificationPoseFrame> Frames { get; set; } = new();

    [JsonIgnore]
    public long UserId { get; set; }

    [JsonIgnore]
    public string KeyCloakUserId { get; set; } = string.Empty;
}

public sealed class SubmitVerificationViewModel
{
    public VerificationStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public int AttemptCount { get; set; }
}

public sealed class SubmitVerificationCommandHandler : IRequestHandler<SubmitVerificationCommand, SubmitVerificationViewModel>
{
    // Expected pose sequence length - kept in sync with the frontend's capture
    // flow (center, left, right, up). Rejecting short submissions here means a
    // malicious client can't just send one convenient frame and call it done.
    private const int ExpectedFrameCount = 4;

    private readonly DamiContext _context;
    private readonly IFaceVerificationService _faceVerificationService;
    private readonly ICurrentUserProfileService _profileService;
    private readonly ILogger<SubmitVerificationCommandHandler> _logger;

    public SubmitVerificationCommandHandler(
        DamiContext context,
        IFaceVerificationService faceVerificationService,
        ICurrentUserProfileService profileService,
        ILogger<SubmitVerificationCommandHandler> logger)
    {
        _context = context;
        _faceVerificationService = faceVerificationService;
        _profileService = profileService;
        _logger = logger;
    }

    public async Task<SubmitVerificationViewModel> Handle(SubmitVerificationCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            throw new InvalidOperationException("Current user is required for verification.");
        }

        if (request.Frames is null || request.Frames.Count < ExpectedFrameCount)
        {
            throw new ArgumentException(
                $"Expected {ExpectedFrameCount} captured frames (one per pose).", nameof(request.Frames));
        }

        var user = await _context.DamiUsers.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken)
                   ?? throw new InvalidOperationException("User profile not found.");

        // No cap on attempts - a user can retry as many times as they want.
        // We still count and log every attempt below (VerificationAttempt
        // rows + the AttemptCount returned to the caller) purely for
        // visibility/audit purposes, it just no longer blocks anything.
        var poseSequence = string.Join(",", request.Frames.Select(f => f.Pose));

        // Never trust the frontend's own pose-matched claim - re-run the check
        // server-side against the uploaded frames. Any failure here (bad
        // image data, the detection service being unreachable, etc.) is
        // treated as a failed attempt rather than an unhandled 500, so the
        // user gets a normal retry instead of a broken page.
        FaceVerificationResult result;
        try
        {
            var frames = request.Frames.Select(f => new VerificationPoseFrame
            {
                Pose = f.Pose,
                ImageData = DecodeImage(f.ImageBase64)
            }).ToList();

            result = await _faceVerificationService.VerifyAsync(frames, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Face verification check threw for user {UserId}", user.Id);
            result = new FaceVerificationResult { Passed = false, FailureReason = "verification_error" };
        }

        var attempt = new VerificationAttempt
        {
            DamiUserId = user.Id,
            PoseSequence = poseSequence,
            Result = result.Passed ? VerificationStatus.Verified : VerificationStatus.Failed,
            FailureReason = result.Passed ? null : result.FailureReason,
            AttemptedAt = DateTime.UtcNow
        };
        _context.VerificationAttempts.Add(attempt);

        user.VerificationStatus = attempt.Result;
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.KeyCloakUserId))
        {
            await _profileService.InvalidateAsync(request.KeyCloakUserId);
        }

        var attemptCount = await _context.VerificationAttempts
            .AsNoTracking()
            .CountAsync(x => x.DamiUserId == user.Id, cancellationToken);

        return new SubmitVerificationViewModel
        {
            Status = attempt.Result,
            FailureReason = attempt.FailureReason,
            AttemptCount = attemptCount
        };
    }

    private static byte[] DecodeImage(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return Array.Empty<byte>();
        }

        var commaIndex = base64.IndexOf(',');
        var payload = base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0
            ? base64[(commaIndex + 1)..]
            : base64;

        return Convert.FromBase64String(payload);
    }
}
