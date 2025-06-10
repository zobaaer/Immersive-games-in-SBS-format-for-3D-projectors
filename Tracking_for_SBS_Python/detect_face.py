import cv2
import os

coord_files = {
    "current": "current_coords.txt",
    "left": "left_coords.txt",
    "right": "right_coords.txt",
    "top": "top_coords.txt",
    "bottom": "bottom_coords.txt"
}

for fname in coord_files.values():
    if not os.path.exists(fname):
        with open(fname, "w") as f:
            f.write("")

# Load Haar cascade for face detection
face_cascade = cv2.CascadeClassifier(cv2.data.haarcascades + "haarcascade_frontalface_default.xml")

# Open webcam
cap = cv2.VideoCapture(0)

def save_coords(filename, coords):
    with open(filename, "w") as f:
        f.write(f"{coords[0]},{coords[1]},{coords[2]},{coords[3]}")

while True:

    ret, frame = cap.read()
    if not ret:
        break

    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    faces = face_cascade.detectMultiScale(gray, 1.3, 5)

    h, w = frame.shape[:2]
    border_thresh = 0.1  # 10% of width/height

    for (x, y, fw, fh) in faces:
        # Draw bounding box
        cv2.rectangle(frame, (x, y), (x+fw, y+fh), (0, 255, 0), 2)
        # Draw center
        cx, cy = x + fw // 2, y + fh // 2
        cv2.circle(frame, (cx, cy), 4, (0, 0, 255), -1)

        # Save current coords
        save_coords(coord_files["current"], (x, y, fw, fh))
    
        # Check borders
        if x < int(w * border_thresh):
            save_coords(coord_files["left"], (x, y, fw, fh))
        if x + fw > int(w * (1 - border_thresh)):
            save_coords(coord_files["right"], (x, y, fw, fh))
        if y < int(h * border_thresh):
            save_coords(coord_files["top"], (x, y, fw, fh))
        if y + fh > int(h * (1 - border_thresh)):
            save_coords(coord_files["bottom"], (x, y, fw, fh))

        break  # Only process the first detected face

    cv2.imshow('Face Detection', frame)
    if cv2.waitKey(1) & 0xFF == 27:  # ESC to quit
        break

cap.release()
cv2.destroyAllWindows()