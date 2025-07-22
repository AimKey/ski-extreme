using UnityEngine;
using Unity.Cinemachine;

public class CameraZoomEffect : MonoBehaviour
{
    public CinemachineCamera cinemachineCam;
    public CinemachineFollow cinemachineFollow;

    [Header("Zoom Settings")]
    public float zoomSpeed = 3f;
    public float zoomMultiplier = 1.5f;

    [Header("Follow Offset Settings")]
    public float followOffsetRight = 5f;
    public float followOffsetBottom = 2f;

    private float targetSize;
    private float originalSize;
    private float zoomOutSize;

    private Vector3 originalOffset;
    private Vector3 zoomOffset;
    private Vector3 targetOffset;

    public static CameraZoomEffect Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        cinemachineCam = GetComponent<CinemachineCamera>();
        cinemachineFollow = GetComponent<CinemachineFollow>();

        originalSize = cinemachineCam.Lens.OrthographicSize;
        zoomOutSize = originalSize * zoomMultiplier;
        targetSize = originalSize;

        originalOffset = cinemachineFollow.FollowOffset;
        zoomOffset = originalOffset + new Vector3(followOffsetRight, followOffsetBottom, 0f);
        targetOffset = originalOffset;
    }

    void Update()
    {
        // Smoothly interpolate the size
        var lens = cinemachineCam.Lens;
        lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, targetSize, Time.deltaTime * zoomSpeed);
        cinemachineCam.Lens = lens;

        // Smoothly interpolate the follow offset
        cinemachineFollow.FollowOffset = Vector3.Lerp(cinemachineFollow.FollowOffset, targetOffset, Time.deltaTime * zoomSpeed);
    }

    public void ZoomOut()
    {
        targetSize = zoomOutSize;
        targetOffset = zoomOffset;
    }

    public void ResetZoom()
    {
        targetSize = originalSize;
        targetOffset = originalOffset;
    }
}
