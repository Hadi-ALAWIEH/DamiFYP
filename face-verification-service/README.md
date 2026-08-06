# Face Verification Service

Small FastAPI service that re-runs face/pose detection server-side for
DamiFYP's identity-verification onboarding step. The .NET backend
(`SubmitVerificationCommandHandler`, via `FaceVerificationServiceClient`)
calls this instead of trusting the frontend's own "pose matched" claim.

## Run locally

    pip install -r requirements.txt
    uvicorn app:app --port 8001

Or via the repo's `compose.yaml` (`docker compose up face-verification`),
which builds this folder's `Dockerfile` and exposes port 8001 alongside
Postgres and Keycloak.

Point the .NET API at it via `appsettings.json` -> `FaceVerificationService:BaseUrl`
(defaults to `http://localhost:8001`, matching the compose port mapping).

## API

`POST /verify`

```json
{
  "frames": [
    { "pose": "Center", "image_base64": "..." },
    { "pose": "Left",   "image_base64": "..." },
    { "pose": "Right",  "image_base64": "..." },
    { "pose": "Up",     "image_base64": "..." }
  ]
}
```

->

```json
{ "passed": true, "failure_reason": null }
```

`failure_reason` (when `passed` is `false`): `no_frames`, `invalid_image`,
`static_image_detected`, `no_face_detected`, `multiple_faces_detected`,
`pose_mismatch`.

## Notes / things to tune before relying on this beyond a student project

- Uses MediaPipe's **Tasks API** (`FaceLandmarker`), same API family as the
  frontend's Face Landmarker. Head pose (yaw/pitch/roll) is derived from the
  detector's facial transformation matrix rather than a hand-rolled
  `solvePnP` call. This needed its model bundle (`face_landmarker.task`)
  downloaded at Docker build time - see the `ADD` line in the Dockerfile -
  because the older `mp.solutions` Solutions API this originally used has
  been removed entirely from recent mediapipe releases
  (`ModuleNotFoundError: No module named 'mediapipe.python'`).
- `YAW_THRESHOLD`, `PITCH_UP_THRESHOLD`, `CENTER_TOLERANCE` in `app.py` are
  starting points - calibrate against your actual webcam and frontend
  capture flow.
- The Left/Right sign convention assumes the image is **not** mirrored
  (subject's real left appears on the left of the frame). If the frontend
  sends its mirrored `<video>` preview frame instead, flip those two
  branches in `classify_pose`.
- The "frames too similar" check is a cheap anti-replay heuristic (rejects
  four near-identical images), not real liveness detection. It stops the
  most trivial "submit one photo four times" attack, nothing more
  sophisticated (e.g. four different photos of the same static picture from
  slightly different angles would still pass).
- No captured frames are stored by this service - each request is processed
  in memory and discarded, matching the "retain the result, not the images"
  decision made for `VerificationAttempt` on the .NET side.
