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

    private Vector3 initialPosition; // Store the initial position of the fish bank

    void Start()
    {
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

        // Calculate the center of the calibration area
        Vector2 centerCoord = new Vector2(
            (leftCoord.x + rightCoord.x) / 2,
            (topCoord.y + bottomCoord.y) / 2
        );

        // Calculate offsets based on the difference between currentCoord and centerCoord
        Vector2 offset = currentCoord - centerCoord;

        // Normalize the offsets to a range of -1 to 1
        Vector2 normalizedOffset = new Vector2(
            -(Mathf.InverseLerp(leftCoord.x, rightCoord.x, currentCoord.x) - 0.5f), // Negate X to fix left/right inversion
            -(Mathf.InverseLerp(topCoord.y, bottomCoord.y, currentCoord.y) - 0.5f) // Negate Y to fix up/down inversion
        );

        // Map the normalized offsets to the desired range
        float mappedX = Mathf.Lerp(-50, 50, normalizedOffset.x + 0.5f); // Map x to -50 to 50
        float mappedY = Mathf.Lerp(-80, 50, normalizedOffset.y + 0.5f);   // Map y to 0 to 50

        // Apply the offsets to the fish bank's position
        Vector3 newPosition = initialPosition + new Vector3(mappedX, mappedY, 0);
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
}