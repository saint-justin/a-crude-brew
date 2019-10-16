﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{

    private Vector3 mouseOffset; // offset from center of object to mouse cursor position

    private Vector3 currentHardPosition; // position of object before picked up by mouse

    public string type;

    public ComponentGrid gridRef;

    private bool drawLines = false;

    private Rect columnBounds, rowBounds;

    // Start is called before the first frame update
    void Start()
    {

        // currentHardPosition variable needed for column and row bounds method
        currentHardPosition = transform.position;

        CalcColumnAndRowBounds();

    }

    // Update is called once per frame
    void Update()
    {

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

    private void OnMouseDown()
    {
        drawLines = true;
        Vector3 pos = Input.mousePosition;
        pos.z = transform.position.z + transform.parent.position.z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(pos);
        mouseWorld.z = transform.position.z;

        mouseOffset = mouseWorld - transform.position;
        currentHardPosition = transform.position;

        // calc column and row bounds here
        CalcColumnAndRowBounds();
    }

    private void OnMouseDrag()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = transform.position.z + transform.parent.position.z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(pos);
        mouseWorld.z = transform.position.z;
        transform.position = mouseWorld - mouseOffset;
    }

    private void OnMouseUp()
    {
        drawLines = false;

        //Debug.Log(columnBounds);
        //Debug.Log(currentHardPosition);
        //Debug.Log(transform.position);

        // first check: see if object is in its row or column, otherwise return
        if (!columnBounds.Contains(transform.position) && !rowBounds.Contains(transform.position))
        {
            transform.position = currentHardPosition;
            return;
        }

        // if obj is w/in BOTH row & column bounds, it hasn't moved... so return it to its home position
        if (columnBounds.Contains(transform.position) && rowBounds.Contains(transform.position))
        {
            transform.position = currentHardPosition;
            return;
        }

        // if swap doesn't work, revert object back to its original position
        if (!gridRef.CheckSwap(currentHardPosition, transform.position))
        {
            transform.position = currentHardPosition;
            return;
        }
    }

    private void CalcColumnAndRowBounds()
    {
        // bounds are based on padding as well
        float padding = gridRef.padding;

        columnBounds = new Rect(
            currentHardPosition.x - 0.5f - (padding / 2.0f),
            gridRef.transform.position.y - 0.5f - (padding / 2.0f),
            1.0f + padding,
            gridRef.rows * (1.0f + padding)
            );
        rowBounds = new Rect(
            gridRef.transform.position.x - 0.5f - (padding / 2.0f),
            currentHardPosition.y - 0.5f - (padding / 2.0f),
            gridRef.columns * (1.0f + padding),
            1.0f + padding
            );
    }
}
