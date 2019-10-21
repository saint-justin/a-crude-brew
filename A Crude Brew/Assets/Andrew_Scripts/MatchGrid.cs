using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MatchKeeper
{
    int streak;
    Component type;
}

public class MatchGrid : MonoBehaviour
{
    public int columns;
    public int rows;

    public float padding = 0.2f;
    //public float gridWidth = 

    public Vector3 mousePosition;

    public GameObject[] jars;
    public GameObject itemPrefab;

    private GameObject[,] components;
    private MatchComponent[,] componentRefs;

    private int[,] matchKeeper; // eventually change to a struct that contains both a number and a type, based on struct above
    private bool recheckFlag = false;

    public Plane gridPlane; // An xyz coordinate plane used to determine mouse collision; predefeined to be pointing in the -z plane

    // the two following arrays must be the same length
    // Blue, White, Teal, Green, Orange, Red
    private Color[] colors = { new Color(0.21f, 0.34f, 0.80f, 1), new Color(1.0f, 1.0f, 1.0f, 1), new Color(0.63f, 0.91f, 0.88f, 1), new Color(0.26f, 0.73f, 0.21f, 1), new Color(0.925f, 0.63f, 0.26f, 1), new Color(1.0f, 0.0f, 0.0f, 1) };

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
                components[i, j] = Instantiate(
                    itemPrefab, // prefab
                    new Vector3( // position
                        gameObject.transform.position.x + (i + padding * i) * transform.localScale.x,
                        gameObject.transform.position.y + (j + padding * j) * transform.localScale.y,
                        gameObject.transform.position.z
                        ), 
                    Quaternion.identity, //rotation
                    gameObject.transform // parent
                    );

                componentRefs[i, j] = components[i, j].GetComponent<MatchComponent>();

                // setup back reference to grid
                componentRefs[i, j].gridRef = this;

                // Select a random component type based off the length of the Component enum
                Component tempType = (Component)Random.Range(0, System.Enum.GetNames(typeof(Component)).Length);
                componentRefs[i, j].type = tempType;
                components[i, j].GetComponent<Renderer>().material.SetColor("_Color", colors[(int)tempType]);
            }
        }
        gridPlane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), transform.position);

        CheckAllRowsAndColumns();
        RemoveMatches();
    }

    // Update is called once per frame
    void Update()
    {
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

    public Vector2Int ComponentPositionToIndex(Vector3 position)
    {
        return new Vector2Int(
            (int)((position.x - transform.position.x + transform.localScale.x * (0.5f + (padding/2.0f))) / (transform.localScale.x * (1.0f + padding))),
            (int)((position.y - transform.position.y + transform.localScale.y * (0.5f + (padding/2.0f))) / (transform.localScale.y * (1.0f + padding)))
            );

    }

    // returns 
    public bool CheckSwap(Vector3 aPos, Vector3 bPos)
    {
        Vector2Int aIndex = ComponentPositionToIndex(aPos);
        Vector2Int bIndex = ComponentPositionToIndex(bPos);
        // Debug.Log($"a: {aIndex}, b: {bIndex}");

        // perform swap
        MatchComponent temp = componentRefs[aIndex.x, aIndex.y];
        componentRefs[aIndex.x, aIndex.y] = componentRefs[bIndex.x, bIndex.y];
        componentRefs[bIndex.x, bIndex.y] = temp;

        CheckAllRowsAndColumns();

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
    }

    // This is where the matching check happens
    private void CheckAllRowsAndColumns()
    {
        recheckFlag = false;

        // column check
        for(int c = 0; c < columns; c++)
        {
            int currentStreak = 0;
            Component type;
            // Initialize the previousType to the first value in the column; since currentStreak is set to 0, it will be incremented to 1
            Component previousType = componentRefs[c, 0].type;

            for (int r = 0; r < rows; r++)
            {
                //Debug.Log($"{c}, {r}");
                type = componentRefs[c, r].type;

                // if not starting or streak is broken, set currentStreak to 1
                if(currentStreak == 0 || type != previousType)
                {
                    if (r > rows - 3) // If the streak is broken with less than 3 rows remaining (indexed at 0), stop checking the column
                        break;
                    currentStreak = 1;
                }
                else // if current obj has same type as previous type, add to streak
                    currentStreak++;

                // if there is a match record it, or if it extends further, rerecord it as the longer streak
                if(currentStreak >= 3)
                {
                    // indicates this calc needs to be handled, then run again
                    // set to true if there are ANY matches
                    recheckFlag = true;
                    // save info on each coordinate in "match keeper" array about current match
                    for (int i = r; i > r - currentStreak; i--)
                        matchKeeper[c, i] = currentStreak;
                }
                previousType = type;
            }
        }

        // row check
        // same as above but for rows
        for(int r = 0; r < rows; r++)
        {
            int currentStreak = 0;
            Component type;
            // Initialize the previousType to the first value in the row; since currentStreak is set to 0, it will be incremented to 1
            Component previousType = componentRefs[0, r].type;

            for (int c = 0; c < columns; c++)
            {
                type = componentRefs[c, r].type;

                // if not starting or streak is broken, set currentStreak to 1
                if (currentStreak == 0 || type != previousType)
                {
                    if (c > columns - 3) // If the streak is broken with less than 3 columns remaining (indexed at 0), stop checking the row
                        break;
                    currentStreak = 1;
                }
                else // if current obj has same type as previous type, add to streak
                    currentStreak++;

                // if there is a match record it, or if it extends further, rerecord it as the longer streak
                if (currentStreak >= 3)
                {
                    recheckFlag = true;
                    // save info on each coordinate in "match keeper" array about current match
                    for (int i = c; i > c - currentStreak; i--)
                    {
                        // potentially: check if this item was already marked on in column check, then give extra points!
                        // TODO: Ensure that a cross section doesn't have the middle value overwritten (e.g. 4 on column, 3 on row will set the value to 3)
                        // add to matchkeeper
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
    }

    // grid mechanics on match are implemented here
    public void RemoveMatches()
    {
        for(int c = 0; c < columns; c++)
        {
            List<int> notDestroyed = new List<int>();
            List<Vector3> positions = new List<Vector3>();
            // loop thru rows
            for (int r = 0; r < rows; r++)
            {
                positions.Add(components[c, r].transform.position);
                // If a match was not made with this block, preserve the component
                if (matchKeeper[c, r] == 0)
                {
                    notDestroyed.Add(r);
                }
                // A match was made; destroy the component
                else
                {
                    jars[(int)componentRefs[c, r].type].GetComponent<Jar>().AddComponent();
                    Destroy(components[c, r]);
                }
            }

            // move existing elements to top spots (upside down, so falling)
            // no chance of overlap, since things move up or stay in the same spots
            for(int i = 0; i < notDestroyed.Count; i++)
            {
                components[c, i] = components[c, notDestroyed[i]];
                components[c, i].transform.position = positions[i];
                componentRefs[c, i] = componentRefs[c, notDestroyed[i]];
            }

            // replace the rest with random objects
            for(int j = notDestroyed.Count; j < rows; j++)
            {
                //Debug.Log($"{c}, {j}");
                // TODO: put this code inside CreateMatchComponent method
                components[c, j] = Instantiate(
                    itemPrefab, // prefab
                    positions[j], // position
                    Quaternion.identity, //rotation
                    gameObject.transform // parent
                    );

                componentRefs[c, j] = components[c, j].GetComponent<MatchComponent>();

                // setup back reference to grid
                componentRefs[c, j].gridRef = this;

                // Select a random component type based off the length of the Component enum
                Component tempType = (Component)Random.Range(0, System.Enum.GetNames(typeof(Component)).Length);
                componentRefs[c, j].type = tempType;
                components[c, j].GetComponent<Renderer>().material.SetColor("_Color", colors[(int)tempType]);
            }
        }

        matchKeeper = new int[columns, rows];

        HandleMatches();
    }

    public void HandleMatches()
    {
        CheckAllRowsAndColumns();
        if (recheckFlag)
            RemoveMatches();
    }
}
