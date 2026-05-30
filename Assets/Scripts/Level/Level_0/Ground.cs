using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public static Ground Instance { get; private set; }

    public Block[,] Blocks { get; private set; } = new Block[10, 20];
    public GameObject[] blockPrefabs;
    private static Sprite[] _loadedSprites;
    
    private Sprite GetBlockSprite(int id)
    {
        if (_loadedSprites == null)
        {
            var all = Resources.LoadAll<Sprite>("Sprites/Blocks");
            _loadedSprites = all;
        }
        string name = $"Blocks_{id}";
        foreach (var s in _loadedSprites)
        {
            if (s.name == name) return s;
        }
        return null;
    }

    public delegate void Call();
    public Call UpdateCall;
    public delegate void Clear(int count);
    public Clear ClearCall;

    private void Awake()
    {
        Instance = this;
        Blocks = new Block[Frame.xSize, Frame.ySize];
    }

    private float timmer = 0;
    private const float tickTime= 0.1f;

    private void Update()
    {
        UpdateCall?.Invoke();

        timmer += Time.deltaTime;
        while (timmer >= tickTime)
        {
            timmer -= tickTime;
            DoTick();
            CheckFull();
            RunFluidSim();
        }
    }

    // === Batch1: APK Minecraft Physics (ported from APK Blocks.Tick) ===
    private Block pendingRestore = null;  // block that triggered memory restore

    private void DoTick()
    {
        // If memory restore is pending, handle it
        if (pendingRestore != null)
        {
            pendingRestore.memoryTimer += tickTime;
            if (pendingRestore.memoryRestorePhase == 0 && pendingRestore.memoryTimer >= 0.4f)
            {
                // Phase0: Destroy all blocks
                for (int xx = 0; xx < Frame.xSize; xx++)
                    for (int yy = 0; yy < Frame.ySize; yy++)
                        if (Blocks[xx, yy] != null && Blocks[xx, yy] != pendingRestore)
                            SetBlock(new Vector2Int(xx, yy), 0);
                pendingRestore.memoryRestorePhase = 1;
                pendingRestore.memoryTimer = 0f;
            }
            else if (pendingRestore.memoryRestorePhase == 1 && pendingRestore.memoryTimer >= 0.5f)
            {
                // Phase1: Restore from saved layout
                var saved = pendingRestore.savedLayout;
                if (saved != null)
                {
                    for (int xx = 0; xx < 10; xx++)
                        for (int yy = 0; yy < 20; yy++)
                        {
                            int sid = saved[xx, yy];
                            if (sid != 0 && sid != 12 && sid != 13)
                                SetBlock(new Vector2Int(xx, yy), sid);
                        }
                }
                // Replace memory block as id=14
                pendingRestore.ID = 14;
                pendingRestore.SetSprite(GetBlockSprite(14));
                pendingRestore.isMemoryRestoring = false;
                FallingBlock.hadMemoryBlockThisRound = false;
                pendingRestore.memoryRestorePhase = 0;
                pendingRestore = null;
            }
            return; // Don't process normal ticks during restore
        }

        for (int y = 0; y < Frame.ySize; y++)
        {
            for (int x = 0; x < Frame.xSize; x++)
            {
                TickBlock(x, y);
            }
        }
    }

    private void TickBlock(int x, int y)
    {
        Block b = Blocks[x, y];
        if (b == null) return;
        int id = b.ID;

        switch (id)
        {
            case 1: TNT_Check(x, y, b); break;
            case 3: case4: DirtGrass_Check(x, y, b); break;
            case 6: case7: Lamp_Check(x, y, b); break;
            case 8: Sand_Check(x, y, b); break;
            case 11: Oak_Check(x, y, b); break;
            case 12: Memory_Save(x, y, b); break;
            case 15: Target_Check(x, y, b); break;
            case 16: Golem_Check(x, y, b); break;
            case 17: case49: StoneBrick_Check(x, y, b); break;
        }
    }

    // --- id=12: Memory block → save layout ---
    private void Memory_Save(int x, int y, Block b)
    {
        // Save current layout
        b.savedLayout = new int[10, 20];
        for (int xx = 0; xx < 10; xx++)
            for (int yy = 0; yy < 20; yy++)
                b.savedLayout[xx, yy] = GetBlockID(new Vector2Int(xx, yy));
        b.savedX = x;
        b.savedY = y;
        b.ID = 13;
        b.SetSprite(GetBlockSprite(13));
    }

    // --- id=15: Target → redstone after0.5s ---
    private void Target_Check(int x, int y, Block b)
    {
        if (b.targetTimer < 0f)
        {
            b.targetTimer = 0.5f;
        }
        else
        {
            b.targetTimer -= tickTime;
            if (b.targetTimer <= 0f)
            {
                b.ID = 2; // becomes redstone
                b.SetSprite(GetBlockSprite(2));
                b.targetTimer = -1f;
            }
        }
    }

    // --- id=1: TNT ---
    private void TNT_Check(int x, int y, Block b)
    {
        // Check4 adjacent cells for redstone(2) or lava(56)
        int[] checkX = { x+1, x-1, x, x };
        int[] checkY = { y, y, y+1, y-1 };
        for (int i = 0; i < 4; i++)
        {
            if (checkX[i] >= 0 && checkX[i] < Frame.xSize && checkY[i] >= 0 && checkY[i] < Frame.ySize)
            {
                int adj = GetBlockID(new Vector2Int(checkX[i], checkY[i]));
                if (adj == 2 || adj == 56)
                {
                    // TNT explodes: clear5×4 area
                    ExplodeTNT(x, y, b);
                    return;
                }
            }
        }
    }

    private void ExplodeTNT(int x, int y, Block b)
    {
        // APK: spawn TNT explosion entity
        var tntGO = new GameObject("Tnt_1");
        tntGO.transform.position = new Vector2(-2.8f + x * 0.4f, -3.8f + y * 0.4f);
        var tnt = tntGO.AddComponent<Tnt_1>();
        tnt.cx = x; tnt.cy = y;

        // Camera shake
        CameraShake.Shake(0.3f, 0.3f);

        // Clear blocks in5×4 area (APK pattern)
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -1; dy <= 2; dy++)
            {
                int tx = x + dx, ty = y + dy;
                if (tx >= 0 && tx < Frame.xSize && ty >= 0 && ty < Frame.ySize)
                {
                    int tid = GetBlockID(new Vector2Int(tx, ty));
                    // Immune: bedrock(9), memory(12,13,14), command(57)
                    if (tid == 9 || tid == 12 || tid == 13 || tid == 14 || tid == 57) continue;
                    if (tid == 1) // Chain reaction TNT
                    {
                        SetBlock(new Vector2Int(tx, ty), 0);
                        ExplodeTNT(tx, ty, null);
                    }
                    else
                    {
                        SetBlock(new Vector2Int(tx, ty), 0);
                    }
                }
            }
        }
    }

    // --- id=3↔4: Dirt ↔ Grass ---
    private void DirtGrass_Check(int x, int y, Block b)
    {
        if (b.ID == 3 && GetBlockID(new Vector2Int(x, y+1)) == 0)
        {
            b.ID = 4;
            b.SetSprite(GetBlockSprite(4));
        }
        else if (b.ID == 4 && GetBlockID(new Vector2Int(x, y+1)) != 0)
        {
            b.ID = 3;
            b.SetSprite(GetBlockSprite(3));
        }
    }

    // --- id=6↔7: Redstone lamp ---
    private void Lamp_Check(int x, int y, Block b)
    {
        bool nearRedstone = false;
        int[] checkX = { x+1, x-1, x, x };
        int[] checkY = { y, y, y+1, y-1 };
        for (int i = 0; i < 4; i++)
        {
            if (checkX[i] >= 0 && checkX[i] < Frame.xSize && checkY[i] >= 0 && checkY[i] < Frame.ySize)
            {
                if (GetBlockID(new Vector2Int(checkX[i], checkY[i])) == 2)
                    nearRedstone = true;
            }
        }
        if (b.ID == 6 && nearRedstone)
        {
            b.ID = 7;
            b.SetSprite(GetBlockSprite(7));
        }
        else if (b.ID == 7 && !nearRedstone)
        {
            b.ID = 6;
            b.SetSprite(GetBlockSprite(6));
        }
    }

    // --- id=8: Sand ---
    private void Sand_Check(int x, int y, Block b)
    {
        if (y == 0) return;
        int below = GetBlockID(new Vector2Int(x, y-1));
        if (below == 0 || below == 56 || below == 58)
        {
            // Move sand down
            Blocks[x, y] = null;
            Blocks[x, y-1] = b;
            b.Pos = new Vector2Int(x, y-1);
            b.transform.localPosition = Frame.instance.GetPosV2(new Vector2Int(x, y-1));
            // Note: no MoveTo animation for sand; instantaneous like APK
        }
    }

    // --- id=11: Oak → leaves ---
    private void Oak_Check(int x, int y, Block b)
    {
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i], ny = y + dy[i];
            if (nx >= 0 && nx < Frame.xSize && ny >= 0 && ny < Frame.ySize)
            {
                if (GetBlockID(new Vector2Int(nx, ny)) == 0)
                    SetBlock(new Vector2Int(nx, ny), 10);
            }
        }
    }

    // --- id=16: Pumpkin → golem spawner ---
    private void Golem_Check(int x, int y, Block b)
    {
        // Snow golem:2 snow blocks(54) in a row (vertical or horizontal)
        // Vertical down
        if (y >= 2 && GetBlockID(new Vector2Int(x, y-1)) == 54 && GetBlockID(new Vector2Int(x, y-2)) == 54)
        {
            SetBlock(new Vector2Int(x, y-1), 0); SetBlock(new Vector2Int(x, y-2), 0);
            SetBlock(new Vector2Int(x, y), 0);
            var sm = new GameObject("SnowMan");
            sm.transform.position = new Vector2(-2.8f + x * 0.4f, -3.8f + y * 0.4f);
            sm.AddComponent<SnowMan>();
            AudioManager.Play("snowman_voice");
            return;
        }
        // Vertical up
        if (y+2 < Frame.ySize && GetBlockID(new Vector2Int(x, y+1)) == 54 && GetBlockID(new Vector2Int(x, y+2)) == 54)
        {
            SetBlock(new Vector2Int(x, y+1), 0); SetBlock(new Vector2Int(x, y+2), 0);
            SetBlock(new Vector2Int(x, y), 0);
            return;
        }
        // Horizontal left
        if (x >= 2 && GetBlockID(new Vector2Int(x-1, y)) == 54 && GetBlockID(new Vector2Int(x-2, y)) == 54)
        {
            SetBlock(new Vector2Int(x-1, y), 0); SetBlock(new Vector2Int(x-2, y), 0);
            SetBlock(new Vector2Int(x, y), 0);
            return;
        }
        // Horizontal right
        if (x+2 < Frame.xSize && GetBlockID(new Vector2Int(x+1, y)) == 54 && GetBlockID(new Vector2Int(x+2, y)) == 54)
        {
            SetBlock(new Vector2Int(x+1, y), 0); SetBlock(new Vector2Int(x+2, y), 0);
            SetBlock(new Vector2Int(x, y), 0);
            return;
        }
        // Iron golem: T-shape of iron(55) - all4 patterns from APK
        TryIronGolem(x, y, b);
    }

    private void TryIronGolem(int x, int y, Block b)
    {
        // T-shape down:2 below + left+right below
        if (y >= 2 && x > 0 && x+1 < Frame.xSize &&
            GetBlockID(new Vector2Int(x, y-1)) == 55 && GetBlockID(new Vector2Int(x, y-2)) == 55 &&
            GetBlockID(new Vector2Int(x-1, y-1)) == 55 && GetBlockID(new Vector2Int(x+1, y-1)) == 55)
        {
            SetBlock(new Vector2Int(x, y-1), 0); SetBlock(new Vector2Int(x, y-2), 0);
            SetBlock(new Vector2Int(x-1, y-1), 0); SetBlock(new Vector2Int(x+1, y-1), 0);
            SetBlock(new Vector2Int(x, y), 0);
            return;
        }
        // T-shape up
        if (y+2 < Frame.ySize && x > 0 && x+1 < Frame.xSize &&
            GetBlockID(new Vector2Int(x, y+1)) == 55 && GetBlockID(new Vector2Int(x, y+2)) == 55 &&
            GetBlockID(new Vector2Int(x-1, y+1)) == 55 && GetBlockID(new Vector2Int(x+1, y+1)) == 55)
        {
            SetBlock(new Vector2Int(x, y+1), 0); SetBlock(new Vector2Int(x, y+2), 0);
            SetBlock(new Vector2Int(x-1, y+1), 0); SetBlock(new Vector2Int(x+1, y+1), 0);
            SetBlock(new Vector2Int(x, y), 0);
            return;
        }
        // T-shape right
        if (x+2 < Frame.xSize && y > 0 && y+1 < Frame.ySize &&
            GetBlockID(new Vector2Int(x+1, y)) == 55 && GetBlockID(new Vector2Int(x+2, y)) == 55 &&
            GetBlockID(new Vector2Int(x+1, y+1)) == 55 && GetBlockID(new Vector2Int(x+1, y-1)) == 55)
        {
            SetBlock(new Vector2Int(x+1, y), 0); SetBlock(new Vector2Int(x+2, y), 0);
            SetBlock(new Vector2Int(x+1, y+1), 0); SetBlock(new Vector2Int(x+1, y-1), 0);
            SetBlock(new Vector2Int(x, y), 0);
            return;
        }
        // T-shape left
        if (x >= 2 && y > 0 && y+1 < Frame.ySize &&
            GetBlockID(new Vector2Int(x-1, y)) == 55 && GetBlockID(new Vector2Int(x-2, y)) == 55 &&
            GetBlockID(new Vector2Int(x-1, y+1)) == 55 && GetBlockID(new Vector2Int(x-1, y-1)) == 55)
        {
            SetBlock(new Vector2Int(x-1, y), 0); SetBlock(new Vector2Int(x-2, y), 0);
            SetBlock(new Vector2Int(x-1, y+1), 0); SetBlock(new Vector2Int(x-1, y-1), 0);
            SetBlock(new Vector2Int(x, y), 0);
        }
    }

    // --- id=17↔49: Stone brick ↔ Mossy ---
    private void StoneBrick_Check(int x, int y, Block b)
    {
        int[] checkX = { x+1, x-1, x, x };
        int[] checkY = { y, y, y+1, y-1 };
        for (int i = 0; i < 4; i++)
        {
            if (checkX[i] >= 0 && checkX[i] < Frame.xSize && checkY[i] >= 0 && checkY[i] < Frame.ySize)
            {
                int adj = GetBlockID(new Vector2Int(checkX[i], checkY[i]));
                if (b.ID == 17 && adj == 58) { b.ID = 49; b.SetSprite(GetBlockSprite(49)); return; }
                if (b.ID == 49 && adj == 56) { b.ID = 17; b.SetSprite(GetBlockSprite(17)); return; }
            }
        }
    }
    }

    // APK Water/Lava fluid simulation (called every tick)
    public void MarkTick(int x, int y) { if (x>=0 && x<10 && y>=0 && y<20) ticksToProcess[x,y]=true; }
    private bool[,] ticksToProcess = new bool[10,20];
    private void RunFluidSim()
    {
        if (Water.Instance != null) Water.Instance.Simulate();
        if (Lava.Instance != null) Lava.Instance.Simulate();
    }


    private bool CheckFull()
    {
        List<int> full=new();
        for(int y=0; y<Frame.ySize; y++)
        {
            bool isFull = true;
            bool hasMemory = false;
            for (int x = 0; x < Frame.xSize; x++)
            {
                if (Blocks[x,y]==null|| Blocks[x, y].ID == 36 || Blocks[x, y].ID <= 0)
                {
                    isFull = false;
                    break;
                }
                if (Blocks[x,y] != null && Blocks[x,y].ID == 13)
                    hasMemory = true;
            }
            if (isFull)
            {
                // APK: if full row contains memory block(13), trigger restore
                if (hasMemory)
                {
                    for (int x = 0; x < Frame.xSize; x++)
                    {
                        if (Blocks[x, y] != null && Blocks[x, y].ID == 13)
                        {
                            pendingRestore = Blocks[x, y];
                            pendingRestore.isMemoryRestoring = true;
                            pendingRestore.memoryTimer = 0f;
                            pendingRestore.memoryRestorePhase = 0;
                        }
                    }
                    // Don't clear this row - memory block stays for restore
                    return false;
                }
                full.Add(y);
            }
        }

        if (full.Count == 0)
        {
            return false;
        }


        int fall = 0;
        int[] falls= new int[Frame.ySize];
        int i = 0;
        for (int y = 0; y < Frame.ySize; y++)
        {
            if(full.Count>i&& full[i] == y)
            {
                for (int x = 0; x < Frame.xSize; x++)
                {
                    SetBlock(new Vector2Int(x, full[i]), 0);
                }
                fall ++;
                i++;
            }
            else
            {
                if (fall == 0)
                {
                    continue;
                }
                for (int x = 0; x < Frame.xSize; x++)
                {
                    MoveDown(new Vector2Int(x, y), fall);
                }
            }
        }

        // APK: score = Pow(2,count)*100
        int scoreAdd = (int)Mathf.Pow(2f, full.Count) * 100;
        // APK: camera shake on line clear
        CameraShake.Shake(0f, 0.4f);
        // APK: levelup sound (>=3 lines = levelup2)
        AudioManager.Play(full.Count >= 3 ? "levelup2" : "levelup1");

        ClearCall?.Invoke(full.Count);
        return true;
    }

    private bool MoveDown(Vector2Int start, int length)
    {
        if (!inRange(start) || !inRange(start+Vector2Int.down*length))
        {
            Debug.LogError("Position is out of range!");
            return false;
        }
        if(Blocks[start.x, start.y] == null)
        {
            return false;
        }
        // Bedrock check: if any block in path is bedrock(9), stop
        for(int i = 1; i <= length; i++)
        {
            Vector2Int check = start + Vector2Int.down * i;
            if (Blocks[check.x, check.y] != null && Blocks[check.x, check.y].ID == 9)
            {
                return false; // bedrock blocks the fall
            }
        }
        bool couldMove = true;
        for(int i = 1; i <= length; i++)
        {
            Vector2Int end = start + Vector2Int.down * i;
            if (Blocks[end.x, end.y] == null || Blocks[end.x, end.y].ID <= 0 || Blocks[end.x, end.y].ID == 36)
            {
                continue;
            }
            else
            {
                couldMove = false;
                break;
            }
        }
        if (!couldMove)
        {
            Debug.LogWarning($"{start.x},{start.y}:Moving Failed!");
            return false;
        }
        bool moved = Blocks[start.x, start.y].MoveTo(start + Vector2Int.down * length, 0.1f, 0.2f);
        if (moved)
        {
            Blocks[start.x, start.y] = null;
            SetBlock(start, 36);//moving block
            SetBlock(start + Vector2Int.down * length, 36);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool inRange(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < Frame.xSize && pos.y < Frame.ySize;
    }

    public int GetBlockID(Vector2Int pos)
    {
        if(pos.y >= Frame.ySize&& pos.x >= 0 && pos.x < Frame.xSize)
        {
            return 0;
        }
        if (pos.x < 0 || pos.y < 0 || pos.x >= Frame.xSize || pos.y >= Frame.ySize)
        {
            return 1;
        }
        if(Blocks[pos.x, pos.y] != null)
        {
            return Blocks[pos.x, pos.y].ID;
        }
        else
        {
            return 0;
        }
    }

    public void SetBlock(Vector2Int pos,int id)
    {
        if (id == 0)
        {
            if (Blocks[pos.x, pos.y] != null)
            {
                Blocks[pos.x, pos.y].Break();
                Blocks[pos.x, pos.y]=null;
            }
            return;
        }
        if (pos.x < 0 || pos.y < 0 || pos.x >= Frame.xSize || pos.y >= Frame.ySize)
        {
            Debug.LogWarning("Pos is out of range!");
            return;
        }
        // Use prefab index1 for high block IDs; sprite comes from GetBlockSprite(id)
        int prefabIdx = (id >= 0 && id < blockPrefabs.Length) ? id : 1;
        Blocks[pos.x, pos.y] = Instantiate(blockPrefabs[prefabIdx]).GetComponent<Block>();
        Blocks[pos.x, pos.y].ID = id;
        Blocks[pos.x, pos.y].transform.SetParent(transform, false);
        Blocks[pos.x, pos.y].Init(new Vector2Int(pos.x, pos.y),this);
        Sprite s = GetBlockSprite(id);
        if (s != null) Blocks[pos.x, pos.y].SetSprite(s);
    }
}