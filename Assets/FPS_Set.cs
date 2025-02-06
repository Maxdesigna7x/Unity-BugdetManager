using UnityEngine;

public class FPS_Set : MonoBehaviour
{
    public int FPS_Target = 60;
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = FPS_Target;
    }
}
