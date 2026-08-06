"""
DamiFYP face-verification service.

Small FastAPI service that re-runs face/pose detection server-side for the
identity-verification step of onboarding. The .NET backend (SubmitVerification
CommandHandler) posts the frames the frontend auto-captured during its own
MediaPipe pose sequence here, and this service independently checks:

  1. Exactly one face is present in every frame.
  2. Each frame's actual head pose (yaw/pitch, derived from MediaPipe Face
     Landmarker's facial transformation matrix) matches the pose label the
     frontend claims for that frame ("Center" | "Left" | "Right" | "Up").
  3. The frames aren't all near-identical - a cheap anti-replay check against
     someone submitting one static photo four times instead of actually
     turning their head.

The frontend's own in-browser pose detection is only used to decide *when*
to auto-capture a frame - it is never trusted as proof by itself. This
service is what actually decides pass/fail.

No frames are persisted anywhere by this service - they're processed
in-memory for the single request and discarded.

Uses MediaPipe's Tasks API (Face Landmarker) rather than the older
`mp.solutions` Solutions API - recent mediapipe releases have dropped the
legacy Solutions submodules entirely, and Tasks is the actively maintained
surface (and the same API family the frontend uses). It needs its model
bundle (`face_landmarker.task`) downloaded separately - see the Dockerfile.
"""

import base64
import math
import os
import re
from typing import List, Optional, Tuple, Union

import cv2
import numpy as np
from fastapi import FastAPI
from pydantic import BaseModel

import mediapipe as mp
from mediapipe.tasks import python as mp_tasks
from mediapipe.tasks.python import vision as mp_vision

app = FastAPI(title="DamiFYP Face Verification Service")

MODEL_PATH = os.environ.get("FACE_LANDMARKER_MODEL_PATH", "face_landmarker.task")

# Degrees. Starting points only - calibrate against your own webcam/frontend
# before relying on this for anything beyond a student project. The frontend
# sends the RAW (unmirrored) captured frame, not the mirrored preview the
# user sees - confirmed by real-device testing that when the subject turns
# their head to their own left, yaw comes out POSITIVE here, not negative
# (the opposite of the naive assumption). Keep classify_pose() in sync with
# classifyPose() in useFacePose.ts - both must agree or every submission
# fails pose_mismatch.
YAW_THRESHOLD = 15.0
PITCH_UP_THRESHOLD = -12.0
CENTER_TOLERANCE = 10.0

# Anti-replay: mean per-pixel grayscale difference (0-255 scale, on a
# downsized 64x64 image) below this between two frames is treated as
# "basically the same image".
STATIC_IMAGE_DIFF_THRESHOLD = 1.5


class Frame(BaseModel):
    pose: str
    image_base64: str


class VerifyRequest(BaseModel):
    frames: List[Frame]


class VerifyResponse(BaseModel):
    passed: bool
    failure_reason: Optional[str] = None


_landmarker: Optional[mp_vision.FaceLandmarker] = None


def get_landmarker() -> mp_vision.FaceLandmarker:
    """Lazily creates the FaceLandmarker once per process instead of per
    request - it loads the model bundle from disk, which isn't cheap."""
    global _landmarker
    if _landmarker is None:
        options = mp_vision.FaceLandmarkerOptions(
            base_options=mp_tasks.BaseOptions(model_asset_path=MODEL_PATH),
            running_mode=mp_vision.RunningMode.IMAGE,
            num_faces=2,
            output_facial_transformation_matrixes=True,
            output_face_blendshapes=False,
        )
        _landmarker = mp_vision.FaceLandmarker.create_from_options(options)
    return _landmarker


def decode_image(image_base64: str) -> Optional[np.ndarray]:
    payload = image_base64
    match = re.match(r"^data:image/\w+;base64,(.*)$", payload)
    if match:
        payload = match.group(1)

    try:
        raw = base64.b64decode(payload)
    except Exception:
        return None

    array = np.frombuffer(raw, dtype=np.uint8)
    image = cv2.imdecode(array, cv2.IMREAD_COLOR)
    return image


def rotation_matrix_to_euler(matrix: Union[np.ndarray, list]) -> Tuple[float, float, float]:
    m = np.array(matrix, dtype=np.float64).reshape(4, 4)[:3, :3]
    sy = math.sqrt(m[0, 0] ** 2 + m[1, 0] ** 2)
    # Matches useFacePose.ts's rotationMatrixToEuler - real-device testing
    # showed the textbook Rz*Ry*Rx names for these three don't match
    # MediaPipe's actual face-camera axis convention. atan2(-m[2,0], sy)
    # tracks real left/right yaw; atan2(m[1,0], m[0,0]) stayed near 0 through
    # both a pure left/right turn and an extreme up/down tilt (consistent
    # with it being roll, i.e. ear-to-shoulder tilt, untested so far); by
    # elimination atan2(m[2,1], m[2,2]) is real pitch. Sign not yet confirmed
    # by a live test - if PITCH_UP_THRESHOLD triggers backwards, negate this.
    # Must stay in sync with the frontend or every submission fails
    # pose_mismatch.
    yaw = math.degrees(math.atan2(-m[2, 0], sy))
    pitch = math.degrees(math.atan2(m[2, 1], m[2, 2]))
    roll = math.degrees(math.atan2(m[1, 0], m[0, 0]))
    return yaw, pitch, roll


def estimate_head_pose(image_bgr: np.ndarray) -> Optional[Tuple[float, float, float]]:
    """Returns (yaw, pitch, roll) in degrees, None if no face was found, or
    the string "multiple" if more than one face was found."""
    rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
    mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)

    result = get_landmarker().detect(mp_image)

    if not result.face_landmarks:
        return None
    if len(result.face_landmarks) > 1:
        return "multiple"  # type: ignore[return-value]
    if not result.facial_transformation_matrixes:
        return None

    return rotation_matrix_to_euler(result.facial_transformation_matrixes[0])


def classify_pose(yaw: float, pitch: float) -> str:
    if pitch < PITCH_UP_THRESHOLD:
        return "Up"
    if yaw < -YAW_THRESHOLD:
        return "Right"
    if yaw > YAW_THRESHOLD:
        return "Left"
    if abs(yaw) <= CENTER_TOLERANCE and abs(pitch) <= CENTER_TOLERANCE:
        return "Center"
    return "Ambiguous"


def frames_too_similar(images: List[np.ndarray]) -> bool:
    if len(images) < 2:
        return False

    resized = [cv2.resize(cv2.cvtColor(img, cv2.COLOR_BGR2GRAY), (64, 64)) for img in images]
    for i in range(len(resized) - 1):
        diff = cv2.absdiff(resized[i], resized[i + 1])
        if float(np.mean(diff)) < STATIC_IMAGE_DIFF_THRESHOLD:
            return True
    return False


@app.post("/verify", response_model=VerifyResponse)
def verify(request: VerifyRequest) -> VerifyResponse:
    if len(request.frames) == 0:
        return VerifyResponse(passed=False, failure_reason="no_frames")

    images: List[np.ndarray] = []
    for frame in request.frames:
        image = decode_image(frame.image_base64)
        if image is None:
            return VerifyResponse(passed=False, failure_reason="invalid_image")
        images.append(image)

    if frames_too_similar(images):
        return VerifyResponse(passed=False, failure_reason="static_image_detected")

    for i, (frame, image) in enumerate(zip(request.frames, images)):
        pose_result = estimate_head_pose(image)

        if pose_result is None:
            print(f"[verify] frame {i}: requested={frame.pose!r} -> no face detected", flush=True)
            return VerifyResponse(passed=False, failure_reason="no_face_detected")
        if pose_result == "multiple":
            print(f"[verify] frame {i}: requested={frame.pose!r} -> multiple faces detected", flush=True)
            return VerifyResponse(passed=False, failure_reason="multiple_faces_detected")

        yaw, pitch, roll = pose_result
        detected_pose = classify_pose(yaw, pitch)

        # DEBUG: prints every frame (not just failures) so you can see the
        # full sequence in `docker compose logs -f face-verification`. Safe
        # to remove once thresholds are confirmed good against this service's
        # own model/image pipeline (which may see slightly different angles
        # than the frontend's live WASM detector did on the same physical
        # pose, due to JPEG compression and running IMAGE mode vs VIDEO mode).
        match = "OK" if detected_pose == frame.pose else "MISMATCH"
        print(
            f"[verify] frame {i}: requested={frame.pose!r} detected={detected_pose!r} "
            f"yaw={yaw:.1f} pitch={pitch:.1f} roll={roll:.1f} -> {match}",
            flush=True,
        )

        if detected_pose != frame.pose:
            return VerifyResponse(passed=False, failure_reason="pose_mismatch")

    return VerifyResponse(passed=True)


@app.get("/health")
def health():
    return {"status": "ok"}
