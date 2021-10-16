using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int tileX;
    public int tileZ;
    public TileMap map;
    int movePoints = 2;
    float remainingMovePoints = 2;

    public List<Node> currentPath = null;

    // Line renderer for showing path
    public int lengthOfRender = 2;

    void Start()
    {
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
    }

    // Draw path on map using debug
    void Update()
    {
        if (currentPath != null)
        {
            bool firstPass = true;
            var points = new Vector3[128];
            int currNode = 0;
            int n = 1;
            while (currNode < currentPath.Count-1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].z) +
                    new Vector3(0, -1f, 0);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode+1].x, currentPath[currNode+1].z) +
                    new Vector3(0, -1f, 0);

                // Draws path for user to see
                // !Convert to unity line renderer
                //Debug.DrawLine(start, end, Color.red);

                // Line renderer
                if (firstPass == true)
                {
                    points[0] = new Vector3(tileX, 0.75f, tileZ);
                    firstPass = false;
                }
                points[n] = end + new Vector3(0, 1.75f, 0);

                n += 1;
                currNode++;
            }
            LineRenderer lr = GetComponent<LineRenderer>();
            lr.positionCount = n;

            lr.SetPositions(points);

            n = 0;           
        }
        
        // Have we moved our visible piece close enough to the target tile that we can
        // advance to the next step in our pathfinding?
        if (Vector3.Distance(transform.position, map.TileCoordToWorldCoord(tileX, tileZ)) < 0.1f)
            MoveNextTile();

        // Smoothly animate towards the correct map tile.
        transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord(tileX, tileZ), 5f * Time.deltaTime);
    }

    // Moving, tile by tile
    public void MoveNextTile()
    {
        if (currentPath == null)
            return;

        if (remainingMovePoints <= 0)
            return;

        // Teleport us to our correct "current" position, in case we
        // haven't finished the animation yet.
        transform.position = map.TileCoordToWorldCoord(tileX, tileZ);

        // Get cost from current tile to next tile
        remainingMovePoints -= map.CostToEnterTile(currentPath[0].x, currentPath[0].z, currentPath[1].x, currentPath[1].z);

        // Move us to the next tile in the sequence
        tileX = currentPath[1].x;
        tileZ = currentPath[1].z;

        // Remove the old "current" tile from the pathfinding list
        currentPath.RemoveAt(0);

        if (currentPath.Count == 1)
        {
            // We only have one tile left in the path, and that tile MUST be our ultimate
            // destination -- and we are standing on it!
            // So let's just clear our pathfinding info.
            currentPath = null;
        }
    }

    // The "Next Turn" button calls this.
    public void NextTurn()
    {
        // Make sure to wrap-up any outstanding movement left over.
        while (currentPath != null && remainingMovePoints > 0)
        {
            MoveNextTile();
        }

        // Reset our available movement points.
        remainingMovePoints = movePoints;
    }
}
