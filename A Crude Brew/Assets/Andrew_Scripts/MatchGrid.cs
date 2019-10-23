using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchGrid : MonoBehaviour
{
    public int columns;
    public int rows;

    public int activeRow;
    public int activeColumn;

    public float padding = 0.2f;

    public Vector3 mousePosition;

    public GameObject[] jars;
    public GameObject itemPrefab;

    private GameObject[,] components;
    private MatchComponent[,] componentRefs;

    private int[,] matchKeeper; // Records 0 for no matches, or 3+ for the match size in the grid
    private bool recheckFlag = false;

    public Plane gridPlane; // An xyz coordinate plane used to determine mouse collision; predefeined to be pointing in the -z plane

    // the two following arrays must be the same length
    public Sprite[] componentSprites = new Sprite[6]; // Set to the array of sprites: Raindrop, Tooth, Vial, Feather, Horn, Yarn

    // Start is called before the first frame update
    void Start()
    {
        components = new GameObject[columns, rows];
        componentRefs = new MatchComponent[columns, rows];
        matchKeeper = new int[columns, rows];
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                CreateComponent(j, i);
            }
        }
        gridPlane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), transform.position);

        HandleMatches(componentRefs);
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Creates a new random component at the indexed row and column
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void CreateComponent(int row, int column)
    {
        activeRow = row;
        activeColumn = column;

        GameObject newComponent = Instantiate(
            itemPrefab, // prefab
            IndexToWorldPos(new Vector2Int(row, column)), //position
            Quaternion.identity, //rotation
            gameObject.transform // parent
            );

        components[column, row] = newComponent;
        componentRefs[column, row] = newComponent.GetComponent<MatchComponent>();
        componentRefs[column, row].SetLocation(new Vector2Int(row, column));

        // Select a random component type based off the length of the Component enum
        componentRefs[column, row].SetComponentType((Component)Random.Range(0, 6));
    }

    /// <summary>
    /// Sets mousePosition relative to the grid plane; called through OnMouseDown and OnMouseDrag for clicked grid components
    /// </summary>
    public void SetMousePosition()
    {
        // Get a raycast based off the mouse and the camera perspective
        Ray mouseRaycast = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distFromCamera;
        if (gridPlane.Raycast(mouseRaycast, out distFromCamera))
        {
            mousePosition = mouseRaycast.GetPoint(distFromCamera);
        }
    }

    /// <summary>
    /// Returns the current mousePosition on the grid
    /// </summary>
    /// <returns>mousePosition</returns>
    public Vector3 GetMousePosition()
    {
        return mousePosition;
    }

    /// <summary>
    /// Casts a Vector3 global location to a 2D index in the grid
    /// </summary>
    /// <param name="position">Global coordinates of a component</param>
    /// <returns>{ column, row }</returns>
    public Vector2Int ComponentPositionToIndex(Vector3 position)
    {
        return new Vector2Int(
            (int)((position.x - transform.position.x + transform.localScale.x * (0.5f + (padding/2.0f))) / (transform.localScale.x * (1.0f + padding))),
            (int)((position.y - transform.position.y + transform.localScale.y * (0.5f + (padding/2.0f))) / (transform.localScale.y * (1.0f + padding)))
            );

    }

    /// <summary>
    /// Creates a 2D grid of what the grid would be if the component was swapped
    /// </summary>
    /// <param name="currentHardPosition">currentHardPosition of the MatchComponent that's being swapped</param>
    /// <param name="newPosition">transform.position of the MatchComponent that's being swapped</param>
    /// <returns>2D array of MatchComponents with the implemented swap if it's a valid swap; returns null for an invalid swap</returns>
    private MatchComponent[,] BuildSwapArray(Vector3 currentHardPos, Vector3 newPos)
    {
        // Translates the locations to 2D grid coordinates
        Vector2Int originIndex = ComponentPositionToIndex(currentHardPos);
        Vector2Int newIndex = ComponentPositionToIndex(newPos);
        Vector2Int offset = newIndex - originIndex;
        MatchComponent tempComponent;

        // If the new position is outside the grid, return null
        if(newIndex.x < 0 || newIndex.x > columns || newIndex.y < 0 || newIndex.y > rows)
        {
            return null;
        }
        // Return null if the component is swapping both rows/columns or neither
        if((offset.x == 0 && offset.y == 0) || (offset.x != 0 && offset.y != 0))
        {
            return null;
        }

        // A valid swap has been made; move the rows/columns, then return the swapped value
        MatchComponent[,] swapComponentGrid = new MatchComponent[columns, rows];
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                swapComponentGrid[i, j] = components[i, j].GetComponent<MatchComponent>();
            }
        }

        // Component was moved either left or right
        if(offset.x != 0)
        {
            // Component was moved left
            if(offset.x < 0)
            {
                for(int i = originIndex.x; i > originIndex.x + offset.x; i--)
                {
                    tempComponent = swapComponentGrid[i, originIndex.y];
                    swapComponentGrid[i, originIndex.y] = swapComponentGrid[i - 1, originIndex.y];
                    swapComponentGrid[i - 1, originIndex.y] = tempComponent;
                }
            }
            // Component was moved right
            else
            {
                for (int i = originIndex.x; i < originIndex.x + offset.x; i++)
                {
                    tempComponent = swapComponentGrid[i, originIndex.y];
                    swapComponentGrid[i, originIndex.y] = swapComponentGrid[i + 1, originIndex.y];
                    swapComponentGrid[i + 1, originIndex.y] = tempComponent;
                }
            }
        }
        // Component was moved either up or down
        else 
        {
            // Component was moved up
            if(offset.y > 0)
            {
                for (int i = originIndex.y; i < originIndex.y + offset.y; i++)
                {
                    tempComponent = swapComponentGrid[originIndex.x, i];
                    swapComponentGrid[originIndex.x, i] = swapComponentGrid[originIndex.x, i + 1];
                    swapComponentGrid[originIndex.x, i + 1] = tempComponent;
                }
            }
            // Component was moved down
            else
            {
                for (int i = originIndex.y; i > originIndex.y + offset.y; i--)
                {
                    tempComponent = swapComponentGrid[originIndex.x, i];
                    swapComponentGrid[originIndex.x, i] = swapComponentGrid[originIndex.x, i - 1];
                    swapComponentGrid[originIndex.x, i - 1] = tempComponent;
                }
            }
        }
        return swapComponentGrid;
    }

    /// <summary>
    /// Reads in a component swap and checks if the swap results in a match
    /// If a match is made, the components are added into the jar
    /// If a match is not made, all pieces return to their original grid location
    /// </summary>
    /// <param name="currentHardPos">currentHardPos of the MatchComponent being moved</param>
    /// <param name="newPos">transform.position of the MatchComponent being moved</param>
    /// <returns>True if a match was made, else returns false</returns>
    public void CheckSwap(Vector3 currentHardPos, Vector3 newPos)
    {
        MatchComponent[,] swapComponentGrid = BuildSwapArray(currentHardPos, newPos);
        // If a match was not made, make all pieces return to their currentHardPosition
        if (!HandleMatches(swapComponentGrid))
        {
            ReturnToHardPos();
        }
    }

    /*public bool CheckSwap(Vector3 aPos, Vector3 bPos)
    {
        Vector2Int aIndex = ComponentPositionToIndex(aPos);
        Vector2Int bIndex = ComponentPositionToIndex(bPos);
        // Debug.Log($"a: {aIndex}, b: {bIndex}");

        // perform swap
        MatchComponent temp = componentRefs[aIndex.x, aIndex.y];
        componentRefs[aIndex.x, aIndex.y] = componentRefs[bIndex.x, bIndex.y];
        componentRefs[bIndex.x, bIndex.y] = temp;

        CheckGridForMatches(componentRefs);

        if (recheckFlag) // if swap causes match
        {
            // change transforms first because it's easier here
            components[aIndex.x, aIndex.y].transform.position = components[bIndex.x, bIndex.y].transform.position;
            components[bIndex.x, bIndex.y].transform.position = aPos;

            // perform swap on other array, do this one here for optimization
            GameObject tempObj = components[aIndex.x, aIndex.y];
            components[aIndex.x, aIndex.y] = components[bIndex.x, bIndex.y];
            components[bIndex.x, bIndex.y] = tempObj;

            // remove matches
            RemoveMatches();
            return true;
        }
        else
        {
            // swap back
            temp = componentRefs[aIndex.x, aIndex.y];
            componentRefs[aIndex.x, aIndex.y] = componentRefs[bIndex.x, bIndex.y];
            componentRefs[bIndex.x, bIndex.y] = temp;

            return recheckFlag;
        }
    }*/

    /// <summary>
    /// Checks all rows and columns for potential matches; saves any matches made in the 2D array matchKeeper
    /// </summary>
    /// <param name="swapComponentGrid">2D grid of components to check for a match (can set to componentRefs for current grid)</param>
    /// <returns>True if at least one match was made, else false</returns>
    private bool CheckGridForMatches(MatchComponent[,] swapComponentGrid)
    {
        bool matchWasMade = false;

        // column check
        for(int c = 0; c < columns; c++)
        {
            int currentStreak = 1;
            Component type;
            // Initialize the previousType to the first value in the column; since currentStreak is set to 0, it will be incremented to 1
            Component previousType = swapComponentGrid[c, 0].type;

            for (int r = 1; r < rows; r++)
            {
                //Debug.Log($"{c}, {r}");
                type = swapComponentGrid[c, r].type;

                if(type != previousType)
                {
                    // If the streak is broken with less than 3 rows remaining (indexed at 0), stop checking the column
                    if (r > rows - 3)
                    {
                        break;
                    }
                    currentStreak = 1;
                }
                else
                {
                    currentStreak++;
                }

                if(currentStreak >= 3)
                {
                    matchWasMade = true;
                    for (int i = r; i > r - currentStreak; i--)
                    {
                        matchKeeper[c, i] = currentStreak;
                    }
                }
                previousType = type;
            }
        }

        // row check
        // same as above but for rows
        for(int r = 0; r < rows; r++)
        {
            int currentStreak = 1;
            Component type;
            // Initialize the previousType to the first value in the row; since currentStreak is set to 0, it will be incremented to 1
            Component previousType = swapComponentGrid[0, r].type;

            for (int c = 1; c < columns; c++)
            {
                type = swapComponentGrid[c, r].type;

                if (type != previousType)
                {
                    // If the streak is broken with less than 3 columns remaining (indexed at 0), stop checking the row
                    if (c > columns - 3)
                    {
                        break;
                    }
                    currentStreak = 1;
                }
                else
                {
                    currentStreak++;
                }

                if (currentStreak >= 3)
                {
                    matchWasMade = true;
                    for (int i = c; i > c - currentStreak; i--)
                    {
                        // TODO: Ensure that a cross section doesn't have the middle value overwritten (e.g. 4 on column, 3 on row will set the value to 3)
                        matchKeeper[i, r] = currentStreak;
                    }
                }
                previousType = type;
            }
        }
        // vibe check
        // ...

        // debug to check match array
        //string arrString = "";
        //for (int i = 0; i < rows; i++)
        //{
        //    string row = "";
        //    for (int j = 0; j < columns; j++)
        //        row += $"{matchKeeper[j, i]}{componentRefs[j, i].type.ToCharArray()[0]}, ";

        //    row = row.Substring(0, row.Length - 2) + '\n';
        //    arrString += row;
        //}
        //Debug.Log(arrString);

        return matchWasMade;
    }

    /// <summary>
    /// Deletes all objects that have a completed match from the grid
    /// </summary>
    public void RemoveMatches()
    {
        // Number of components destroyed in the column so far
        int vertOffset;

        for (int c = 0; c < columns; c++)
        {
            vertOffset = 0;
            for(int r = 0; r < rows; r++)
            {
                if (matchKeeper[c, r] != 0)
                {
                    // A match was made; add the component to its corresponding jar
                    jars[(int)componentRefs[c, r].type].GetComponent<Jar>().AddComponent();
                    Destroy(components[c, r]);

                    // Increment vertOffset instead of r since r contained a destroyed component
                    vertOffset++;
                    components[c, r] = null;
                    componentRefs[c, r] = null;
                }
                else
                {
                    if(vertOffset > 0)
                    {
                        components[c, r - vertOffset] = components[c, r];
                        componentRefs[c, r - vertOffset] = componentRefs[c, r];
                        componentRefs[c, r - vertOffset].SetLocation(new Vector2Int(r - vertOffset, c));
                        components[c, r] = null;
                        componentRefs[c, r] = null;
                    }
                }
            }
            while(vertOffset > 0)
            {
                CreateComponent(rows - vertOffset, c);
                vertOffset--;
            }
        }
        matchKeeper = new int[columns, rows];

        // Set the transform.position, currentHardPosition and currentObjectivePosition of all match components to their current row/column
        CementGrid();

        // Recursively test matches for combos
        HandleMatches(componentRefs);
    }

    /// <summary>
    /// Checks to see if the given grid has matches; if so, it cements the swap locations and removes the pieces from the swap
    /// </summary>
    /// <param name="swapComponentGrid"></param>
    /// <returns>True if a match was made, false if no matches were made</returns>
    public bool HandleMatches(MatchComponent[,] swapComponentGrid)
    {
        // If the component swapped to an invalid location, the grid will be set to null
        if(swapComponentGrid == null)
        {
            return false;
        }
        if (CheckGridForMatches(swapComponentGrid))
        {
            ConfirmSwap(swapComponentGrid);
            RemoveMatches();
            return true;
        }
        return false;
    }

    /// <summary>
    /// TODO: Sets the objective and hard positions of any components post-swap that do not match the current grid state
    /// </summary>
    /// <param name="swapComponentGrid">2D grid containing the swap</param>
    public void ConfirmSwap(MatchComponent[,] swapComponentGrid)
    {
        for(int c = 0; c < columns; c++)
        {
            for(int r = 0; r < rows; r++)
            {
                // TODO: SetLocation() the MatchComponents to the new row/column
                swapComponentGrid[c, r].SetLocation(swapComponentGrid[c, r].rowColumn);
            }
        }
    }

    /// <summary>
    /// Returns a Component sprite stored in componentSprites
    /// </summary>
    /// <param name="index">Component type</param>
    /// <returns>Sprite for the indexed component</returns>
    public Sprite GetSprite(Component index)
    {
        return componentSprites[(int)index];
    }

    /// <summary>
    /// Returns a world position based off the row/column
    /// </summary>
    /// <param name="row">Index of the row (left = 0, right = max - 1)</param>
    /// <param name="column">Index of the column (bottom = 0, top = max - 1)</param>
    /// <returns>Global coordinates centered around the indexed location</returns>
    public Vector3 IndexToWorldPos(Vector2Int rowColumn)
    {
        int row = rowColumn.x;
        int column = rowColumn.y;
        return new Vector3(
            transform.position.x + transform.localScale.x * (0.5f + column * 1.0f + column * padding),
            transform.position.y + transform.localScale.y * (0.5f + row * 1.0f + row * padding),
            transform.position.z);
    }

    /// <summary>
    /// Sets the objective position of all pieces back to their origin (undo any moved pieces)
    /// </summary>
    public void ReturnToHardPos()
    {
        for(int r = 0; r < rows; r++)
        {
            for(int c = 0; c < columns; c++)
            {
                componentRefs[c, r].SetObjectiveLocation();
            }
        }
    }

    /// <summary>
    /// Set the currentHardPosition and currentObjectivePosition for all components equal to their row and column position
    /// Called by ConfirmSwap(...) when a match is successfully made
    /// </summary>
    public void CementGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                componentRefs[c, r].SetLocation(new Vector2Int(r, c));
            }
        }
    }
}
