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
        // Only show if this preview slot exists in waiting list
        if (id < fb.waiting.Count)
        {
            show.Show(fb.waiting[id], FallingBlock.Rotations.Zero);
            // Use visible default block texture (id=3=dirt) so preview shows real textures, not red
            int[] defaultIDs = { 3, 3, 3, 3 };
            show.SetBlockSprites(defaultIDs);
        }
    }
}