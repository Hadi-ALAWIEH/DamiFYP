FROM python:3.11-slim

# Without this, Python fully buffers stdout/stderr when they're not attached
# to a real terminal (always true inside a container) - print() output can
# sit in the buffer indefinitely instead of reaching `docker compose logs`.
ENV PYTHONUNBUFFERED=1

WORKDIR /app

# opencv-python-headless still needs these system libs present even in
# "headless" mode on a slim base image.
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# MediaPipe's Tasks API needs its model bundle downloaded separately - it
# isn't shipped inside the pip package. Fetched once at build time so the
# running container doesn't need internet access. If this URL ever moves,
# grab the current one from Google's MediaPipe Face Landmarker docs and
# update it here.
ADD https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task /app/face_landmarker.task

COPY . .

EXPOSE 8001
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8001"]
