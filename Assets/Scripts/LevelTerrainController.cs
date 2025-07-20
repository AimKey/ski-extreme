using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelTerrainController : MonoBehaviour
{
    [SerializeField] private SurfaceEffector2D surfaceEffector;

    [FormerlySerializedAs("defaultSpeed")] public float baseSpeed = 50f;

    public readonly float currentSpeed;

    public LevelTerrainController()
    {
        currentSpeed = baseSpeed;
    }

    private void Start()
    {
        if (surfaceEffector == null)
        {
            surfaceEffector = GetComponent<SurfaceEffector2D>();
            if (surfaceEffector == null)
            {
                Debug.LogError("SurfaceEffector2D component is missing on the LevelTerrainController GameObject.");
            }
        }
        surfaceEffector.speed = baseSpeed;
    }

    private void Update()
    {
    }

    public void SetSurfaceSpeed(float speed)
    {
        surfaceEffector.speed = speed;
    }

    public void ResetSurfaceSpeed()
    {
        surfaceEffector.speed = baseSpeed;
    }
}