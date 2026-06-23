using UnityEngine;

/// <summary>
/// Spawns obstacle segments at the right edge at a fixed spacing, each with its gap
/// placed at a random height, and scrolls them left. Per-prefab gap centers ensure each
/// segment's opening lands inside the camera view regardless of how the prefab is built.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] partPrefabs;
    [SerializeField] private float[] gapCenterLocalY;      // parallel to partPrefabs
    [SerializeField] private float scrollSpeed = 3.5f;
    [SerializeField] private float spawnSpacing = 3.0f;    // world units between segments
    [SerializeField] private float firstSpawnDelay = 2.5f; // units of travel before first segment
    [SerializeField] private float spawnX = 3.8f;
    [SerializeField] private float despawnX = -4.5f;
    [SerializeField] private Vector2 gapYRange = new Vector2(-2.2f, 2.2f);

    private float distanceUntilNext;

    public float ScrollSpeed => scrollSpeed;

    public void ResetSpawner()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        distanceUntilNext = firstSpawnDelay;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;
        if (partPrefabs == null || partPrefabs.Length == 0) return;

        distanceUntilNext -= scrollSpeed * Time.deltaTime;
        if (distanceUntilNext <= 0f)
        {
            Spawn();
            distanceUntilNext += spawnSpacing;
        }
    }

    private void Spawn()
    {
        int i = Random.Range(0, partPrefabs.Length);
        var prefab = partPrefabs[i];
        if (prefab == null) return;

        float gapCenter = (gapCenterLocalY != null && i < gapCenterLocalY.Length) ? gapCenterLocalY[i] : 0f;
        float targetGapY = Random.Range(gapYRange.x, gapYRange.y);

        var go = Instantiate(prefab, transform);
        go.transform.position = new Vector3(spawnX, targetGapY - gapCenter, 0f);

        var part = go.GetComponent<ObstaclePart>();
        if (part == null) part = go.AddComponent<ObstaclePart>();
        part.Init(scrollSpeed, despawnX);
    }
}
