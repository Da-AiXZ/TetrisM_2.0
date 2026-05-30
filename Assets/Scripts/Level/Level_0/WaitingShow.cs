using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingShow : MonoBehaviour
{
    public int id;

    public FallingBlock fb;

    private FallingGroupShow show;

    private void Awake()
    {
        show = GetComponent<FallingGroupShow>();
        fb.OnReload += Reload;
    }

    private void Reload()
    {
        if (id < fb.waiting.Count)
        {
            show.Show(fb.waiting[id], FallingBlock.Rotations.Zero);
            int[] defaultIDs = { 3, 3, 3, 3 };
            show.SetBlockSprites(defaultIDs);
        }
        else if (fb.waiting.Count > 0)
        {
            // Edge case: show last available if slot beyond list
            show.Show(fb.waiting[fb.waiting.Count - 1], FallingBlock.Rotations.Zero);
            int[] defaultIDs = { 3, 3, 3, 3 };
            show.SetBlockSprites(defaultIDs);
        }
    }
}