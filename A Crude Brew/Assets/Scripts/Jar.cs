using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jar : MonoBehaviour
{
    private int maxCapacity = 20;
    public int currentCapacity = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentCapacity = 0;
        CalcLiquidHeight();
    }

    // Update is called once per frame
    void Update()
    {
        CalcLiquidHeight();
    }

    /// <summary>
    /// Add components into the jar, provided it is not at max capacity
    /// </summary>
    /// <param name="amount">Number of components to place into the jar</param>
    public void AddComponents(int amount)
    {
        currentCapacity += amount;
        if (currentCapacity > maxCapacity)
            currentCapacity = maxCapacity;
        CalcLiquidHeight();
    }

    /// <summary>
    /// Remove a component from the jar
    /// </summary>
    /// <param name="amount">Default to removing one item, please do not change</param>
    public bool RemoveComponents(int amount = 1)
    {
        if (currentCapacity >= amount)
        {
            currentCapacity -= amount;
            CalcLiquidHeight();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Scales the height of the liquid to its current capacity divided by its max capacity
    /// </summary>
    private void CalcLiquidHeight()
    {
        transform.GetChild(0).localScale = new Vector3(1, 0.01f + 0.99f * currentCapacity / maxCapacity, 1);
    }
}
