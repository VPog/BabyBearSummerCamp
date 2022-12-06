using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BearManager : MonoBehaviour
{
    private List<BearController> selectedBears = new List<BearController>();
    public GameObject mainTilemap; // This is the walkable tiles
    public Camera camera;
    public bool shiftPressed {
        get => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
    public ReadOnlyCollection<BearController> SelectedBears
    {
        get => selectedBears.AsReadOnly();
    }
    public void DeselectAllBears()
    {
        foreach (BearController bear in selectedBears)
        {
            bear.Selected = false;
        }
        selectedBears.Clear();
    }
    public void SelectBear(BearController bear)
    {
        bear.Selected = true;
        selectedBears.Add(bear);
        SelectedAnyBearThisFrame = true;
    }

    public bool SelectedAnyBearThisFrame = false;
    Tilemap grid;

    // Start is called before the first frame update
    void Start()
    {
        grid = mainTilemap.GetComponent<Tilemap>();
    }

    Vector2Int PosOnGrid(Tilemap grid, Vector3 worldPos)
    {
        // Get the position on the grid
        var pos = grid.WorldToCell(worldPos);
        return new Vector2Int(pos.x, pos.y);

        //Vector3 positionRelativeToGrid = mainTilemap.transform.InverseTransformPoint(worldPos);
        //return new Vector2Int((int)(positionRelativeToGrid.x / (int)grid.cellSize.x * (int)grid.cellSize.x), (int)(positionRelativeToGrid.y / (int)grid.cellSize.y * (int)grid.cellSize.y));
    }

    TileBase TileAtGridPos(Tilemap grid, Vector2Int gridPos)
    {
        return grid.GetTile(new Vector3Int(gridPos.x, gridPos.y, 0));
    }

    TileBase TileAtWorldPos(Tilemap grid, Vector3 worldPos)
    {
        return TileAtGridPos(grid, PosOnGrid(grid, worldPos));
    }

    public Vector3 TilePosToWorldPos(Tilemap grid, Vector2Int tilePos)
    {
        // Get the position on the grid
        var pos = grid.CellToWorld(new Vector3Int(tilePos.x, tilePos.y, 0));
        if (selectedBears.Count > 0)
        {
            var selectedBear = selectedBears[0];
            Collider2D collider = selectedBear.GetComponent<Collider2D>();
            pos.x += collider.bounds.size.x / 2.0f;
            pos.y += collider.bounds.size.y;
        }
        return pos;

        //Vector3 posInWorld = mainTilemap.transform.TransformVector(new Vector3(tilePos.x * grid.cellSize.x, tilePos.y * grid.cellSize.y, 0));
        //return posInWorld;
    }
    public Vector3 TilePosToWorldPos(Vector2Int tilePos)
    {
        return TilePosToWorldPos(mainTilemap.GetComponent<Tilemap>(), tilePos);
    }

    int EdgeWeight(Vector2Int a, Vector2Int b)
    {
        //Tile tileA = grid.GetTile<Tile>(new Vector3Int(a.x, a.y, 0));
        Tile tileB = grid.GetTile<Tile>(new Vector3Int(b.x, b.y, 0));
        if (tileB != null && tileB.sprite.name == "transparent_grey"
            )
        {
            return 1;
        }
        return int.MaxValue;
    }

    void Update()
    {
        if (!SelectedAnyBearThisFrame && Input.GetMouseButtonDown(0)) // Left mouse click
        {
            // This should run before any bear receives the click event

            if (selectedBears.Count > 0)
            {
                foreach (BearController SelectedBear in selectedBears)
                {
                    // Give pathfinding command at the position of the click, if the tile is reachable etc.
                    //print(Input.mousePosition);
                    // Get our click position in the world
                    Vector3 position = camera.ScreenToWorldPoint(Input.mousePosition);
                    // Get our click position on the grid and then get the tile at that position
                    Vector2Int clickCellPos = PosOnGrid(grid, position);
                    // Get tile at click position
                    TileBase tileDest = TileAtGridPos(grid, clickCellPos);

                    // Get our bear's position on the grid and then get the tile at that position
                    Vector2Int bearCellPos = PosOnGrid(grid, SelectedBear.transform.position);
                    // Get tile at bear position
                    //TileBase tileBear = TileAtGridPos(grid, bearCellPos);

                    print("Pathfinding from " + bearCellPos + " to " + clickCellPos);
                    IList<Vector2Int> pathToFollow = AStar.A_Star(EdgeWeight, bearCellPos, clickCellPos, new Vector2Int(grid.size.x, grid.size.y));
                    string result = "";
                    foreach (var item in pathToFollow)
                    {
                        result += item.ToString() + ", ";
                    }
                    print("resulting path: " + result);

                    SelectedBear.PathToFollow = pathToFollow;
                }
            }

            // Deselect any selected bears
            //SelectedBear = null;
        }
        SelectedAnyBearThisFrame = false;
    }
}
