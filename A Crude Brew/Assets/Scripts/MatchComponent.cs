using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchComponent : MonoBehaviour
{
    public Component type;
    public GameObject gridObject;
    public MatchGrid gridRef;

    private Vector3 mouseOffset; // offset from center of object to mouse cursor position
    public Vector3 currentHardPosition; // position of object before picked up by mouse
    public Vector3 currentObjectivePosition; // Position that the object should move towards
    private Vector3 mouseWorld; // Current stored location of the mouse on the grid

    private bool drawLines = false;
    private Rect columnBounds;
    private Rect rowBounds;

    public Vector2Int rowColumn;

    // Start is called before the first frame update
    void Start()
    {
        rowColumn = new Vector2Int(-1, -1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currentObjectivePosition;

        // for debug purposes
        // this will show up as offset due to transform having a parent
        if (drawLines)
        {
            Debug.DrawLine(
                new Vector3(columnBounds.min.x, columnBounds.min.y, transform.position.z),
                new Vector3(columnBounds.max.x, columnBounds.max.y, transform.position.z),
                Color.magenta
                );
            Debug.DrawLine(
                new Vector3(rowBounds.min.x, rowBounds.min.y, transform.position.z),
                new Vector3(rowBounds.max.x, rowBounds.max.y, transform.position.z),
                Color.green
                );
        }
    }

    /// <summary>
    /// Function call to set the values for row/column; this makes creating a MatchComponent[,] not create "new"
    /// </summary>
    /// <param name="gridGameObject">Reference to the Grid's gameObject</param>
    /// <param name="row">Indexed row</param>
    /// <param name="column">Indexed column</param>
    public void Initialize(GameObject gridGameObject, int row, int column)
    {

        gridObject = gridGameObject;
        gridRef = gridObject.GetComponent<MatchGrid>();
        rowColumn = new Vector2Int(row, column);

        // currentHardPosition variable needed for column and row bounds method
        SetLocation(rowColumn);

        CalcColumnAndRowBounds(rowColumn);
    }

    /// <summary>
    /// Sets the component type and sprite renderer to match the given type
    /// </summary>
    /// <param name="type">Component enum</param>
    public void SetComponentType(Component type)
    {
        this.type = type;
        // In case gridRef gives up, slap it back into submission
        if(gridRef == null)
        {
            gridObject = GameObject.Find("Grid");
            gridRef = gridObject.GetComponent<MatchGrid>();
        }
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = gridRef.GetSprite(type);
        // Set sprite equal to gridRef.GetSprite(type);
    }

    /// <summary>
    /// Picks up the component off the grid
    /// </summary>
    private void OnMouseDown()
    {
        drawLines = true;
        Vector3 mouseWorld = gridRef.GetMousePosition();
        mouseOffset = mouseWorld - transform.position;
        currentObjectivePosition = mouseWorld - mouseOffset;

        // CalcColumnAndRowBounds(rowColumn);
    }

    /// <summary>
    /// Moves the lifted piece to follow the mouse movement
    /// </summary>
    private void OnMouseDrag()
    {
        Vector3 mouseWorld = gridRef.GetMousePosition();
        currentObjectivePosition = mouseWorld - mouseOffset + new Vector3(0.0f, 0.0f, -0.001f);
        // gridRef.DragPiece(rowColumn, gridRef.WorldPosToIndex(mouseWorld - mouseOffset));
    }

    /// <summary>
    /// Swap the current piece if it's in the specified row or column and completes a match, else return to original position
    /// </summary>
    private void OnMouseUp()
    {
        drawLines = false;

        //Debug.Log(columnBounds);
        //Debug.Log(currentHardPosition);
        //Debug.Log(transform.position);

        // TODO: Implement this as a parameter for CheckSwap
        Vector2Int newRowColumn = gridRef.WorldPosToIndex(currentObjectivePosition);
        Debug.Log("newRowColumn.x = " + newRowColumn.x + ", newRowColumn.y = " + newRowColumn.y);

        // if swap doesn't work, revert object back to its original position
        gridRef.CheckSwap(currentHardPosition, transform.position);
    }

    /// <summary>
    /// Sets the bounding rectangles for the row and column that contains the component
    /// </summary>
    private void CalcColumnAndRowBounds(Vector2Int _rowColumn)
    {
        // bounds are based on padding as well
        Vector3 gridScale = gridObject.transform.localScale;
        Vector3 gridPos = gridObject.transform.position;
        float padding = gridRef.padding;
        int row = _rowColumn.x;
        int column = _rowColumn.y;

        columnBounds = new Rect(
            gridPos.x + gridScale.x * (column * (padding + 1.0f)),
            gridPos.y,
            gridScale.x * ((column + 1) * (padding + 1.0f)),
            gridScale.y * (gridRef.rows * (padding + 1.0f))
            );
        rowBounds = new Rect(
            gridPos.x,
            gridPos.y + gridScale.y * (row * (padding + 1.0f)),
            gridScale.x * (gridRef.columns * (padding + 1.0f)),
            gridScale.y * ((row+1) * (padding + 1.0f))
            );
    }

    /// <summary>
    /// Sets the currentHardPosition, currentObjectivePosition, and transform.position at the given row/column
    /// </summary>
    public void SetLocation(Vector2Int _rowColumn)
    {
        // TODO: Overhaul the MatchComponents to include a row/column reference instead of being based on the fucking transform.position
        rowColumn = _rowColumn;
        if (gridRef == null)
        {
            gridObject = GameObject.Find("Grid");
            gridRef = gridObject.GetComponent<MatchGrid>();
        }
        currentHardPosition = gridRef.IndexToWorldPos(rowColumn);
        currentObjectivePosition = currentHardPosition;
        transform.position = currentObjectivePosition;
    }

    /// <summary>
    /// Set the objective location to the indexed row/column
    /// </summary>
    /// <param name="_rowColumn">Vector2Int(row, column)</param>
    public void SetObjectiveLocation(Vector2Int _rowColumn)
    {
        currentObjectivePosition = gridRef.IndexToWorldPos(_rowColumn);
    }

    /// <summary>
    /// (Default) Set the objective location to its hard location
    /// </summary>
    public void SetObjectiveLocation()
    {
        currentObjectivePosition = currentHardPosition;
    }
}
