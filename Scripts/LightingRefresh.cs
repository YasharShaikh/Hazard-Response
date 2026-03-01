using UnityEngine;
using UnityEngine.Rendering;

public class LightingRefresh : MonoBehaviour
{
    [Header("Lighting Settings")]
    public float ambientIntensity = 1f;
    public Color ambientColor = Color.white;
    
    void Start()
    {
        ForceGoodLighting();
    }

    void ForceGoodLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.shadowDistance = 150f;
        
        QualitySettings.pixelLightCount = 4;
        QualitySettings.realtimeReflectionProbes = true;
        
        DynamicGI.UpdateEnvironment();
        
        Debug.Log("Lighting forced to realtime settings");
    }
}
