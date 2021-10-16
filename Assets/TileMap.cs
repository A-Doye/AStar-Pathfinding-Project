using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap : MonoBehaviour {

    public GameObject selectedUnit;
    // Tile map needs to store:
    // What a tile is (tileTypes)
    // Where a tile is (tiles)
    // How to pathfind using a graph of these tiles

    public TileType[] tileTypes;

    int[,] tiles;
    Node[,] graph;

    // This only works with one unit on the map
    List<Node> currentPath = null;

    int mapSizeX = 12;
    int mapSizeZ = 8;

    void Start()
    {
        // Set unit's variables
        selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<Unit>().tileZ = (int)selectedUnit.transform.position.z;
        selectedUnit.GetComponent<Unit>().map = this;

        // Sets tiles to be the specified map size 
        tiles = new int[mapSizeX, mapSizeZ];

        // Auto fills tiles with TileType[0]
        for(int x=0; x < mapSizeX; x++)
        {
            for(int z=0; z < mapSizeZ; z++)
            {
                tiles[x,z] = 0;
            }
        }

        // Customizing new tiles
        // !Hard coded; Should find a better way to make maps
        // !Convert tile types to Enums to make it more clear
        tiles[4, 3] = 2;
        tiles[4, 4] = 2;
        tiles[4, 5] = 2;
        tiles[3, 4] = 2;
        tiles[5, 4] = 2;

        tiles[6, 6] = 1;
        tiles[7, 6] = 1;
        tiles[8, 6] = 1;

        //Generate random map from a seed
        //GenerateMapDesign();

        // Generate pathfinding graph, for use in pathfinding alogrithms
        GeneratePathfindingGraph();

        // Generate the map using tiles[] array
        GenerateMapVisual();
    }

    void GenerateMapDesign()
    {
        int seed = UnityEngine.Random.Range(10000, 99999);
        int seedLength = (int)Math.Floor(Math.Log10(seed) + 1);
        int[] digits = new int[seedLength];

        Debug.Log("Seed: "+seed);
        Debug.Log(seedLength);

        for (int i = 0; i < seedLength; i++)
        {
            string temp = seed.ToString();
            digits[i] = int.Parse(Char.ToString(temp[i]));
            Debug.Log(digits[i]);
        }


        int counter = 0;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                counter++;
                if ((seed - counter) % 10 == 2)
                {
                    tiles[x, z] = 2;
                }
            }
        }
    }

    void GeneratePathfindingGraph()
    {
        // Initialise array
        graph = new Node[mapSizeX, mapSizeZ];

        // Initialise nodes in the array. Creates an empty node for each tile
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                graph[x, z] = new Node();
                graph[x, z].x = x;
                graph[x, z].z = z;
            }
        }

        // Finds the neighbours of each node (tile)
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                // Finding neighbours for 4 way tiles, can be expanded
                // if statements handle map edges
                /*if (x > 0)
                    graph[x, y].neighbours.Add(graph[x-1, y]);
                if (x < mapSizeX-1)
                    graph[x, y].neighbours.Add(graph[x+1, y]);

                if (y > 0)
                    graph[x, y].neighbours.Add(graph[x, y-1]);
                if (y < mapSizeZ - 1)
                    graph[x, y].neighbours.Add(graph[x, y+1]);*/

                // 8 way movement
                // Try left
                if (x > 0)
                {
                    graph[x, z].neighbours.Add(graph[x - 1, z]);
                    if (z > 0)
                        graph[x, z].neighbours.Add(graph[x - 1, z - 1]);
                    if (z < mapSizeZ - 1)
                        graph[x, z].neighbours.Add(graph[x - 1, z + 1]);
                }
                // Try right
                if (x < mapSizeX - 1)
                {
                    graph[x, z].neighbours.Add(graph[x + 1, z]);
                    if (z > 0)
                        graph[x, z].neighbours.Add(graph[x + 1, z - 1]);
                    if (z < mapSizeZ - 1)
                        graph[x, z].neighbours.Add(graph[x + 1, z + 1]);
                }

                // Try up and down
                if (z > 0)
                    graph[x, z].neighbours.Add(graph[x, z - 1]);
                if (z < mapSizeZ - 1)
                    graph[x, z].neighbours.Add(graph[x, z + 1]);
            }
        }
    }

    // GenerateMapVisual finds what TileType each co-ord should be, and spawns them
    // in the world
    void GenerateMapVisual()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                TileType nextTile = tileTypes[ tiles[x,z] ];
                GameObject go = (GameObject)Instantiate(nextTile.tileVisualPrefab as GameObject, new Vector3(x, 0, z), Quaternion.identity);

                // Sets coord variables
                MovementHandler mh = go.GetComponent<MovementHandler>();
                mh.tileX = x;
                mh.tileZ = z;
                mh.map = this;
            }
        }
    }

    public float CostToEnterTile(int sourceX, int sourceZ, int targetX, int targetZ)
    {
        TileType tt = tileTypes[tiles[targetX, targetZ]];

        // This stops the unit from pathing through a tile
        if (UnitCanEnterTile(targetX, targetZ) == false)
            return Mathf.Infinity;

        float cost = tt.moveCost;

        if(sourceX!=targetX && sourceZ!=targetZ)
        {
            cost += 0.001f;
        }

        return cost;
    }    

    public Vector3 TileCoordToWorldCoord(int x, int z)
    {
        // Take in tile coord, return world coord (for now)
        return new Vector3(x, 0, z);
    }

    public bool UnitCanEnterTile(int x, int z)
    {
        // Test special unit passing here
        return tileTypes[tiles[x, z]].isPassable;
    }

    // Moving the unit
    public void MoveUnitTo(int x, int z)
    {
        selectedUnit.GetComponent<Unit>().currentPath = null;

        // This stops a player from selecting an impassable tile as their ultimate goal
        if (UnitCanEnterTile(x, z) == false)
        {
            return;
        }

        // ---Dijkstra's algorithm---

        // The dictionary data type stores unordered key-value pairs
        Dictionary<Node, float> dist = new Dictionary<Node, float>(); // Contains a node and its move cost
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();   // Contains a node and its most opitmal path

        // List of nodes not yet checked
        List<Node> unvisited = new List<Node>();

        // Source: Where we're coming from
        Node source = graph[selectedUnit.GetComponent<Unit>().tileX, 
                            selectedUnit.GetComponent<Unit>().tileZ];
        // Target: Where we'd like to go
        Node target = graph[x, z];

        // Set source as the starting point
        dist[source] = 0;
        prev[source] = null;


        // Initialise every other point to have infinity distance until true distance is found
        foreach(Node v in graph)
        {
            if(v != source)
            {
                dist[v] = Mathf.Infinity; // Distance set to infinity
                prev[v] = null;           // Therefor no path is to be set
            }
            unvisited.Add(v);
        }

        while (unvisited.Count > 0) // While there are still nodes to explore
        {
            // Slow, needs improvement. Make unvisted a "priority queue" (self sorting data structure)
            //Node u = unvisited.OrderBy(n => dist[n]).First(); // Order all remaining nodes, find the shortest one
            //unvisited.Remove(u);

            // "u" is the unvisited node with the shortest distance. Note the first node scanned is the origin, then
            // its neighbours distances with be discovered, allowing for further exploration
            Node u = null;

            foreach(Node possibleU in unvisited)
            {
                if(u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            // If we've arrived at the target cease path finding
            if(u == target)
            {
                break;
            }

            unvisited.Remove(u);

            // For every neighbour of a given node
            foreach (Node v in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(v); // Find distance to each neighbour (without considering tile restrictions)
                float alt = dist[u] + CostToEnterTile(u.x, u.z, v.x, v.z); // This makes it consider cost to move instead of the shortest route
                if (alt < dist[v]) // See if this distance is shorter than any other found
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        // By this point either: The shortest route has been found OR there is no possible route

        if (prev[target] == null)
        {
            // There is no route
            return;
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;

        // Works through prev chain to create a path FROM target TO source
        while(curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        // Reverse path to become from source to path
        currentPath.Reverse();

        selectedUnit.GetComponent<Unit>().currentPath = currentPath;
    }
}
