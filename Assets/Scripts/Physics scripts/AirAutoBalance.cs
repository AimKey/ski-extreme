// AirSpinBalance.cs – v2.1 (compile‑clean, slope‑aware auto‑balance)
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SlideController))]
[DefaultExecutionOrder(150)]
public class AirSpinBalance : MonoBehaviour
{
    // ───── Spin while cruising ─────
    [Header("Cruise spin")]
    [Tooltip("Degrees per second; negative = clockwise")]
    [SerializeField] float cruiseRate = -90f;
    [Tooltip("Maximum nose‑down angle allowed (deg)")]
    [SerializeField] float maxNoseDown = 60f;

    // ───── Slope check ─────
    [Header("Slope alignment")]
    [Tooltip("Metres to probe forward & down for the landing surface")]
    [SerializeField] float lookAhead = 3f;
    [Tooltip("Stop spinning when board‑to‑slope ≤ this many degrees")]
    [SerializeField] float alignTolerance = 6f;
    [SerializeField] float maxGroundAngle = 65f;
    [SerializeField] LayerMask groundMask = ~0;

    // ───── Stability ─────
    [Space, Tooltip("Seconds after last ground contact before spin starts")]
    [SerializeField] float leaveGroundDelay = 0.08f;

    // ───── Internals ─────
    Rigidbody2D rb;
    SlideController slide;
    float airTimer;

    void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        slide = GetComponent<SlideController>();
    }

    void FixedUpdate()
    {
        bool grounded = slide.IsGrounded;
        airTimer = grounded ? 0f : airTimer + Time.fixedDeltaTime;

        bool flipping = Input.GetKey(KeyCode.Space);        // trick button held?
        bool cruise   = !grounded && !flipping && airTimer >= leaveGroundDelay;

        if (!cruise) return;

        /* 1 ── look ahead to find the landing slope */
        Vector2 probeDir = (Vector2.down + rb.velocity.normalized * 0.35f).normalized; // use velocity, not linearVelocity
        RaycastHit2D hit = Physics2D.Raycast(rb.position, probeDir, lookAhead, groundMask);

        bool slopeValid = hit.collider != null &&                 // collider must exist
                          Vector2.Angle(hit.normal, Vector2.up) <= maxGroundAngle;

        float slopeDeg = slopeValid
            ? Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f
            : float.NaN;

        /* 2 ── decide whether to spin this frame */
        bool keepSpinning = true;
        if (slopeValid)
        {
            float delta = Mathf.Abs(Mathf.DeltaAngle(rb.rotation, slopeDeg));
            keepSpinning = delta > alignTolerance;
        }

        /* 3 ── perform spin step if still needed */
        if (keepSpinning)
        {
            float nextRot = rb.rotation + cruiseRate * Time.fixedDeltaTime;

            // clamp to avoid excessive nose dives
            float noseDown = Mathf.DeltaAngle(0f, nextRot);
            if (noseDown < -maxNoseDown) nextRot = -maxNoseDown;

            rb.MoveRotation(nextRot);                        // applies rotation respecting physics
        }
        // else: aligned within tolerance → leave rotation untouched
    }
}
