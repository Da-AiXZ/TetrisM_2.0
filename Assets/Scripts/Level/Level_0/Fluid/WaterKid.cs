using UnityEngine;

// Minimal WaterKid stub - APK uses prefab-based visuals
public class WaterKid : MonoBehaviour
{
    public int x, y, n;
    public SpriteRenderer sprite;
    public void Hello() {}
    public void GoodBye() { Destroy(gameObject); }
    public void Wei(int len) { n = len; }
}
