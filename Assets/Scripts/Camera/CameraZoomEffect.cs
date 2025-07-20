using Unity.Cinemachine;
using UnityEngine;

public class CameraZoomEffect : MonoBehaviour
{
    public CinemachineCamera cinemachineCam;
    public float zoomSpeed = 3f;
    public float zoomMultiplier = 1.5f; // Multiplier for zoom out size

    private float targetSize;
    private float originalSize;
    private float zoomOutSize;

    public static CameraZoomEffect Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        cinemachineCam = GetComponent<CinemachineCamera>();
        originalSize = cinemachineCam.Lens.OrthographicSize;
        targetSize = originalSize;
        zoomOutSize = originalSize * zoomMultiplier;
    }

    void Update()
    {
        float current = cinemachineCam.Lens.OrthographicSize;
        cinemachineCam.Lens.OrthographicSize = Mathf.Lerp(current, targetSize, Time.deltaTime * zoomSpeed);
    }

    public void ZoomOut()
    {
        targetSize = zoomOutSize;
    }
    public void ResetZoom()
    {
        targetSize = originalSize;
    }
}
