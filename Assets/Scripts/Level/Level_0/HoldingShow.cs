using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingShow : MonoBehaviour
{
    public FallingBlock fb;

    private FallingGroupShow show;

    private void Awake()
    {
        show = GetComponent<FallingGroupShow>();
        fb.OnHold += Hold;
    }

    private void Hold()
    {
        show.Show(fb.Holding, FallingBlock.Rotations.Zero);
        int[] defaultIDs = { 3, 3, 3, 3 };
        show.SetBlockSprites(defaultIDs);
    }
}
