using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchComponent : MonoBehaviour
{
    public GameObject gridObject;
    public MatchGrid gridRef;

    private Vector3 mouseOffset; // offset from center of object to mouse cursor position
    public Vector3 currentHardPosition; // position of object before picked up by mouse
    public Vector3 currentObjectivePosition; // Position that the object should move towards
    public Vector2Int columnRow;
    public Component type;

    private bool initialized = false;


    // Start is called before the first frame update
    void Start()
    {
        // If Initialize(...) hasn't already been called, flag the columnRow as (-1, -1)
        if(!initialized)
        {
            columnRow = new Vector2Int(-1, -1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currentObjectivePosition;
    }

    /// <summary>
    /// Function call to set the values for column/row; this makes creating a MatchComponent[,] not create "new" components
    /// </summary>
    /// <param name="gridGameObject">Reference to the Grid's gameObject</param>
    /// <param name="columnRow">new Vector2Int(column, row)</param>
    public void Initialize(GameObject gridGameObject, Vector2Int columnRow)
    {

        gridObject = gridGameObject;
        gridRef = gridObject.GetComponent<MatchGrid>();
        this.columnRow = columnRow;
        initialized = true;

        // currentHardPosition variable needed for column and row bounds method
        SetLocation(columnRow);
    }

    /// <summary>
    /// Sets the component type and sprite renderer to match the assigned type
    /// </summary>
    /// <param name="type">Component enum</param>
    public void SetComponentType(Component type)
    {
        this.type = type;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = gridRef.GetSprite(type);
    }

    /// <summary>
    /// Picks up the component off the grid
    /// </summary>
    private void OnMouseDown()
    {
        Vector3 mouseWorld = gridRef.GetMousePosition();
        mouseOffset = mouseWorld - transform.position;
        currentObjectivePosition = mouseWorld - mouseOffset;
    }

    /// <summary>
    /// Moves the lifted piece to follow the mouse movement
    /// </summary>
    private void OnMouseDrag()
    {
        Vector3 mouseWorld = gridRef.GetMousePosition();
        currentObjectivePosition = mouseWorld - mouseOffset + new Vector3(0.0f, 0.0f, -0.001f);
        gridRef.DragAdjacentPieces(columnRow, gridRef.WorldPosToIndex(mouseWorld - mouseOffset));
    }

    /// <summary>
    /// Swap the current piece if it's in the specified row or column and completes a match, else return to original position
    /// </summary>
    private void OnMouseUp()
    {
        Vector2Int newColumnRow = gridRef.WorldPosToIndex(currentObjectivePosition);
        // If swap doesn't work, revert object back to its original position
        gridRef.CheckSwap(gridRef.WorldPosToIndex(currentHardPosition), newColumnRow);
    }

    /// <summary>
    /// Sets the currentHardPosition, currentObjectivePosition, and transform.position at the given column/row
    /// </summary>
    /// <param name="columnRow">new Vector2Int(column, row)</param>
    public void SetLocation(Vector2Int columnRow)
    {
        this.columnRow = columnRow;
        currentHardPosition = gridRef.IndexToWorldPos(columnRow);
        currentObjectivePosition = currentHardPosition;
        transform.position = currentObjectivePosition;
    }

    /// <summary>
    /// Set the objective location to the indexed column/row
    /// </summary>
    /// <param name="_columnRow">Vector2Int(column, row)</param>
    public void SetObjectiveLocation(Vector2Int _columnRow)
    {
        currentObjectivePosition = gridRef.IndexToWorldPos(_columnRow);
    }

    /// <summary>
    /// (Default) Set the objective location back to its original location on the grid
    /// </summary>
    public void SetObjectiveLocation()
    {
        currentObjectivePosition = currentHardPosition;
    }
}
