using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Geometry-Dash "Wave" style control: holding (touch / click / space) flies the dart
/// up at a constant speed; releasing dives it down at the same speed. The world scrolls
/// past, so the dart only moves vertically. Touching the top/bottom edge, a wall, or a
/// spike is fatal. A kinematic Rigidbody2D + collider lets it receive obstacle triggers.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DartController : MonoBehaviour
{
    [SerializeField] private float waveSpeed = 5.5f;   // vertical units / second
    [SerializeField] private float tiltAngle = 35f;    // nose tilt up/down (degrees)
    [SerializeField] private float tiltLerp = 14f;
    [SerializeField] private float edgeMargin = 0.15f; // fatal margin from screen top/bottom
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite rainbowSprite;

    private SpriteRenderer sr;
    private bool alive;
    private float halfHeight;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>Reset and hand control to the player at the given start position.</summary>
    public void Begin(Vector2 startPos)
    {
        transform.position = new Vector3(startPos.x, startPos.y, 0f);
        transform.rotation = Quaternion.identity;
        var cam = Camera.main;
        halfHeight = cam != null ? cam.orthographicSize : 5f;
        ApplyEquippedSprite();
        alive = true;
    }

    public void StopControl() { alive = false; }

    public void ApplyEquippedSprite()
    {
        if (sr == null) return;
        sr.sprite = (SaveData.EquippedDart == 1 && rainbowSprite != null) ? rainbowSprite : defaultSprite;
    }

    private static bool Held()
    {
        var p = Pointer.current;
        if (p != null && p.press.isPressed) return true;
        var k = Keyboard.current;
        if (k != null && k.spaceKey.isPressed) return true;
        return false;
    }

    void Update()
    {
        if (!alive) return;

        float dir = Held() ? 1f : -1f;
        Vector3 pos = transform.position;
        pos.y += dir * waveSpeed * Time.deltaTime;

        bool hitEdge = false;
        if (pos.y >= halfHeight - edgeMargin) { pos.y = halfHeight - edgeMargin; hitEdge = true; }
        else if (pos.y <= -halfHeight + edgeMargin) { pos.y = -halfHeight + edgeMargin; hitEdge = true; }
        transform.position = pos;

        float targetTilt = dir > 0f ? tiltAngle : -tiltAngle;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(0f, 0f, targetTilt), 1f - Mathf.Exp(-tiltLerp * Time.deltaTime));

        if (hitEdge) Die();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (alive) Die();
    }

    private void Die()
    {
        if (!alive) return;
        alive = false;
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
    }
}
