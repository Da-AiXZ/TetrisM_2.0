using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static FallingGroupShow;
using static InputManager;

public class FallingBlock : MonoBehaviour
{
    public enum GroupTypes
    {
        I=0,
        O=1,
        L=2,
        J=3,
        T=4,
        Z=5,
        ZM=6,
    }
    public enum Rotations
    {
        Zero = 0,
        R = 1,
        Two = 2,
        L = 3,
    }
    public static Dictionary<GroupTypes, float[]> cneterDic = new()
    {
    { GroupTypes.I,new float[]{ 2,2} } ,
    { GroupTypes.J,new float[]{ 1.5f,1.5f } } ,
    { GroupTypes.L,new float[]{ 1.5f,1.5f } } ,
    { GroupTypes.O,new float[]{ 2, 2 } } ,
    { GroupTypes.ZM,new float[]{ 1.5f, 1.5f } } ,
    { GroupTypes.T,new float[]{ 1.5f, 1.5f } } ,
    { GroupTypes.Z,new float[]{ 1.5f, 1.5f } } ,
    };
    public static Dictionary<GroupTypes, int[,,]> groupDic = new() {
        { GroupTypes.I,new int[,,]{
            {
        {0, 2},
        {1, 2},
        {2, 2},
        {3, 2},
    },
    {
        {2, 3},
        {2, 2},
        {2, 1},
        {2, 0},
    },
    {
        {3, 1},
        {2, 1},
        {1, 1},
        {0, 1},
    },
    {
        {1, 0},
        {1, 1},
        {1, 2},
        {1, 3},
    },} },
        { GroupTypes.J,new int[,,]{
            {
        {0, 2},
        {0, 1},
        {1, 1},
        {2, 1},
    },
    {
        {2, 2},
        {1, 2},
        {1, 1},
        {1, 0},
    },
    {
        {2, 0},
        {2, 1},
        {1, 1},
        {0, 1},
    },
    {
        {0, 0},
        {1, 0},
        {1, 1},
        {1, 2},
    },} },
        { GroupTypes.L,new int[,,]{
            {
        {0, 1},
        {1, 1},
        {2, 1},
        {2, 2},
    },
    {
        {1, 2},
        {1, 1},
        {1, 0},
        {2, 0},
    },
    {
        {2, 1},
        {1, 1},
        {0, 1},
        {0, 0},
    },
    {
        {1, 0},
        {1, 1},
        {1, 2},
        {0, 2},
    },} },
        { GroupTypes.O,new int[,,]{
            {
        {1, 1},
        {1, 2},
        {2, 2},
        {2, 1},
    },
            {
        {1, 2},
        {2, 2},
        {2, 1},
        {1, 1},
    },
            {
        {2, 2},
        {2, 1},
        {1, 1},
        {1, 2},
    },
            {
        {2, 1},
        {1, 1},
        {1, 2},
        {2, 2},
    },} },
        { GroupTypes.ZM,new int[,,]{
            {
        {0, 1},
        {1, 1},
        {1, 2},
        {2, 2},
    },
    {
        {1, 2},
        {1, 1},
        {2, 1},
        {2, 0},
    },
    {
        {2, 1},
        {1, 1},
        {1, 0},
        {0, 0},
    },
    {
        {1, 0},
        {1, 1},
        {0, 1},
        {0, 2},
    }, } },
        { GroupTypes.T,new int[,,]{
            {
        {0, 1},
        {1, 1},
        {2, 1},
        {1, 2},
    },
    {
        {1, 2},
        {1, 1},
        {1, 0},
        {2, 1},
    },
    {
        {2, 1},
        {1, 1},
        {0, 1},
        {1, 0},
    },
    {
        {1, 0},
        {1, 1},
        {1, 2},
        {0, 1},
    }, } },
        { GroupTypes.Z,new int[,,]{
            {
        {0, 2},
        {1, 2},
        {1, 1},
        {2, 1},
    },
    {
        {2, 2},
        {2, 1},
        {1, 1},
        {1, 0},
    },
    {
        {2, 0},
        {1, 0},
        {1, 1},
        {0, 1},
    },
    {
        {0, 0},
        {0, 1},
        {1, 1},
        {1, 2},
    }, } },
    };
    public static Dictionary<(Rotations,Rotations), int[,]> rotareCheck = new()
    {
        { (Rotations.Zero,Rotations.R),new int[,]
        {
            {0,0},
            {-1,0},
            {-1,1},
            {0,-2},
            {-1,-2},
        }
        },
        { (Rotations.R,Rotations.Zero),new int[,]
        {
            {0,0},
            {1,0},
            {1,-1},
            {0,2},
            {1,2},
        }
        },
        { (Rotations.R,Rotations.Two),new int[,]
        {
            {0,0},
            {1,0},
            {1,-1},
            {0,2},
            {1,2},
        }
        },
        { (Rotations.Two,Rotations.R),new int[,]
        {
            {0,0},
            {-1,0},
            {-1,1},
            {0,-2},
            {-1,-2},
        }
        },
        { (Rotations.Two,Rotations.L),new int[,]
        {
            {0,0},
            {1,0},
            {1,1},
            {0,-2},
            {1,-2},
        }
        },
        { (Rotations.L,Rotations.Two),new int[,]
        {
            {0,0},
            {-1,0},
            {-1,-1},
            {0,2},
            {-1,2},
        }
        },
        { (Rotations.L,Rotations.Zero),new int[,]
        {
            {0,0},
            {-1,0},
            {-1,-1},
            {0,2},
            {-1,2},
        }
        },
        { (Rotations.Zero,Rotations.L),new int[,]
        {
            {0,0},
            {1,0},
            {1,1},
            {0,-2},
            {1,-2},
        }
        },
    };
    public static Dictionary<(Rotations, Rotations), int[,]> rotareCheckI = new() 
    {
        { (Rotations.Zero,Rotations.R),new int[,]
        {
            {0,0},
            {-2,0},
            {1,0},
            {-2,-1},
            {1,2},
        }
        },
        { (Rotations.R,Rotations.Zero),new int[,]
        {
            {0,0},
            {2,0},
            {-1,0},
            {2,1},
            {-1,-2},
        }
        },
        { (Rotations.R,Rotations.Two),new int[,]
        {
            {0,0},
            {-1,0},
            {2,0},
            {-1,2},
            {2,-1},
        }
        },
        { (Rotations.Two,Rotations.R),new int[,]
        {
            {0,0},
            {1,0},
            {-2,0},
            {1,-2},
            {-2,1},
        }
        },
        { (Rotations.Two,Rotations.L),new int[,]
        {
            {0,0},
            {2,0},
            {-1,0},
            {2,1},
            {-1,-2},
        }
        },
        { (Rotations.L,Rotations.Two),new int[,]
        {
            {0,0},
            {-2,0},
            {1,0},
            {-2,-1},
            {1,2},
        }
        },
        { (Rotations.L,Rotations.Zero),new int[,]
        {
            {0,0},
            {1,0},
            {-2,0},
            {1,-2},
            {-2,1},
        }
        },
        { (Rotations.Zero,Rotations.L),new int[,]
        {
            {0,0},
            {-1,0},
            {2,0},
            {-1,2},
            {2,-1},
        }
        },
    };
    public struct FallingBlocks
    {
        public GroupTypes type;
        public int[] blockIDs;
    }

    public FallingGroupShow fallingGroupShow;
    public GroupTypes Type { get; private set; }
    public Rotations Rotation { get; private set; }
    public int[] BlockID { get; private set; }=new int[4] { 1,1,1,1};

    public Vector2Int Pos { get; private set; } = new(3, 21);

    public delegate void Call();
    public event Call OnReload;
    public event Call OnHold;

    public List<GroupTypes> waiting = new();
    public List<GroupTypes> bag = new();
    public GroupTypes Holding = GroupTypes.I;
    public bool isHolding = false;
    public bool couldHold = true;
    private bool hadMemoryBlockThisRound = false;
    // Tick-based movement (APK system: FixedUpdate = 50Hz = 20ms per tick)
    private int fallTick = 0;
    private int moveTick = 0;
    private const int SPEED = 50;       // 50 ticks = 1.0s per fall
    private const int SOFT_DROP = 5;    // 5 ticks = 100ms soft drop
    private const int MOVE_REPEAT = 10; // 10 ticks = 200ms left/right repeat

    public void Reload()
    {
        couldHold = true;
        Pos =new Vector2Int(3, 18);
        fallTick = 0;
        moveTick = 0;
        Type = GetOneFromWaiting();
        Rotation = Rotations.Zero;
        
        // APK: FallDown.Start() randomizes Minecraft block IDs for each sub-block
        // Range A (50%): id=1-22, excl0,4,7,12,13,14,57; if id>=18→+36
        // Range B (50%): id=18-53, excl4,7,13,14,49
        int GetRandomBlockID()
        {
            int id;
            if (Random.Range(0, 2) == 0)
            {
                do { id = Random.Range(1, 23); }
                while (id == 0 || id == 4 || id == 7 || id == 12 || id == 13 || id == 14 || id == 57);
                if (id >= 18) id += 36;
            }
            else
            {
                do { id = Random.Range(18, 54); }
                while (id == 4 || id == 7 || id == 13 || id == 14 || id == 49);
            }
            return id;
        }
        
        int baseID = GetRandomBlockID();
        // APK: TNT chain - if previous block was TNT(1), high chance of trigger blocks
        // This is handled in APK's FallDown.Start(); simplified here
        BlockID = new int[4] { baseID, baseID, baseID, baseID };
        
        // APK: ~1% chance of memory block per sub-block
        for (int i = 0; i < 4 && !hadMemoryBlockThisRound; i++)
        {
            if (Random.Range(1, 101) == 3)
            {
                BlockID[i] = 12;
                hadMemoryBlockThisRound = true;
                break;
            }
        }

        transform.localPosition = Frame.instance.GetPosV2(Pos);
        fallingGroupShow.SetType(Type);
        fallingGroupShow.SetBlockSprites(BlockID);
        fallingGroupShow.SetRotation(Rotation);

        OnReload?.Invoke();
    }

    private GroupTypes GetOneFromWaiting()
    {
        while (waiting.Count < 5)
        {
            waiting.Add(GetOneFromBag());
        }
        GroupTypes res = waiting[0];
        waiting.RemoveAt(0);
        return res;
    }
    private GroupTypes GetOneFromBag()
    {
        if (bag.Count == 0)
        {
            bag = new() { GroupTypes.I, GroupTypes.O, GroupTypes.L, GroupTypes.J, GroupTypes.T, GroupTypes.Z, GroupTypes.ZM };
        }
        int i=Random.Range(0, bag.Count);
        GroupTypes res= bag[i];
        bag.RemoveAt(i);
        return res;
    }

    // SettingChange removed (APK has no settings)

    private void Start()
    {
        Reload();
        // Settings removed (APK has no settings menu)
    }

    private void OnDestroy()
    {
        Settings.settingChange -= SettingChange;
    }

    private void Update()
    {
        // Rotation and hard-drop / hold (frame-independent, handled in Update)
        if (!GetKey(Key.x) && GetKeyDown(Key.b) && !GetKey(Key.y))
        {
            bool couldRotate;
            Vector2Int offset;
            Rotations rotation;
            (couldRotate,offset,rotation)=RotateRight();
            if (couldRotate)
            {
                Pos += offset;
                Rotation = rotation;
            }
        }
        if (GetKeyDown(Key.x) && !GetKey(Key.b) && !GetKey(Key.y))
        {
            bool couldRotate;
            Vector2Int offset;
            Rotations rotation;
            (couldRotate, offset, rotation) = RotateLeft();
            if (couldRotate)
            {
                Pos += offset;
                Rotation = rotation;
            }
        }
        if (!GetKey(Key.x) && !GetKey(Key.b) && GetKeyDown(Key.y))
        {
            bool couldRotate;
            Vector2Int offset;
            Rotations rotation;
            (couldRotate, offset, rotation) = Rotate180();
            if (couldRotate)
            {
                Pos += offset;
                Rotation = rotation;
            }
        }

        if (GetKeyDown(Key.a))
        {
            Down();
        }

        if (GetKeyDown(Key.up))
        {
            Hold();
        }

        transform.localPosition = Frame.instance.GetPosV2(Pos);
    }

    private void FixedUpdate()
    {
        // APK tick-based movement: FixedUpdate runs at 50Hz (20ms per tick)
        fallTick++;
        if (fallTick >= SPEED)
        {
            fallTick = 0;
            Move(Vector2Int.down, true);
        }
        // Soft drop: hold down to fall faster
        if (GetKey(Key.down) && fallTick >= SOFT_DROP)
        {
            fallTick = 0;
            Move(Vector2Int.down, true);
        }

        // Left/right: immediate first move, then repeat every MOVE_REPEAT ticks
        int dir = 0;
        if (GetKey(Key.left)) dir = -1;
        if (GetKey(Key.right)) dir = (dir != 0) ? 0 : 1;

        if (dir == 0)
        {
            moveTick = 100; // reset: next press moves immediately
        }
        else
        {
            moveTick++;
            if (moveTick >= MOVE_REPEAT)
            {
                moveTick = 0;
                if (dir == -1) Move(Vector2Int.left, false);
                else Move(Vector2Int.right, false);
            }
        }
    }

    private (bool,Vector2Int,Rotations) RotateRight()
    {
        Rotations r = Rotate(Rotation,1);

        bool couldRotate;
        Vector2Int offset;
        (couldRotate, offset) = CheckRotate(Rotation,r,Pos,Type);
        if (couldRotate)
        {
            fallingGroupShow.RotateTo(r);
            return (true, offset, r);
        }

        return (false, new Vector2Int(),r);
    }

    private (bool, Vector2Int, Rotations) RotateLeft()
    {
        Rotations r = Rotate(Rotation, -1);

        bool couldRotate;
        Vector2Int offset;
        (couldRotate, offset) = CheckRotate(Rotation, r, Pos, Type);
        if (couldRotate)
        {
            fallingGroupShow.RotateTo(r);
            return (true, offset, r);
        }

        return (false, new Vector2Int(), r);
    }

    private (bool, Vector2Int, Rotations) Rotate180()
    {
        Rotations r = Rotate(Rotation, 1);

        bool couldRotate;
        Vector2Int offset;
        (couldRotate, offset) = CheckRotate(Rotation, r, Pos, Type);
        if (couldRotate)
        {
            Rotations r2 = Rotate(r, 1);
            bool couldRotate2;
            Vector2Int offset2;
            (couldRotate2, offset2) = CheckRotate(r, r2, Pos+offset, Type);
            if (couldRotate2)
            {
                fallingGroupShow.RotateTo(r2);
                return (true, offset+offset2, r2);
            }
            else
            {
                fallingGroupShow.RotateTo(r);
                return (true, offset, r);
            }
        }
        else
        {
            return (false, new Vector2Int(), r);
        }
    }
    private static (bool, Vector2Int) CheckRotate(Rotations origin, Rotations target,Vector2Int pos,GroupTypes type)
    {
        int[,] checks;
        if (type == GroupTypes.I)
        {
            checks = rotareCheckI[(origin, target)];
        }
        else
        {
            checks = rotareCheck[(origin, target)];
        }
        for (int i = 0; i < checks.GetLength(0); i++)
        {
            Vector2Int offset = new(checks[i, 0], checks[i, 1]);
            if (CheckPosFit(pos + offset, type, target))
            {
                return (true, offset);
            }
        }
        return (false, new Vector2Int());
    }

    private Rotations Rotate(Rotations rotation,int direction)
    {
        direction = (direction % 4 + 4) % 4;
        return (Rotations)(((int)rotation + direction) % 4);
    }

    public delegate void GameOverCall();
    public static event GameOverCall OnGameOver;

    private bool Move(Vector2Int move,bool set)
    {
        bool fit;

        // Crush glass below when moving down
        if (move == Vector2Int.down)
            CrushGlass(Pos, Type, Rotation);

        fit = CheckPosFit(Pos + move, Type, Rotation);

        if (fit)
        {
            Pos += move;
            return true;
        }
        else if(set)
        {
            SetBlock(Pos, Type, Rotation, BlockID);
            // APK GameOver: if any block landed above y=19 (top line)
            int[,,] dat = groupDic[Type];
            for (int i = 0; i < 4; i++)
            {
                int by = dat[(int)Rotation, i, 1] + Pos.y;
                if (by >= Frame.ySize)
                {
                    OnGameOver?.Invoke();
                    Debug.Log("Game Over - block above top line");
                    return false;
                }
            }
            Reload();
            return false;
        }
        else
        {
            return false;
        }
    }
    private void Down()
    {
        while (Move(Vector2Int.down, true)) ;
    }
    private void Hold()
    {
        if (!couldHold)
        {
            return;
        }
        couldHold = false;
        if (isHolding)
        {
            GroupTypes tmp = Holding;
            Holding = Type;
            Type = tmp;
        }
        else
        {
            Holding = Type;
            Type = GetOneFromWaiting();
            OnReload?.Invoke();
            isHolding = true;
        }
        fallingGroupShow.SetType(Type);
        OnHold?.Invoke();
    }

    private static void SetBlock(Vector2Int pos, GroupTypes type, Rotations rotation, int[] blockID)
    {
        int[,,] dat = groupDic[type];
        if (blockID.Length != 4)
        {
            Debug.LogError("Wrong blockID length!");
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Vector2Int blockPos = new(dat[(int)rotation, i, 0] + pos.x, dat[(int)rotation, i, 1] + pos.y);
            int id = blockID[i];
            // Water(58) and lava(56): skip solid block; fluid system handles them (batch2)
            if (id == 58 || id == 56)
            {
                Ground.Instance.SetBlock(blockPos, 0);
                continue;
            }
            Ground.Instance.SetBlock(blockPos, id);
        }
    }

    private static bool CheckPosFit(Vector2Int pos,GroupTypes type,Rotations rotation)
    {
        int[,,] dat = groupDic[type];
        for (int i = 0; i < 4; i++)
        {
            Vector2Int blockPos = new(dat[(int)rotation, i, 0]+pos.x, dat[(int)rotation, i, 1]+pos.y);
            int idAt = Ground.Instance.GetBlockID(blockPos);
            if (idAt > 0)
            {
                // Pass through water(58) and lava(56)
                if (idAt == 58 || idAt == 56) continue;
                // Crush glass(5)
                if (idAt == 5)
                {
                    // Will be crushed; handled in Move() when glass is destroyed
                    continue;
                }
                return false;
            }
        }
        return true;
    }

    // Check and crush glass below blocks (called before Move succeeds)
    private static void CrushGlass(Vector2Int pos, GroupTypes type, Rotations rotation)
    {
        int[,,] dat = groupDic[type];
        for (int i = 0; i < 4; i++)
        {
            Vector2Int blockPos = new(dat[(int)rotation, i, 0]+pos.x, dat[(int)rotation, i, 1]+pos.y);
            if (blockPos.y - 1 >= 0 && blockPos.y - 1 < Frame.ySize)
            {
                int below = Ground.Instance.GetBlockID(new Vector2Int(blockPos.x, blockPos.y - 1));
                if (below == 5)
                {
                    Ground.Instance.SetBlock(new Vector2Int(blockPos.x, blockPos.y - 1), 0);
                    // TODO: play glass sound (batch3 audio system)
                }
            }
        }
    }
}