using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FollowFaceFish : MonoBehaviour
{
    [Header("Fish Bank to Move")]
    public Transform fishBank;

    [Header("Calibration Files")]
    public string coordFolder = "Tracking_for_SBS_Python";
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

    private Vector3 initialPosition; // Store the initial position of the fish bank

    private Vector3 targetCoord3D;
    private Vector3 currentCoord3D;

    void Start()
    {
        // Make coordFolder relative to the application's data path
        coordFolder = Path.Combine(Application.dataPath, "..", coordFolder);

        // Save the initial position of the fish bank
        if (fishBank != null)
        {
            initialPosition = fishBank.position;
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
        if (!calibrated || fishBank == null)
            return;

        Vector3 newCoord3D = ReadCoord3D(currentFile);

        // Only update if the file has changed
        if (newCoord3D != targetCoord3D)
        {
            targetCoord3D = newCoord3D;
        }

        // Smoothly interpolate currentCoord3D towards targetCoord3D
        currentCoord3D = Vector3.Lerp(currentCoord3D, targetCoord3D, Time.deltaTime * smoothSpeed);

        // Log the current coordinates to the console
        Debug.Log($"Current Coordinates: {currentCoord3D}");

        // Calculate the center of the calibration area (XY only)
        Vector2 centerCoord = new Vector2(
            (leftCoord.x + rightCoord.x) / 2,
            (topCoord.y + bottomCoord.y) / 2
        );

        // Normalize the offsets to a range of -1 to 1 (XY only)
        Vector2 normalizedOffset = new Vector2(
            -(Mathf.InverseLerp(leftCoord.x, rightCoord.x, currentCoord3D.x) - 0.5f),
            -(Mathf.InverseLerp(topCoord.y, bottomCoord.y, currentCoord3D.y) - 0.5f)
        );

        // Map the normalized offsets to the desired range
        float mappedX = Mathf.Lerp(-50, 50, normalizedOffset.x + 0.5f); 
        float mappedY = Mathf.Lerp(-100, 50, normalizedOffset.y + 0.5f);

        // Map depth (Z): adjust these values as needed for your scene
        float minDepth = 114f;   // Closest
        float maxDepth = 60f; // Furthest
        float mappedZ = Mathf.Lerp(100, -100, Mathf.InverseLerp(minDepth, maxDepth, currentCoord3D.z));

        // Apply the offsets to the fish bank's position
        Vector3 newPosition = initialPosition + new Vector3(mappedX, mappedY, mappedZ);
        fishBank.position = Vector3.Lerp(fishBank.position, newPosition, Time.deltaTime * smoothSpeed);
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

    Vector3 ReadCoord3D(string fileName)
    {
        string path = Path.Combine(coordFolder, fileName);
        if (!File.Exists(path))
            return targetCoord3D;

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                string content = reader.ReadToEnd();
                string[] values = content.Split(',');

                if (values.Length >= 3)
                {
                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);
                    float z = float.Parse(values[2]);
                    return new Vector3(x, y, z);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading coordinates: {ex.Message}");
        }
        return targetCoord3D;
    }
}