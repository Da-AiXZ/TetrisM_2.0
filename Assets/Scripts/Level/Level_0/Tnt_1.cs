using UnityEngine;

// APK Tnt_1.cs: TNT explosion entity - bounce, flash7 times, explode
public class Tnt_1 : MonoBehaviour
{
    public int cx, cy; // Explosion center
    private int flashCount = 0;
    private float flashTimer = 0f;
    private const float FLASH_INTERVAL = 0.15f;
    private const int MAX_FLASHES = 7;
    private bool exploded = false;
    private SpriteRenderer sr;
    private float lifetime = 0f;
    private Vector2 velocity;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        // Use TNT sprite (id=1)
        var all = Resources.LoadAll<Sprite>("Sprites/Blocks");
        foreach (var s in all)
            if (s.name == "Blocks_1") { sr.sprite = s; break; }
        transform.localScale = new Vector3(2f, 2f, 1f);
        // Random initial velocity
        velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 6f));
        AudioManager.Play("fuse");
    }

    private void FixedUpdate()
    {
        if (exploded) return;

        flashTimer += Time.fixedDeltaTime;
        lifetime += Time.fixedDeltaTime;

        // Bounce physics
        velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime * 2f;
        transform.position += (Vector3)velocity * Time.fixedDeltaTime;

        // Flash red/white
        if (flashTimer >= FLASH_INTERVAL)
        {
            flashTimer = 0f;
            flashCount++;
            if (flashCount >= MAX_FLASHES)
            {
                Explode();
            }
            else
            {
                sr.color = (flashCount % 2 == 0) ? Color.white : Color.red;
            }
        }
    }

    private void Explode()
    {
        exploded = true;
        sr.color = Color.white;

        // Camera shake
        CameraShake.Shake(0.3f, 0.3f);

        // Play explosion sound
        int r = Random.Range(1, 5);
        AudioManager.Play($"explode{r}");

        // Destroy blocks in5×4 area (APK: x-2 to x+2, y-2 to y+1)
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 1; dy++)
            {
                int tx = cx + dx, ty = cy + dy;
                if (tx < 0 || tx >= 10 || ty < 0 || ty >= 20) continue;
                int id = Ground.Instance.GetBlockID(new Vector2Int(tx, ty));
                if (id <= 0) continue;
                // Immune: bedrock(9), memory(12,13,14), command(57)
                if (id == 9 || id == 12 || id == 13 || id == 14 || id == 57) continue;
                // Chain TNT
                if (id == 1)
                    SpawnTNT(tx, ty);
                Ground.Instance.SetBlock(new Vector2Int(tx, ty), 0);
            }
        }

        // Spawn particles
        SpawnParticles();

        // Self-destruct
        Destroy(gameObject, 0.5f);
    }

    private void SpawnTNT(int x, int y)
    {
        var go = new GameObject("Tnt_chain");
        go.transform.position = new Vector2(-2.8f + x * 0.4f, -3.8f + y * 0.4f);
        var tnt = go.AddComponent<Tnt_1>();
        tnt.cx = x; tnt.cy = y;
    }

    private void SpawnParticles()
    {
        for (int i = 0; i < 8; i++)
        {
            var go = new GameObject("Tnt_particle");
            go.transform.SetParent(transform);
            go.AddComponent<Tnt_2>();
            go.AddComponent<DestoryTimmer>().time = 1f;
        }
    }
}

// APK Tnt_2.cs: TNT particle
public class Tnt_2 : MonoBehaviour
{
    private void Start()
    {
        transform.localPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        transform.localEulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
        float s = Random.Range(1f, 3f);
        transform.localScale = new Vector3(s, s, 1f);
        var sr = gameObject.AddComponent<SpriteRenderer>();
        var all = Resources.LoadAll<Sprite>("Sprites/Blocks");
        foreach (var sp in all)
            if (sp.name == "Blocks_1") { sr.sprite = sp; break; }
        sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
}

// APK DestoryTimmer.cs: simple countdown destroy
public class DestoryTimmer : MonoBehaviour
{
    public float time = 1f;
    private float timer;
    private void Update() { timer += Time.deltaTime; if (timer > time) Destroy(gameObject); }
}
