using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int ID { get; set; } = 1;

    public Vector2Int Pos { get; private set; }= new Vector2Int();

    // Target block timer (id=15 → id=2 after0.5s)
    public float targetTimer = -1f;
    // TNT explosion timer (unused simplified, kept for batch3)
    public int tntTimer = -1;
    // Memory block saved layout (id=12→13)
    public int[,] savedLayout = null;
    public int savedX, savedY;
    public bool isMemoryRestoring = false;
    public int memoryRestorePhase = 0;
    public float memoryTimer = 0f;

    public virtual void Break()
    {
        Destroy(gameObject);
    }
    public virtual bool MoveTo(Vector2Int pos, float waitingTime, float movingTime)
    {
        if (nowTime > 0)
        {
            Debug.LogError("Already Moving!");
            return false;
        }
        moveStartPos = Pos;
        moveEndPos = pos;
        Pos = pos;
        this.waitingTime = waitingTime;
        this.movingTime = movingTime;
        nowTime = 0;
        ground.UpdateCall += Moving;
        return true;
    }

    public virtual void Init(Vector2Int pos,Ground ground)
    {
        Pos = pos;
        this.ground = ground;
        transform.localPosition = Frame.instance.GetPosV2(pos);
    }

    protected Ground ground;


    protected Vector2Int moveStartPos, moveEndPos;
    protected float waitingTime, movingTime,nowTime;
    public virtual void SetSprite(Sprite sprite)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sprite != null)
            sr.sprite = sprite;
    }

    protected virtual void Moving()
    {
        nowTime += Time.deltaTime;
        if (nowTime >= waitingTime)
        {
            if (nowTime >= waitingTime+movingTime)
            {
                transform.localPosition = Frame.instance.GetPosV2(moveEndPos);
                ground.SetBlock(moveStartPos, 0);
                ground.SetBlock(moveEndPos, 0);
                ground.Blocks[moveEndPos.x, moveEndPos.y] = this;
                nowTime = 0;

                ground.UpdateCall -= Moving;
            }
            else
            {
                Vector2 start = Frame.instance.GetPosV2(moveStartPos);
                Vector2 end = Frame.instance.GetPosV2(moveEndPos);
                float t = (nowTime-waitingTime)/movingTime/2;
                t = Mathf.SmoothStep(0, 1, t)*2;
                transform.localPosition = Vector3.Lerp(start, end, t);
            }
        }
    }
}