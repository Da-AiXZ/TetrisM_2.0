using System;
using UnityEngine;

// APK Water.cs:10×320 sub-grid particle simulation
// water[x,y]:0=empty,1=water particle,2=flowing
public class Water : MonoBehaviour
{
    public static Water Instance { get; private set; }
    public static int[,] water = new int[10, 320];
    public static int[,] showingWater = new int[10, 320];
    private int tick;

    private void Awake() { Instance = this; }

    private void FixedUpdate()
    {
        tick++;
        if (tick >= 1) { tick =0; Tick(); }
    }

    public static void Reset_()
    {
        for (int x=0; x<10; x++)
            for (int y=0; y<320; y++)
            { water[x,y]=0; showingWater[x,y]=0; }
    }

    public static void DeleteWater(int x, int y)
    {
        for (int i=0; i<16; i++) water[x, i+y*16]=0;
        Add(x,y);
    }

    public static void AddWater(int x, int y)
    {
        for (int i=0; i<16; i++) water[x, i+y*16]=1;
        Add(x,y);
    }

    public static void Add(int x, int y)
    {
        if (x+1>=0&&x+1<10&&y>=0&&y<20&&Ground.Instance.GetBlockID(new Vector2Int(x+1,y))!=0)
            Ground.Instance.MarkTick(x+1,y);
        if (x-1>=0&&x-1<10&&y>=0&&y<20&&Ground.Instance.GetBlockID(new Vector2Int(x-1,y))!=0)
            Ground.Instance.MarkTick(x-1,y);
        if (x>=0&&x<10&&y+1>=0&&y+1<20&&Ground.Instance.GetBlockID(new Vector2Int(x,y+1))!=0)
            Ground.Instance.MarkTick(x,y+1);
        if (x>=0&&x<10&&y-1>=0&&y-1<20&&Ground.Instance.GetBlockID(new Vector2Int(x,y-1))!=0)
            Ground.Instance.MarkTick(x,y-1);
    }

    // APK Tick() - particle simulation
    public void Tick()
    {
        int i =0, j=0, k=0, sec=DateTime.Now.Second;
        // Phase1: push particles out of solid blocks
        while (i<10)
        {
            if ((water[i,j]==1||water[i,j]==2) &&
                Ground.Instance.GetBlockID(new Vector2Int(i,j/16))!=0 &&
                Ground.Instance.GetBlockID(new Vector2Int(i,j/16))!=56 &&
                Ground.Instance.GetBlockID(new Vector2Int(i,j/16))!=58)
            {
                k=1; water[i,j]=0;
                if (sec%2==0)
                {
                    while (true)
                    {
                        if (water[i,j+k]!=1&&water[i,j+k]!=2&&
                            (Ground.Instance.GetBlockID(new Vector2Int(i,(j+k)/16))==0||
                             Ground.Instance.GetBlockID(new Vector2Int(i,(j+k)/16))==56||
                             Ground.Instance.GetBlockID(new Vector2Int(i,(j+k)/16))==58))
                            { water[i,j+k]=1; break; }
                        if (i>0&&water[i-1,j+k]!=1&&water[i-1,j+k]!=2&&
                            (Ground.Instance.GetBlockID(new Vector2Int(i-1,(j+k)/16))==0||
                             Ground.Instance.GetBlockID(new Vector2Int(i-1,(j+k)/16))==56||
                             Ground.Instance.GetBlockID(new Vector2Int(i-1,(j+k)/16))==58))
                            { water[i-1,j+k]=1; break; }
                        if (i<9&&water[i+1,j+k]!=1&&water[i+1,j+k]!=2&&
                            (Ground.Instance.GetBlockID(new Vector2Int(i+1,(j+k)/16))==0||
                             Ground.Instance.GetBlockID(new Vector2Int(i+1,(j+k)/16))==56||
                             Ground.Instance.GetBlockID(new Vector2Int(i+1,(j+k)/16))==58))
                            { water[i+1,j+k]=1; break; }
                        k++;
                        if (j+k>319) { /* MySystem.gameOver=true would go here */ break; }
                    }
                }
                else
                {
                    while (true)
                    {
                        if (water[i,j+k]!=1&&water[i,j+k]!=2&&
                            (Ground.Instance.GetBlockID(new Vector2Int(i,(j+k)/16))==0||
                             Ground.Instance.GetBlockID(new Vector2Int(i,(j+k)/16))==56||
                             Ground.Instance.GetBlockID(new Vector2Int(i,(j+k)/16))==58))
                            { water[i,j+k]=1; break; }
                        if (i<9&&water[i+1,j+k]!=1&&water[i+1,j+k]!=2&&
                            (Ground.Instance.GetBlockID(new Vector2Int(i+1,(j+k)/16))==0||
                             Ground.Instance.GetBlockID(new Vector2Int(i+1,(j+k)/16))==56||
                             Ground.Instance.GetBlockID(new Vector2Int(i+1,(j+k)/16))==58))
                            { water[i+1,j+k]=1; break; }
                        if (i>0&&water[i-1,j+k]!=1&&water[i-1,j+k]!=2&&
                            (Ground.Instance.GetBlockID(new Vector2Int(i-1,(j+k)/16))==0||
                             Ground.Instance.GetBlockID(new Vector2Int(i-1,(j+k)/16))==56||
                             Ground.Instance.GetBlockID(new Vector2Int(i-1,(j+k)/16))==58))
                            { water[i-1,j+k]=1; break; }
                        k++;
                        if (j+k>319) { break; }
                    }
                }
            }
            sec++; j++;
            if (j>=320) { j=0; i++; }
        }
        // Phase2: compact - fill gaps
        bool flag=false;
        i=0; j=0;
        while (i<10)
        {
            if (j<319)
            {
                if ((water[i,j+1]==1||water[i,j+1]==2)&&water[i,j]==0&&
                    (Ground.Instance.GetBlockID(new Vector2Int(i,j/16))==0||
                     Ground.Instance.GetBlockID(new Vector2Int(i,j/16))==56||
                     Ground.Instance.GetBlockID(new Vector2Int(i,j/16))==58)&&!flag)
                    { water[i,j]=1; flag=true; }
                else if (water[i,j+1]==0&&flag)
                    { water[i,j]=0; flag=false; }
            }
            j++;
            if (j>=320) { if (flag) water[i,319]=0; j=0; i++; }
        }
        // Phase3: spread sideways
        i=0; j=0;
        int n5=-1;
        sec=DateTime.Now.Second;
        while (i<10)
        {
            if (j>0)
            {
                if (n5==-1&&water[i,j-1]==0&&(water[i,j]==1||water[i,j]==2)&&
                    (Ground.Instance.GetBlockID(new Vector2Int(i,(j-1)/16))==0||
                     Ground.Instance.GetBlockID(new Vector2Int(i,(j-1)/16))==56||
                     Ground.Instance.GetBlockID(new Vector2Int(i,(j-1)/16))==58))
                    n5=j;
                if (n5!=-1&&water[i,j]==0) n5=-1;
                if (sec%2==1)
                {
                    if (i>0&&n5==-1&&(water[i,j]==1||water[i,j]==2)&&water[i-1,j]!=1&&water[i-1,j]!=2&&
                        (Ground.Instance.GetBlockID(new Vector2Int(i-1,j/16))==0||
                         Ground.Instance.GetBlockID(new Vector2Int(i-1,j/16))==56||
                         Ground.Instance.GetBlockID(new Vector2Int(i-1,j/16))==58))
                        { water[i,j]=0; water[i-1,j]=1; }
                }
                else if (i<9&&n5==-1&&(water[i,j]==1||water[i,j]==2)&&water[i+1,j]!=1&&water[i+1,j]!=2&&
                    (Ground.Instance.GetBlockID(new Vector2Int(i+1,j/16))==0||
                     Ground.Instance.GetBlockID(new Vector2Int(i+1,j/16))==56||
                     Ground.Instance.GetBlockID(new Vector2Int(i+1,j/16))==58))
                    { water[i,j]=0; water[i+1,j]=1; }
            }
            sec++; j++;
            if (j>=320) { j=0; i++; }
        }
        // Phase4: update grid blocks
        i=0; j=0;
        int n6=0;
        bool full=true;
        while (i<10)
        {
            if (water[i,j*16+n6]!=1) full=false;
            n6++;
            if (n6>=16)
            {
                if (full)
                    Ground.Instance.SetBlock(new Vector2Int(i,j),58);
                else if (Ground.Instance.GetBlockID(new Vector2Int(i,j))==58)
                    Ground.Instance.SetBlock(new Vector2Int(i,j),0);
                full=true; n6=0; j++;
            }
            if (j>=20) { j=0; i++; }
        }
        // Phase5: visual refresh (simplified - no WaterKid prefabs)
        // APK: ResetShowingWater() creates/destroys WaterKid GameObjects
    }
}
