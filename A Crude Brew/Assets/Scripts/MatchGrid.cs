using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchGrid : MonoBehaviour
{
    public int columns;
    public int rows;
    public float padding = 0.2f;
    public GameObject highlightRow;
    public GameObject highlightColumn;

    public GameObject[] jars;
    public GameObject itemPrefab;

    private GameObject[,] components;
    private MatchComponent[,] componentRefs;
    public Sprite[] componentSprites = new Sprite[6]; // Set to the array of sprites: Raindrop, Tooth, Vial, Feather, Horn, Yarn
    public GameObject[] destroyAnimations = new GameObject[6];

    private int[,] matchKeeper; // Records 0 for no matches, or 3+ for the match size in the grid

    public Plane gridPlane; // An xyz coordinate plane used to determine mouse collision; predefeined to be pointing in the -z plane
    public Vector3 mousePosition;
    private bool initialized = false;
    private int framesBeforeAcceptInput;

    public AudioClip onComponentPlace;
    public AudioClip onComponentSwap;
    public AudioClip onMatch;

    // Start is called before the first frame update
    void Start()
    {
        components = new GameObject[columns, rows];
        componentRefs = new MatchComponent[columns, rows];
        matchKeeper = new int[columns, rows];
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                CreateComponent(c, r);
            }
        }
        gridPlane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), transform.position);

        while (HandleMatches(componentRefs)); // Recursively handle matches until there are no more remaining
        CementGrid();
        initialized = true;
        framesBeforeAcceptInput = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(framesBeforeAcceptInput > 0)
        {
            if(framesBeforeAcceptInput == 1)
            {
                HandleMatches(componentRefs);
            }
            framesBeforeAcceptInput--;
        }
    }

    /// <summary>
    /// Creates a new random component at the indexed row and column
    /// </summary>
    /// <param name="column">Column to place the component in (0 = left, columns - 1 = right)</param>
    /// <param name="row">Row to place the component in (0 = bottom, rows - 1 = top)</param>
    public void CreateComponent(int column, int row)
    {
        GameObject newComponent = Instantiate(
            itemPrefab, // prefab
            IndexToWorldPos(new Vector2Int(column, row)), //position
            Quaternion.identity, //rotation
            gameObject.transform // parent
            );
        newComponent.GetComponent<MatchComponent>().Initialize(gameObject, new Vector2Int(column, row));

        components[column, row] = newComponent;
        componentRefs[column, row] = newComponent.GetComponent<MatchComponent>();

        componentRefs[column, row].SetLocation(new Vector2Int(column, row));

        componentRefs[column, row].SetComponentType((Component)Random.Range(0, 6));
    }

    public void SetSelectionCross(Vector2Int columnRow)
    {
        highlightRow.transform.position = IndexToWorldPos(new Vector2Int((columns - 1) / 2, columnRow.y));
        highlightColumn.transform.position = IndexToWorldPos(new Vector2Int(columnRow.x, (rows - 1) / 2));
    }

    /// <summary>
    /// Returns the current mousePosition on the grid
    /// </summary>
    /// <returns>Vector3 with the z value attached to the grid</returns>
    public Vector3 GetMousePosition()
    {
        Ray mouseRaycast = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distFromCamera;
        if (gridPlane.Raycast(mouseRaycast, out distFromCamera))
        {
            mousePosition = mouseRaycast.GetPoint(distFromCamera);
        }
        return mousePosition;
    }

    /// <summary>
    /// Casts a Vector3 global location to a 2D index in the grid
    /// </summary>
    /// <param name="position">Global coordinates of a component</param>
    /// <returns>Vector2Int(column, row)</returns>
    public Vector2Int WorldPosToIndex(Vector3 position)
    {
        Vector2 gridToComponent = new Vector2(position.x - transform.position.x,position.y - transform.position.y);
        Vector2Int offsetToIndex = new Vector2Int(
            (int)((gridToComponent.x / transform.localScale.x) / (1.0f + padding)),
            (int)((gridToComponent.y / transform.localScale.y) / (1.0f + padding))
            );

        return offsetToIndex;
    }

    /// <summary>
    /// Creates a 2D grid of what the grid would be if the component was swapped
    /// </summary>
    /// <param name="currentHardPosition">currentHardPosition of the MatchComponent that's being swapped</param>
    /// <param name="newPosition">transform.position of the MatchComponent that's being swapped</param>
    /// <returns>2D array of MatchComponents with the implemented swap if it's a valid swap; returns null for an invalid swap</returns>
    private MatchComponent[,] BuildSwapArray(Vector2Int originIndex, Vector2Int newIndex)
    {
        Vector2Int offset = newIndex - originIndex;
        MatchComponent tempComponent;

        if(newIndex.x < 0 || newIndex.x >= columns || newIndex.y < 0 || newIndex.y >= rows)
        {
            return null;
        }
        if((offset.x == 0 && offset.y == 0) || (offset.x != 0 && offset.y != 0))
        {
            return null;
        }

        MatchComponent[,] swapComponentGrid = new MatchComponent[columns, rows];
        for(int c = 0; c < columns; c++)
        {
            for(int r = 0; r < rows; r++)
            {
                swapComponentGrid[c, r] = componentRefs[c, r];
            }
        }

        // Component was moved either left or right
        if(offset.x != 0)
        {
            if(offset.x < 0)
            {
                for(int i = originIndex.x; i > originIndex.x + offset.x; i--)
                {
                    tempComponent = swapComponentGrid[i, originIndex.y];
                    swapComponentGrid[i, originIndex.y] = swapComponentGrid[i - 1, originIndex.y];
                    swapComponentGrid[i - 1, originIndex.y] = tempComponent;
                }
            }
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
            if(offset.y > 0)
            {
                for (int i = originIndex.y; i < originIndex.y + offset.y; i++)
                {
                    tempComponent = swapComponentGrid[originIndex.x, i];
                    swapComponentGrid[originIndex.x, i] = swapComponentGrid[originIndex.x, i + 1];
                    swapComponentGrid[originIndex.x, i + 1] = tempComponent;
                }
            }
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
    /// <summary>
    /// Reads in a component swap and checks if the swap results in a match
    /// If a match is made, the components are added into the jar
    /// If a match is not made, all pieces return to their original grid location
    /// </summary>
    /// <param name="originIndex">gridRef.WorldPosToIndex(currentHardPosition)</param>
    /// <param name="newIndex">gridRef.WorldPosToIndex(currentObjectivePosition);</param>
    public void CheckSwap(Vector2Int originIndex, Vector2Int newIndex)
    {
        MatchComponent[,] swapComponentGrid = BuildSwapArray(originIndex, newIndex);
        if (!HandleMatches(swapComponentGrid))
        {
            ReturnToHardPos();
        }
    }

    /// <summary>
    /// Checks all rows and columns for potential matches; saves any matches made in the 2D array matchKeeper
    /// </summary>
    /// <param name="swapComponentGrid">2D grid of components to check for a match, or componentRefs for no swap (initialization or combos)</param>
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

        return matchWasMade;
    }

    /// <summary>
    /// Deletes all objects that have a completed match from the grid, makes the components fall down, and initializes new components at the top of the grid
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
                    if(initialized)
                    {
                        Instantiate(
                            destroyAnimations[(int)componentRefs[c, r].type], // prefab
                            IndexToWorldPos(componentRefs[c, r].columnRow), //position
                            Quaternion.identity, //rotation
                            gameObject.transform // parent
                            );

                        // play match audio
                        GameObject.Find("AudioManager").GetComponent<AudioSource>().PlayOneShot(onMatch);
                    }
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
                        componentRefs[c, r - vertOffset].SetLocationNoTransform(new Vector2Int(c, r - vertOffset));
                        components[c, r] = null;
                        componentRefs[c, r] = null;
                    }
                }
            }
            for(int i = 0; i < vertOffset; i++)
            {
                CreateComponent(c, rows - vertOffset + i);
                componentRefs[c, rows - vertOffset + i].transform.position = IndexToWorldPos(new Vector2Int(c, rows + i));
            }
        }
        matchKeeper = new int[columns, rows];
    }

    /// <summary>
    /// Checks to see if the given grid has matches; if so, it cements the swap locations and removes the pieces from the swap
    /// </summary>
    /// <param name="swapComponentGrid">2D grid containing a swap, or componentRefs if no swap was made (initialization or combos)</param>
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
            // play placement of component audio
            GameObject.Find("AudioManager").GetComponent<AudioSource>().PlayOneShot(onComponentPlace);
            RemoveMatches();
            framesBeforeAcceptInput = 20;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the objective and hard positions of any components post-swap that do not match the current grid state
    /// </summary>
    /// <param name="swapComponentGrid">2D grid containing the swap</param>
    public void ConfirmSwap(MatchComponent[,] swapComponentGrid)
    {
        componentRefs = swapComponentGrid;
        for(int c = 0; c < columns; c++)
        {
            for(int r = 0; r < rows; r++)
            {
                componentRefs[c, r] = swapComponentGrid[c, r];
                components[c, r] = componentRefs[c, r].gameObject;
                componentRefs[c, r].SetLocation(new Vector2Int(c, r));
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
    /// <param name="columnRow">Vector2Int(column, row) of the grid</param>
    /// <returns>Global coordinates centered at the indexed location</returns>
    public Vector3 IndexToWorldPos(Vector2Int columnRow)
    {
        int column = columnRow.x;
        int row = columnRow.y;
        return new Vector3(
            transform.position.x + transform.localScale.x * ((column + 0.5f) * (1.0f + padding)),
            transform.position.y + transform.localScale.y * ((row + 0.5f) * (1.0f + padding)),
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
    /// Called by the Start() method when no matches exist anymore
    /// </summary>
    public void CementGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                componentRefs[c, r].SetLocation(new Vector2Int(c, r));
            }
        }
    }

    /// <summary>
    /// Sets the objective locations for all dragged that share a row or column with the dragged component
    /// </summary>
    /// <param name="columnRow">MatchComponent.columnRow</param>
    /// <param name="newColumnRow">gridRef.WorldPosToIndex(mouseWorld - mouseOffset)</param>
    public void DragAdjacentPieces(Vector2Int columnRow, Vector2Int newColumnRow)
    {
        // TODO: Discover why the branch is not properly resetting the positions on a failed move
        // If the match is invalid, reset all adjacent pieces to their original positions
        if ((newColumnRow.x < 0 || newColumnRow.x >= columns || newColumnRow.y < 0 || newColumnRow.y >= rows)
        || (newColumnRow.x - columnRow.x != 0 && newColumnRow.y - columnRow.y != 0)
        || newColumnRow == columnRow)
        {
            for(int c = 0; c < columns; c++)
            {
                if(c != columnRow.x)
                {
                    componentRefs[c, columnRow.y].SetObjectiveLocation(new Vector2Int(c, columnRow.y));
                }
            }
            for(int r = 0; r < rows; r++)
            {
                if(r != columnRow.y)
                {
                    componentRefs[columnRow.x, r].SetObjectiveLocation(new Vector2Int(columnRow.x, r));
                }
            }
            return;
        }

        // Set the position for all components in the same row or column besides the active one
        // Move the row
        for(int c = 0; c < columns; c++)
        {
            if(c >= newColumnRow.x && c < columnRow.x)
            {
                componentRefs[c, columnRow.y].SetObjectiveLocation(new Vector2Int(c + 1, columnRow.y));
            }
            else if(c > columnRow.x && c <= newColumnRow.x)
            {
                componentRefs[c, columnRow.y].SetObjectiveLocation(new Vector2Int(c - 1, columnRow.y));
            }
            else if(c != columnRow.x)
            {
                componentRefs[c, columnRow.y].SetObjectiveLocation(new Vector2Int(c, columnRow.y));
            }
        }
        // Move the column
        for (int r = 0; r < rows; r++)
        {
            if (r >= newColumnRow.y && r < columnRow.y)
            {
                componentRefs[columnRow.x, r].SetObjectiveLocation(new Vector2Int(columnRow.x, r + 1));
            }
            else if(r > columnRow.y && r <= newColumnRow.y)
            {
                componentRefs[columnRow.x, r].SetObjectiveLocation(new Vector2Int(columnRow.x, r - 1));
            }
            else if(r != columnRow.y)
            {
                componentRefs[columnRow.x, r].SetObjectiveLocation(new Vector2Int(columnRow.x, r));
            }
        }
    }

    /// <summary>
    /// Function that ensures components cannot be moved before match combos are finished
    /// </summary>
    /// <returns>True if the mouse input can be taken, else false</returns>
    public bool AcceptMouseInput()
    {
        return framesBeforeAcceptInput <= 0;
    }
}
