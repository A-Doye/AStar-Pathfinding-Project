using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

// Each tile has its own movement handler that is used when it is being moved to
public class MovementHandler : MonoBehaviour
{
    // Coord & map variables set when created
    public int tileX;
    public int tileZ;
    public TileMap map;

    
    // On mouse over fires when the mouse is over the instance's collision box
    void OnMouseOver()
    {
        if(Input.GetMouseButton(1))
        {
            map.MoveUnitTo(tileX, tileZ);
        }
    }
}
