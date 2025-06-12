using UnityEngine;

public class AnaglyphController : MonoBehaviour
{
    public Camera leftEyeCam;
    public Camera rightEyeCam;
    public Material anaglyphMaterial;
    
    void Start()
    {
        // Automatically set textures if not assigned
        if (anaglyphMaterial != null)
        {
            if (leftEyeCam.targetTexture != null)
                anaglyphMaterial.SetTexture("_LeftTex", leftEyeCam.targetTexture);
            
            if (rightEyeCam.targetTexture != null)
                anaglyphMaterial.SetTexture("_RightTex", rightEyeCam.targetTexture);
        }
    }
    
    void Update()
    {
        // Update IPD from your StereoCameraRig
        float currentIPD = 0.06f;
        leftEyeCam.transform.localPosition = new Vector3(-currentIPD/2, 0, 0);
        rightEyeCam.transform.localPosition = new Vector3(currentIPD/2, 0, 0);
    }
}