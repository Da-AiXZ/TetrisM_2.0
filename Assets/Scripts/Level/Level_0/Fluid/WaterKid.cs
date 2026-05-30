using UnityEngine;

// APK WaterKid.cs: visual water column
public class WaterKid : MonoBehaviour
{
    public int x, y, n;
    public SpriteRenderer sprite;

    public void Hello()
    {
        name = "Water" + x + "," + y;
        sprite.size = new Vector2(0.16f, 0.01f * n);
        transform.position = new Vector2(-2.8f + x * 0.4f, -4f + y * 0.025f + 0.025f * (n / 2));
        for (int i = 0; i < n && y + i < 320; i++)
            Water.showingWater[x, y + i] = 1;
    }

    public void GoodBye()
    {
        for (int i = 0; i < n && y + i < 320; i++)
            Water.showingWater[x, y + i] = 0;
    }

    public void Wei(int nn)
    {
        if (nn <= 0) { GoodBye(); Destroy(gameObject); return; }
        for (int i = 0; i < n && y + i < 320; i++)
            Water.showingWater[x, y + i] = 0;
        n = nn;
        Hello();
    }
}
