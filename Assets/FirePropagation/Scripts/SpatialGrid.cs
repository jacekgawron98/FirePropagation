using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpatialGrid
{
    public float radius;
    public Dictionary<Vector3, FireCell> gridCells;

    public SpatialGrid()
    {
        gridCells = new Dictionary<Vector3, FireCell>();
    }
}
