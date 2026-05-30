using UnityEngine;

// APK White.cs: landing white flash that fades out
public class White : MonoBehaviour
{
    public SpriteRenderer s;
    private float a = 1f;

    private void Start()
    {
        if (s == null) s = GetComponent<SpriteRenderer>();
        if (s != null) s.color = new Color(1f, 1f, 1f, 1f);
    }

    private void FixedUpdate()
    {
        a -= 0.1f;
        if (a >= 0f)
        {
            if (s != null) s.color = new Color(1f, 1f, 1f, a);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
