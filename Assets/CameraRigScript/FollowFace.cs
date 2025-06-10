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

    private Vector2 leftCoord, rightCoord, topCoord, bottomCoord;
    private bool calibrated = false;

    private Vector2 targetCoord;
    private Vector2 currentCoord;

    private Quaternion initialRotation; // Store the initial rotation of the camera rig

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

        // Map the normalized offsets to rotation angles
        float yaw = Mathf.Lerp(-45, 45, normalizedOffset.x + 0.5f);   // -45 to 45 degrees
        float pitch = Mathf.Lerp(-30, 30, normalizedOffset.y + 0.5f); // -30 to 30 degrees

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
}