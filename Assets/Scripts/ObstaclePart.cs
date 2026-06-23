using UnityEngine;

/// <summary>
/// One scrolling obstacle segment. Moves left while the game is playing, awards a point
/// once it passes the player, and despawns off the left edge. Trigger colliders are added
/// to each child sprite at init so the dart dies on contact with any wall or spike.
/// </summary>
public class ObstaclePart : MonoBehaviour
{
    private float speed;
    private float despawnX;
    private bool scored;

    public void Init(float speed, float despawnX)
    {
        this.speed = speed;
        this.despawnX = despawnX;
        scored = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Auto-fit a trigger collider to every sprite (walls + spikes).
        foreach (var spr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (spr.GetComponent<Collider2D>() == null)
            {
                var box = spr.gameObject.AddComponent<BoxCollider2D>(); // sizes to the sprite
                box.isTrigger = true;
            }
        }
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;

        transform.position += Vector3.left * speed * Time.deltaTime;

        if (!scored && transform.position.x < gm.PlayerX)
        {
            scored = true;
            gm.AddScore();
        }
        if (transform.position.x < despawnX) Destroy(gameObject);
    }
}
