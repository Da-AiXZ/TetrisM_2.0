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

    // === Minecraft Physics (ported from APK Blocks.Tick) ===
    private void DoTick()
    {
        // Process bottom-up to handle gravity correctly
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

        // Water: id3(source) and id4(flowing)
        if (id == 3 || id == 4)
        {
            WaterTick(x, y, b);
            return;
        }
        // Lava: id6(source) and id7(flowing)
        if (id == 6 || id == 7)
        {
            LavaTick(x, y, b);
            return;
        }
        // Sand/Gravel: id13, id14
        if (id == 13 || id == 14)
        {
            GravityTick(x, y, b);
            return;
        }
        // TNT: id8(ready), id10(lit), id11(exploding)
        if (id == 8 || id == 10 || id == 11)
        {
            TNTTick(x, y, b);
            return;
        }
    }

    private void WaterTick(int x, int y, Block b)
    {
        // APK: self id3→4, spread to adjacent empty/waterkid
        if (b.ID == 3) b.ID = 4;
        if (y + 1 < Frame.ySize && (GetBlockID(new Vector2Int(x, y + 1)) == 0 || GetBlockID(new Vector2Int(x, y + 1)) == 22))
            SetBlock(new Vector2Int(x, y + 1), 3);
        if (y - 1 >= 0 && (GetBlockID(new Vector2Int(x, y - 1)) == 0 || GetBlockID(new Vector2Int(x, y - 1)) == 22))
            SetBlock(new Vector2Int(x, y - 1), 3);
        if (x + 1 < Frame.xSize && (GetBlockID(new Vector2Int(x + 1, y)) == 0 || GetBlockID(new Vector2Int(x + 1, y)) == 22))
            SetBlock(new Vector2Int(x + 1, y), 3);
        if (x - 1 >= 0 && (GetBlockID(new Vector2Int(x - 1, y)) == 0 || GetBlockID(new Vector2Int(x - 1, y)) == 22))
            SetBlock(new Vector2Int(x - 1, y), 3);
    }

    private void LavaTick(int x, int y, Block b)
    {
        // APK: self id6→7, spread to adjacent empty/lavakid
        if (b.ID == 6) b.ID = 7;
        if (y + 1 < Frame.ySize && (GetBlockID(new Vector2Int(x, y + 1)) == 0 || GetBlockID(new Vector2Int(x, y + 1)) == 23))
            SetBlock(new Vector2Int(x, y + 1), 6);
        if (y - 1 >= 0 && (GetBlockID(new Vector2Int(x, y - 1)) == 0 || GetBlockID(new Vector2Int(x, y - 1)) == 23))
            SetBlock(new Vector2Int(x, y - 1), 6);
        if (x + 1 < Frame.xSize && (GetBlockID(new Vector2Int(x + 1, y)) == 0 || GetBlockID(new Vector2Int(x + 1, y)) == 23))
            SetBlock(new Vector2Int(x + 1, y), 6);
        if (x - 1 >= 0 && (GetBlockID(new Vector2Int(x - 1, y)) == 0 || GetBlockID(new Vector2Int(x - 1, y)) == 23))
            SetBlock(new Vector2Int(x - 1, y), 6);
    }

    private void GravityTick(int x, int y, Block b)
    {
        // APK: if below is empty(0), waterkid(22), or lavakid(23), fall down
        if (y == 0) return;
        int below = GetBlockID(new Vector2Int(x, y - 1));
        if (below == 0 || below == 22 || below == 23 || below == 36)
        {
            b.MoveTo(new Vector2Int(x, y - 1), 0.05f, 0.1f);
        }
    }

    private void TNTTick(int x, int y, Block b)
    {
        // APK: id8→10 on first tick with tntTimer=3; count down; explode at0
        if (b.ID == 8)
        {
            b.ID = 10;
            b.tntTimer = 3;
            return;
        }
        if (b.ID == 10)
        {
            b.tntTimer--;
            if (b.tntTimer <= 0)
            {
                // Explode: clear5x4 area centered on TNT
                b.ID = 11;
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -1; dy <= 2; dy++)
                    {
                        int tx = x + dx;
                        int ty = y + dy;
                        if (tx >= 0 && tx < Frame.xSize && ty >= 0 && ty < Frame.ySize)
                        {
                            SetBlock(new Vector2Int(tx, ty), 0);
                        }
                    }
                }
            }
            return;
        }
        // id11: exploded, remove self
        if (b.ID == 11)
        {
            SetBlock(new Vector2Int(x, y), 0);
        }
    }

    private bool CheckFull()
    {
        List<int> full=new();
        for(int y=0; y<Frame.ySize; y++)
        {
            bool isFull = true;
            for (int x = 0; x < Frame.xSize; x++)
            {
                if (Blocks[x,y]==null|| Blocks[x, y].ID == 36 || Blocks[x, y].ID <= 0)
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull)
            {
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