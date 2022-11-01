using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BearManager : MonoBehaviour
{
    private BearController selectedBear = null;
    public GameObject mainTilemap; // This is the walkable tiles
    public Camera camera;
    public BearController SelectedBear
    {
        get => selectedBear;
        set
        {
            if (selectedBear != null)
            {
                selectedBear.Selected = false;
            }

            selectedBear = value;

            if (selectedBear != null)
            {
                selectedBear.Selected = true;
            }
            SelectedAnyBearThisFrame = true;
        }
    }

    public bool SelectedAnyBearThisFrame = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector2Int PosOnGrid(Tilemap grid, Vector3 worldPos)
    {
        // Get the position on the grid
        Vector3 positionRelativeToGrid = mainTilemap.transform.InverseTransformPoint(worldPos);
        return new Vector2Int((int)(positionRelativeToGrid.x / (int)grid.cellSize.x * (int)grid.cellSize.x), (int)(positionRelativeToGrid.y / (int)grid.cellSize.y * (int)grid.cellSize.y));
    }

    TileBase TileAtGridPos(Tilemap grid, Vector2Int gridPos)
    {
        return grid.GetTile(new Vector3Int(gridPos.x, gridPos.y, 0));
    }

    TileBase TileAtWorldPos(Tilemap grid, Vector3 worldPos)
    {
        return TileAtGridPos(grid, PosOnGrid(grid, worldPos));
    }

    int EdgeWeight(Vector2Int a, Vector2Int b)
    {
        return 1;
    }

    void Update()
    {
        if (!SelectedAnyBearThisFrame && Input.GetMouseButtonDown(0)) // Left mouse click
        {
            // This should run before any bear receives the click event

            if (SelectedBear != null)
            {
                // Give pathfinding command at the position of the click, if the tile is reachable etc.
                //print(Input.mousePosition);
                Tilemap grid = mainTilemap.GetComponent<Tilemap>();
                // Get our click position in the world
                Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
                // Get our click position on the grid and then get the tile at that position
                Vector2Int clickCellPos = PosOnGrid(grid, position);
                // Get tile at click position
                TileBase tileDest = TileAtGridPos(grid, clickCellPos);

                // Get our bear's position on the grid and then get the tile at that position
                Vector2Int bearCellPos = PosOnGrid(grid, SelectedBear.transform.position);
                // Get tile at bear position
                TileBase tileBear = TileAtGridPos(grid, bearCellPos);

                print("Pathfinding from " + bearCellPos + " to " + clickCellPos);
                IList<Vector2Int> pathToFollow = AStar.A_Star(EdgeWeight, bearCellPos, clickCellPos, new Vector2Int(grid.size.x, grid.size.y));
                string result = "";
                foreach (var item in pathToFollow)
                {
                    result += item.ToString() + ", ";
                }
                print("resulting path: " + result);

                SelectedBear.PathToFollow = result;
            }

            // Deselect any selected bears
            //SelectedBear = null;
        }
        SelectedAnyBearThisFrame = false;
    }
}
