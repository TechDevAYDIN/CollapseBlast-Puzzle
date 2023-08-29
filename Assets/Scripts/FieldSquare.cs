using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldSquare : MonoBehaviour
{
    public Vector2 size;
    public Vector2Int pos;

    public float offsetBase = 0.33f;
    public float pivotY;

    public void AdjustOffset()
    {
        pivotY = transform.position.y + (offsetBase * pos.y);
    }
    public Vector2 GetWorldPosition() => new Vector2(transform.position.x, pivotY);

}
