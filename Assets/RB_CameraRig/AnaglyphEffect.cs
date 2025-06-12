using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class AnaglyphEffect : MonoBehaviour
{
    public Camera leftEyeCamera;
    public Camera rightEyeCamera;
    public Material anaglyphMaterial;

    private RenderTexture leftTex;
    private RenderTexture rightTex;

    void Start()
    {
        // Initialize render textures
        leftTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        rightTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        
        leftEyeCamera.targetTexture = leftTex;
        rightEyeCamera.targetTexture = rightTex;
    }

    void OnDestroy()
    {
        // Clean up render textures
        if (leftTex != null) leftTex.Release();
        if (rightTex != null) rightTex.Release();
    }

    void Update()
    {
        // Manually render the cameras each frame
        leftEyeCamera.Render();
        rightEyeCamera.Render();
        
        // Update the material's textures
        anaglyphMaterial.SetTexture("_LeftTex", leftTex);
        anaglyphMaterial.SetTexture("_RightTex", rightTex);
    }
}