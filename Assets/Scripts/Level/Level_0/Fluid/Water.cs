using System.Collections.Generic;
using UnityEngine;

// APK Water.cs:10×320 sub-grid fluid simulation
// Each column has up to320 sub-layers (16 per block row)
// Particles fall down and spread sideways, alternating direction each second
public class Water : MonoBehaviour
{
    public static Water Instance { get; private set; }

    // water[10,320]: particle count per sub-cell (0-16)
    public static int[,] water = new int[10, 320];
    // showingWater[10,320]: whether a WaterKid is currently visible
    public static int[,] showingWater = new int[10, 320];

    private static int timer = 0;
    private static int direction = 1; //1=right,-1=left, alternates each second
    private static int dirTimer = 0;
    private const int FULL = 16;

    private Dictionary<string, WaterKid> kids = new();

    private void Awake()
    {
        Instance = this;
    }

    // APK: Reset_() - clear all water
    public static void Reset_()
    {
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 320; y++)
            {
                water[x, y] = 0;
                showingWater[x, y] = 0;
            }
        timer = 0;
        direction = 1;
        dirTimer = 0;
    }

    // APK: AddWater(x,y) - add a full block's worth of water
    public static void AddWater(int x, int y)
    {
        int yy = y * 16;
        for (int i = 0; i < 16; i++)
            water[x, yy + i] = 16; // Fill all16 sub-layers
        // Mark top of this column as active for ticking
        if (x >= 0 && x < 10 && yy >= 0 && yy < 320)
            Ground.Instance?.MarkTick(x, y);
    }

    // APK: DeleteWater(x,y) - remove water at grid position
    public static void DeleteWater(int x, int y)
    {
        int yy = y * 16;
        for (int i = 0; i < 16; i++)
            water[x, yy + i] = 0;
    }

    // APK: Tick() - simulate one step of fluid physics
    public void Simulate()
    {
        timer++;
        if (timer < 50) return; // APK: runs every50 FixedUpdate ticks (~1s)
        timer = 0;

        // Alternate spread direction each second
        dirTimer++;
        if (dirTimer >= 50)
        {
            dirTimer = 0;
            direction = -direction;
        }

        // Process from bottom to top (water falls down)
        for (int y = 0; y < 320; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                int n = water[x, y];
                if (n <= 0) continue;

                int gridY = y / 16;
                int subY = y % 16;

                // Check if this cell is part of a solid block
                if (subY == 0 && gridY < 20)
                {
                    int blockID = Ground.Instance.GetBlockID(new Vector2Int(x, gridY));
                    if (blockID != 0 && blockID != 58 && blockID != 56)
                        continue; // Blocked by solid (non-fluid)
                }

                // Fall down if space below
                if (y > 0 && water[x, y - 1] < FULL)
                {
                    int space = FULL - water[x, y - 1];
                    int move = Mathf.Min(n, space);
                    water[x, y] -= move;
                    water[x, y - 1] += move;
                    continue;
                }

                // Spread sideways (alternating direction)
                int nx = x + direction;
                if (nx >= 0 && nx < 10 && water[nx, y] < FULL)
                {
                    int space = FULL - water[nx, y];
                    int move = Mathf.Min(n / 2, space);
                    if (move > 0)
                    {
                        water[x, y] -= move;
                        water[nx, y] += move;
                    }
                }

                // Check opposite direction too
                nx = x - direction;
                if (nx >= 0 && nx < 10 && water[nx, y] < FULL && water[x, y] > 0)
                {
                    int space = FULL - water[nx, y];
                    int move = Mathf.Min(water[x, y] / 2, space);
                    if (move > 0)
                    {
                        water[x, y] -= move;
                        water[nx, y] += move;
                    }
                }
            }
        }

        // Update grid blocks: if a column is full → water block(58); if empty → remove
        for (int x = 0; x < 10; x++)
        {
            for (int gy = 0; gy < 20; gy++)
            {
                int yy = gy * 16;
                bool full = true;
                bool empty = true;
                for (int i = 0; i < 16; i++)
                {
                    if (water[x, yy + i] < FULL) full = false;
                    if (water[x, yy + i] > 0) empty = false;
                }

                int existing = Ground.Instance.GetBlockID(new Vector2Int(x, gy));
                if (full && existing == 0)
                {
                    Ground.Instance.SetBlock(new Vector2Int(x, gy), 58);
                }
                else if (empty && existing == 58)
                {
                    Ground.Instance.SetBlock(new Vector2Int(x, gy), 0);
                }
            }
        }

        RefreshVisuals();
    }

    // Create/update WaterKid visual objects
    private void RefreshVisuals()
    {
        HashSet<string> seen = new();
        for (int x = 0; x < 10; x++)
        {
            int runStart = -1;
            int runLen = 0;
            for (int y = 0; y < 320; y++)
            {
                if (water[x, y] > 0)
                {
                    if (runStart < 0) runStart = y;
                    runLen++;
                }
                else
                {
                    if (runLen > 0 && runStart >= 0)
                    {
                        string key = $"{x},{runStart}";
                        seen.Add(key);
                        UpdateKid(key, x, runStart, runLen);
                        runStart = -1;
                        runLen = 0;
                    }
                }
            }
            if (runLen > 0 && runStart >= 0)
            {
                string key = $"{x},{runStart}";
                seen.Add(key);
                UpdateKid(key, x, runStart, runLen);
            }
        }

        // Remove unseen kids
        var toRemove = new List<string>();
        foreach (var kv in kids)
            if (!seen.Contains(kv.Key))
                toRemove.Add(kv.Key);
        foreach (var key in toRemove)
        {
            if (kids[key] != null)
            {
                kids[key].GoodBye();
                Destroy(kids[key].gameObject);
            }
            kids.Remove(key);
        }
    }

    private void UpdateKid(string key, int x, int y, int len)
    {
        if (!kids.ContainsKey(key))
        {
            var go = new GameObject($"Water{x},{y}");
            go.transform.SetParent(transform);
            var kid = go.AddComponent<WaterKid>();
            kid.x = x;
            kid.y = y;
            kid.n = len;
            kid.sprite = go.AddComponent<SpriteRenderer>();
            // Use water sprite (id=58)
            var sp = Resources.Load<Sprite>("Sprites/Blocks");
            if (sp == null)
            {
                var all = Resources.LoadAll<Sprite>("Sprites/Blocks");
                foreach (var s in all)
                    if (s.name == "Blocks_58")
                    {
                        kid.sprite.sprite = s;
                        break;
                    }
            }
            kid.sprite.color = new Color(0.27f, 0.44f, 0.91f, 0.7f); // Blue water tint
            kid.Hello();
            kids[key] = kid;
        }
        else if (kids[key] != null && kids[key].n != len)
        {
            kids[key].Wei(len);
        }
    }
}
