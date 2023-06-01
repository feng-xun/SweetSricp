using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLineSweet : ClearedSweet
{
    public bool isRow;
    public override void Clear()
    {
        base.Clear();
        //判断是行消除还是列消除
        if (isRow)
        {
            sweet.gameManager.ClearRow(sweet.Y);
        }
        else
        {
            sweet.gameManager.ClearColumn(sweet.X);
        }
    }
}
