using System.Collections.Generic;
using UnityEngine;

// APK Lava.cs: same as Water but1/3 speed, water+lava collision→cobblestone
public class Lava : MonoBehaviour
{
    public static Lava Instance { get; private set; }

    public static int[,] lava = new int[10, 320];
    public static int[,] showingLava = new int[10, 320];

    private static int timer = 0;
    private static int direction = 1;
    private static int dirTimer = 0;
    private const int FULL = 16;
    private const int SPEED = 150; //3x slower than water (50→150)

    private Dictionary<string, LavaKid> kids = new();

    private void Awake() { Instance = this; }

    public static void Reset_()
    {
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 320; y++)
            {
                lava[x, y] = 0;
                showingLava[x, y] = 0;
            }
        timer = 0;
    }

    public static void AddWater(int x, int y)
    {
        int yy = y * 16;
        for (int i = 0; i < 16; i++)
            lava[x, yy + i] = 16;
        if (x >= 0 && x < 10)
            Ground.Instance?.MarkTick(x, y);
    }

    public static void DeleteWater(int x, int y)
    {
        int yy = y * 16;
        for (int i = 0; i < 16; i++)
            lava[x, yy + i] = 0;
    }

    public void Simulate()
    {
        timer++;
        if (timer < SPEED) return;
        timer = 0;

        dirTimer++;
        if (dirTimer >= 50) { dirTimer = 0; direction = -direction; }

        for (int y = 0; y < 320; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                int n = lava[x, y];
                if (n <= 0) continue;

                int gridY = y / 16;
                int subY = y % 16;
                if (subY == 0 && gridY < 20)
                {
                    int blockID = Ground.Instance.GetBlockID(new Vector2Int(x, gridY));
                    if (blockID != 0 && blockID != 58 && blockID != 56)
                        continue;
                }

                if (y > 0 && lava[x, y - 1] < FULL)
                {
                    int space = FULL - lava[x, y - 1];
                    int move = Mathf.Min(n, space);
                    lava[x, y] -= move;
                    lava[x, y - 1] += move;
                    continue;
                }

                int nx = x + direction;
                if (nx >= 0 && nx < 10 && lava[nx, y] < FULL)
                {
                    int space = FULL - lava[nx, y];
                    int move = Mathf.Min(n / 2, space);
                    if (move > 0) { lava[x, y] -= move; lava[nx, y] += move; }
                }

                nx = x - direction;
                if (nx >= 0 && nx < 10 && lava[nx, y] < FULL && lava[x, y] > 0)
                {
                    int space = FULL - lava[nx, y];
                    int move = Mathf.Min(lava[x, y] / 2, space);
                    if (move > 0) { lava[x, y] -= move; lava[nx, y] += move; }
                }
            }
        }

        // Update grid and check water+lava collision
        for (int x = 0; x < 10; x++)
        {
            for (int gy = 0; gy < 20; gy++)
            {
                int yy = gy * 16;
                bool full = true, empty = true;
                for (int i = 0; i < 16; i++)
                {
                    if (lava[x, yy + i] < FULL) full = false;
                    if (lava[x, yy + i] > 0) empty = false;
                }

                int existing = Ground.Instance.GetBlockID(new Vector2Int(x, gy));
                if (full && existing == 0)
                {
                    // Check if water exists here too → cobblestone!
                    bool hasWater = false;
                    for (int i = 0; i < 16; i++)
                        if (Water.water[x, yy + i] >= FULL) hasWater = true;
                    if (hasWater)
                    {
                        Ground.Instance.SetBlock(new Vector2Int(x, gy), 18); // Cobblestone
                        AudioManager.Play("fizz");
                        for (int i = 0; i < 16; i++)
                        {
                            lava[x, yy + i] = 0;
                            Water.water[x, yy + i] = 0;
                        }
                    }
                    else
                    {
                        Ground.Instance.SetBlock(new Vector2Int(x, gy), 56);
                    }
                }
                else if (empty && existing == 56)
                {
                    Ground.Instance.SetBlock(new Vector2Int(x, gy), 0);
                }
            }
        }

        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        HashSet<string> seen = new();
        for (int x = 0; x < 10; x++)
        {
            int runStart = -1, runLen = 0;
            for (int y = 0; y < 320; y++)
            {
                if (lava[x, y] > 0) { if (runStart < 0) runStart = y; runLen++; }
                else
                {
                    if (runLen > 0 && runStart >= 0)
                    {
                        string key = $"{x},{runStart}"; seen.Add(key);
                        UpdateKid(key, x, runStart, runLen);
                        runStart = -1; runLen = 0;
                    }
                }
            }
            if (runLen > 0 && runStart >= 0)
            {
                string key = $"{x},{runStart}"; seen.Add(key);
                UpdateKid(key, x, runStart, runLen);
            }
        }
        var toRemove = new List<string>();
        foreach (var kv in kids)
            if (!seen.Contains(kv.Key)) toRemove.Add(kv.Key);
        foreach (var key in toRemove)
        {
            if (kids[key] != null) { kids[key].GoodBye(); Destroy(kids[key].gameObject); }
            kids.Remove(key);
        }
    }

    private void UpdateKid(string key, int x, int y, int len)
    {
        if (!kids.ContainsKey(key))
        {
            var go = new GameObject($"Lava{x},{y}");
            go.transform.SetParent(transform);
            var kid = go.AddComponent<LavaKid>();
            kid.x = x; kid.y = y; kid.n = len;
            kid.sprite = go.AddComponent<SpriteRenderer>();
            var all = Resources.LoadAll<Sprite>("Sprites/Blocks");
            foreach (var s in all)
                if (s.name == "Blocks_56") { kid.sprite.sprite = s; break; }
            kid.sprite.color = new Color(0.86f, 0.43f, 0.11f, 0.8f);
            kid.Hello();
            kids[key] = kid;
        }
        else if (kids[key] != null && kids[key].n != len)
        {
            kids[key].Wei(len);
        }
    }
}
