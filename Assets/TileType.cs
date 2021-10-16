using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows unity to access this script directly (?)
[System.Serializable]
public class TileType
{

    public string name;
    public GameObject tileVisualPrefab;

    public bool isPassable = true;
    public float moveCost = 1;

}
