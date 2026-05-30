using UnityEngine;

// Minimal LavaKid stub
public class LavaKid : MonoBehaviour
{
    public int x, y, n;
    public SpriteRenderer sprite;
    public void Hello() {}
    public void GoodBye() { Destroy(gameObject); }
    public void Wei(int len) { n = len; }
}
