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

    // ── APK Blocks.Tick() translation ──
    private void TickBlock(int x, int y)
    {
        Block b = Blocks[x, y];
        if (b == null) return;
        
        switch (b.ID)
        {
            case 1: // TNT: adjacent redstone(2) OR lava(56) → explode
                if (Check(x+1,y,2)||Check(x-1,y,2)||Check(x,y+1,2)||Check(x,y-1,2)
                 || Check(x+1,y,56)||Check(x-1,y,56)||Check(x,y+1,56)||Check(x,y-1,56))
                {
                    b.ID = 0;
                    var t = new GameObject("Tnt_1");
                    t.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    var tc = t.AddComponent<Tnt_1>();
                    tc.cx = x; tc.cy = y;
                    Reset15(x, y, b);
                }
                break;
            case 3: // Dirt: air above → grass
                if (Check(x, y+1, 0)) { b.ID = 4; Reset_Block(x, y, b); }
                break;
            case 4: // Grass: no air above → dirt
                if (!Check(x, y+1, 0)) { b.ID = 3; Reset_Block(x, y, b); }
                break;
            case 6: // Lamp off: adjacent redstone → on
                if (Check(x+1,y,2)||Check(x-1,y,2)||Check(x,y+1,2)||Check(x,y-1,2))
                    { b.ID = 7; Reset15(x, y, b); }
                break;
            case 7: // Lamp on: no adjacent redstone → off
                if (!Check(x+1,y,2)&&!Check(x-1,y,2)&&!Check(x,y+1,2)&&!Check(x,y-1,2))
                    { b.ID = 6; Reset15(x, y, b); }
                break;
            case 8: // Sand: fall while air below
            {
                int snd = 0;
                int cy = y;
                while (Check(x, cy-1, 0))
                {
                    Fall(x, cy, cy-1, 1);
                    cy--;
                    snd = Random.Range(1,5);
                }
                break;
            }
            case 11: // Oak: spread leaves to adjacent air
                if (Check(x+1,y,0)) MakeBlock(x+1,y,10);
                if (Check(x-1,y,0)) MakeBlock(x-1,y,10);
                if (Check(x,y+1,0)) MakeBlock(x,y+1,10);
                if (Check(x,y-1,0)) MakeBlock(x,y-1,10);
                break;
            case 12: // Memory unlanded → capture board → 13
            {
                b.ID = 13;
                b.savedLayout = new int[10,20];
                for (int xx=0;xx<10;xx++)
                    for (int yy=0;yy<20;yy++)
                        b.savedLayout[xx,yy] = GetBlockID(new Vector2Int(xx,yy));
                b.savedX = x; b.savedY = y;
                Reset_Block(x, y, b);
                break;
            }
            case 15: // Target: 0.5s timer → redstone(2)
                if (b.targetTimer < 0f) b.targetTimer = 0.5f;
                b.targetTimer -= tickTime;
                if (b.targetTimer <= 0f)
                {
                    b.targetTimer = -1f;
                    Blocks[x,y].ID = 2;
                    b.SetSprite(GetBlockSprite(2));
                    Add15(x, y);
                }
                break;
            case 16: // Pumpkin → golem (APK: 4 snow + 4 iron patterns)
                // Snow golem: 2 snow(54) below
                if (Check(x,y-1,54) && Check(x,y-2,54))
                {
                    SetBlock(new Vector2Int(x,y-1),0); SetBlock(new Vector2Int(x,y-2),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var sm = new GameObject("SnowMan");
                    sm.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    sm.AddComponent<SnowMan>();
                }
                // Snow golem: 2 snow(54) above
                else if (Check(x,y+1,54) && Check(x,y+2,54))
                {
                    SetBlock(new Vector2Int(x,y+1),0); SetBlock(new Vector2Int(x,y+2),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var sm = new GameObject("SnowMan");
                    sm.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    sm.AddComponent<SnowMan>();
                }
                // Snow golem: 2 snow(54) left
                else if (Check(x-1,y,54) && Check(x-2,y,54))
                {
                    SetBlock(new Vector2Int(x-1,y),0); SetBlock(new Vector2Int(x-2,y),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var sm = new GameObject("SnowMan");
                    sm.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    sm.AddComponent<SnowMan>();
                }
                // Snow golem: 2 snow(54) right
                else if (Check(x+1,y,54) && Check(x+2,y,54))
                {
                    SetBlock(new Vector2Int(x+1,y),0); SetBlock(new Vector2Int(x+2,y),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var sm = new GameObject("SnowMan");
                    sm.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    sm.AddComponent<SnowMan>();
                }
                // Iron golem: T-shape down (55×5)
                else if (Check(x,y-1,55)&&Check(x,y-2,55)&&Check(x-1,y-1,55)&&Check(x+1,y-1,55))
                {
                    SetBlock(new Vector2Int(x,y-1),0); SetBlock(new Vector2Int(x,y-2),0);
                    SetBlock(new Vector2Int(x-1,y-1),0); SetBlock(new Vector2Int(x+1,y-1),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var im = new GameObject("IronMan");
                    im.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    im.AddComponent<IronMan>();
                }
                // Iron golem: T-shape up
                else if (Check(x,y+1,55)&&Check(x,y+2,55)&&Check(x-1,y+1,55)&&Check(x+1,y+1,55))
                {
                    SetBlock(new Vector2Int(x,y+1),0); SetBlock(new Vector2Int(x,y+2),0);
                    SetBlock(new Vector2Int(x-1,y+1),0); SetBlock(new Vector2Int(x+1,y+1),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var im = new GameObject("IronMan");
                    im.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    im.AddComponent<IronMan>();
                }
                // Iron golem: T-shape right
                else if (Check(x+1,y,55)&&Check(x+2,y,55)&&Check(x+1,y+1,55)&&Check(x+1,y-1,55))
                {
                    SetBlock(new Vector2Int(x+1,y),0); SetBlock(new Vector2Int(x+2,y),0);
                    SetBlock(new Vector2Int(x+1,y+1),0); SetBlock(new Vector2Int(x+1,y-1),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var im = new GameObject("IronMan");
                    im.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    im.AddComponent<IronMan>();
                }
                // Iron golem: T-shape left
                else if (Check(x-1,y,55)&&Check(x-2,y,55)&&Check(x-1,y+1,55)&&Check(x-1,y-1,55))
                {
                    SetBlock(new Vector2Int(x-1,y),0); SetBlock(new Vector2Int(x-2,y),0);
                    SetBlock(new Vector2Int(x-1,y+1),0); SetBlock(new Vector2Int(x-1,y-1),0);
                    SetBlock(new Vector2Int(x,y),0);
                    var im = new GameObject("IronMan");
                    im.transform.position = Frame.instance.GetPosV2(new Vector2Int(x,y));
                    im.AddComponent<IronMan>();
                }
                break;
            case 17: // Stone brick: adjacent water(58) → mossy(49)
                if (Check(x+1,y,58)||Check(x-1,y,58)||Check(x,y+1,58)||Check(x,y-1,58))
                    { b.ID = 49; Reset_Block(x, y, b); }
                break;
            case 49: // Mossy: adjacent lava(56) → stone brick(17)
                if (Check(x+1,y,56)||Check(x-1,y,56)||Check(x,y+1,56)||Check(x,y-1,56))
                    { b.ID = 17; Reset_Block(x, y, b); }
                break;
        }
    }

    // ── APK: Check(x,y,id) ──
    private bool Check(int x, int y, int id)
    {
        if (x<0||x>=10||y<0||y>=20) return false;
        return GetBlockID(new Vector2Int(x,y)) == id;
    }

    // ── APK: _Reset() = update sprite + mark 4 neighbors ──
    private void Reset_Block(int x, int y, Block b)
    {
        // block ID tracked via Blocks array directly
        b.SetSprite(GetBlockSprite(b.ID));
        Add(x, y);
    }

    // ── APK: _Reset15() = mark neighbors (exclude id=15,2) ──
    private void Reset15(int x, int y, Block b)
    {
        // block ID tracked via Blocks array directly
        b.SetSprite(GetBlockSprite(b.ID));
        Add15(x, y);
        if (b.ID == 0) { Blocks[x, y] = null; Object.Destroy(b.gameObject); }
    }

    // ── APK: Add() — mark adjacent non-zero blocks for re-tick ──
    private void Add(int x, int y)
    {
        if (x+1>=0&&x+1<10&&y>=0&&y<20&&GetBlockID(new Vector2Int(x+1,y))!=0) MarkTick(x+1,y);
        if (x-1>=0&&x-1<10&&y>=0&&y<20&&GetBlockID(new Vector2Int(x-1,y))!=0) MarkTick(x-1,y);
        if (x>=0&&x<10&&y+1>=0&&y+1<20&&GetBlockID(new Vector2Int(x,y+1))!=0) MarkTick(x,y+1);
        if (x>=0&&x<10&&y-1>=0&&y-1<20&&GetBlockID(new Vector2Int(x,y-1))!=0) MarkTick(x,y-1);
    }

    // ── APK: Add15() — exclude id=15,2 ──
    private void Add15(int x, int y)
    {
        int id;
        if (x+1>=0&&x+1<10&&y>=0&&y<20) {
            id=GetBlockID(new Vector2Int(x+1,y));
            if (id!=0&&id!=15&&id!=2) MarkTick(x+1,y);
        }
        if (x-1>=0&&x-1<10&&y>=0&&y<20) {
            id=GetBlockID(new Vector2Int(x-1,y));
            if (id!=0&&id!=15&&id!=2) MarkTick(x-1,y);
        }
        if (x>=0&&x<10&&y+1>=0&&y+1<20) {
            id=GetBlockID(new Vector2Int(x,y+1));
            if (id!=0&&id!=15&&id!=2) MarkTick(x,y+1);
        }
        if (x>=0&&x<10&&y-1>=0&&y-1<20) {
            id=GetBlockID(new Vector2Int(x,y-1));
            if (id!=0&&id!=15&&id!=2) MarkTick(x,y-1);
        }
    }

    // ── APK: Fall() — move block down (for sand) ──
    private void Fall(int x, int y, int newY, int total)
    {
        Block b = Blocks[x, y];
        if (b == null) return;
        for (int i=1; y-i>=newY; i++)
            if (GetBlockID(new Vector2Int(x,y-i)) == 9 || b.ID == 9) return;
        Blocks[x, y] = null;
        b.Pos = new Vector2Int(x, newY);
        Blocks[x, newY] = b;
        b.transform.localPosition = Frame.instance.GetPosV2(new Vector2Int(x, newY));
        if (newY <= 19) MarkTick(x, newY);
    }

    // ── APK: MakeBlock() — create block (for oak→leaves) ──
    private void MakeBlock(int x, int y, int id)
    {
        if (x<0||x>=10||y<0||y>=20) return;
        if (GetBlockID(new Vector2Int(x,y)) != 0) return;
        SetBlock(new Vector2Int(x, y), id);
    }

    // ── Tick propagation: mark block for next tick cycle ──
    public void MarkTick(int x, int y)
    {
        // DoTick iterates all blocks each tick; no-op
    }
    // ticksToProcess not needed: DoTick does full iteration
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