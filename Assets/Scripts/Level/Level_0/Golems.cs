using UnityEngine;

// APK SnowMan.cs: snow golem that spawns and floats away
public class SnowMan : MonoBehaviour
{
    private void Start()
    {
        var sr = gameObject.AddComponent<SpriteRenderer>();
        // Try to load snowman sprite sheet
        var sprites = Resources.LoadAll<Sprite>("SnowMan");
        if (sprites != null && sprites.Length > 0)
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
        else
        {
            // Fallback: use snow block texture (id=54)
            var blocks = Resources.LoadAll<Sprite>("Sprites/Blocks");
            foreach (var s in blocks)
                if (s.name == "Blocks_54") { sr.sprite = s; break; }
        }

        // APK: random velocity to float away
        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = -0.5f; // Float upward
        rb.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(1f, 3f));
        rb.angularVelocity = Random.Range(-30f, 30f);

        Destroy(gameObject, 5f);
    }
}

// APK ironMan.cs: iron golem
public class IronMan : MonoBehaviour
{
    private void Start()
    {
        var sr = gameObject.AddComponent<SpriteRenderer>();
        var sprites = Resources.LoadAll<Sprite>("IronMan");
        if (sprites != null && sprites.Length > 0)
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
        else
        {
            var blocks = Resources.LoadAll<Sprite>("Sprites/Blocks");
            foreach (var s in blocks)
                if (s.name == "Blocks_55") { sr.sprite = s; break; }
        }

        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 6f));

        Destroy(gameObject, 5f);
    }
}
