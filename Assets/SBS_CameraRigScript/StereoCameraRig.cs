using UnityEngine;

[ExecuteInEditMode]
public class StereoCameraRig : MonoBehaviour
{
    public float eyeDistance = 0.06f; // in meters

    public Camera leftEyeCamera;
    public Camera rightEyeCamera;

    void Update()
    {
        if (leftEyeCamera != null)
            leftEyeCamera.transform.localPosition = new Vector3(-eyeDistance / 2f, 0, 0);

        if (rightEyeCamera != null)
            rightEyeCamera.transform.localPosition = new Vector3(eyeDistance / 2f, 0, 0);
    }
}
