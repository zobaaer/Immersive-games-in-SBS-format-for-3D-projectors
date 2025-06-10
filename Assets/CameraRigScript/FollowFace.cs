using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FollowFace : MonoBehaviour
{
    [Header("Camera Rig to Rotate")]
    public Transform cameraRig;

    [Header("Calibration Files")]
    public string coordFolder = @"C:\Users\ahmd\Documents\SBS Game Python";
    public string currentFile = "current_coords.txt";
    public string leftFile = "left_coords.txt";
    public string rightFile = "right_coords.txt";
    public string topFile = "top_coords.txt";
    public string bottomFile = "bottom_coords.txt";

    [Header("Smoothing")]
    public float smoothSpeed = 5f;

    [Header("Rotation Mapping")]
    public float minYaw = -45f;
    public float maxYaw = 45f;
    public float minPitch = 0f;
    public float maxPitch = 30f;
    public float pitchOffset = 10f;

    private Vector2 leftCoord, rightCoord, topCoord, bottomCoord;
    private bool calibrated = false;

    private Vector2 targetCoord;
    private Vector2 currentCoord;

    private Quaternion initialRotation; // Store the initial rotation of the camera rig

    private float lastDetectionTime = 0f;
    public float detectionTimeout = 1.0f; // seconds before fallback to neutral

    void Start()
    {
        // Save the initial local rotation of the camera rig
        if (cameraRig != null)
        {
            initialRotation = cameraRig.localRotation;
        }

        StartCoroutine(Calibrate());
    }

    IEnumerator Calibrate()
    {
        // Wait for all calibration files to exist
        while (!File.Exists(Path.Combine(coordFolder, leftFile)) ||
               !File.Exists(Path.Combine(coordFolder, rightFile)) ||
               !File.Exists(Path.Combine(coordFolder, topFile)) ||
               !File.Exists(Path.Combine(coordFolder, bottomFile)))
        {
            yield return new WaitForSeconds(0.5f);
        }

        leftCoord = ReadCoord(leftFile);
        rightCoord = ReadCoord(rightFile);
        topCoord = ReadCoord(topFile);
        bottomCoord = ReadCoord(bottomFile);

        calibrated = true;
        yield break;
    }

    void Update()
    {
        if (!calibrated || cameraRig == null)
            return;

        Vector2 newCoord = ReadCoord(currentFile);

        // Only update if the file has changed
        if (newCoord != targetCoord)
        {
            targetCoord = newCoord;
            lastDetectionTime = Time.time;
        }

        // If detection is lost for too long, return to center
        if (Time.time - lastDetectionTime > detectionTimeout)
        {
            targetCoord = new Vector2(
                (leftCoord.x + rightCoord.x) / 2,
                (topCoord.y + bottomCoord.y) / 2
            );
        }

        // Smoothly interpolate currentCoord towards targetCoord
        currentCoord = Vector2.Lerp(currentCoord, targetCoord, Time.deltaTime * smoothSpeed);

        // Log the current coordinates to the console
        Debug.Log($"Current Coordinates: {currentCoord}");

        // Calculate the center of the camera based on calibration files
        Vector2 centerCoord = new Vector2(
            (leftCoord.x + rightCoord.x) / 2,
            (topCoord.y + bottomCoord.y) / 2
        );

        // Calculate offsets based on the difference between currentCoord and centerCoord
        Vector2 offset = currentCoord - centerCoord;

        // Normalize the offsets to a range of -1 to 1
        Vector2 normalizedOffset = new Vector2(
            Mathf.InverseLerp(leftCoord.x, rightCoord.x, currentCoord.x) - 0.5f,
            Mathf.InverseLerp(topCoord.y, bottomCoord.y, currentCoord.y) - 0.5f
        );

        // Map the normalized offsets to rotation angles using public fields
        float yaw = Mathf.Lerp(minYaw, maxYaw, normalizedOffset.x + 0.5f);
        float pitch = Mathf.Lerp(minPitch, maxPitch, normalizedOffset.y + 0.5f);

        // Add a pitch offset to correct the camera looking too down or too up
        pitch += pitchOffset;

        // Apply the rotation offsets to the camera rig based on the initial rotation
        cameraRig.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, 0);
    }

    Vector2 ReadCoord(string fileName)
    {
        string path = Path.Combine(coordFolder, fileName);
        if (!File.Exists(path))
            return targetCoord;

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                string content = reader.ReadToEnd();
                string[] values = content.Split(',');

                if (values.Length >= 2)
                {
                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);
                    return new Vector2(x, y);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading coordinates: {ex.Message}");
        }
        return targetCoord;
    }

    Vector2 NormalizeCoord(Vector2 coord)
    {
        // Map coord.x from leftCoord.x - rightCoord.x to 0-1
        float x = Mathf.InverseLerp(leftCoord.x, rightCoord.x, coord.x);
        // Map coord.y from topCoord.y - bottomCoord.y to 0-1
        float y = Mathf.InverseLerp(topCoord.y, bottomCoord.y, coord.y);
        return new Vector2(x, y);
    }

    // Helper to check if a coordinate is valid (not zero and within calibration bounds)
    private bool IsValidCoord(Vector2 coord)
    {
        // You can adjust this logic based on your detection system's behavior
        return
            coord.x >= leftCoord.x && coord.x <= rightCoord.x &&
            coord.y >= topCoord.y && coord.y <= bottomCoord.y;
    }
}