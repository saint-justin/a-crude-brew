using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jar : MonoBehaviour
{
    private int maxCapacity = 20;
    public int currentCapacity = 0;
    private float currentCapacityLerp = 0;
    private CauldronManager cauldron;
    private int[] modifier;
    public int jarIndex;
    public AudioClip onJarClick;

    // Start is called before the first frame update
    void Start()
    {
        currentCapacity = 0;
        currentCapacityLerp = 0;
        CalcLiquidHeight();
        cauldron = GameObject.Find("CauldronActives").GetComponent<CauldronManager>();
        modifier = new int[6];
        modifier[jarIndex] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentCapacityLerp != currentCapacity)
        {
            CalcLiquidHeight();
        }
    }

    /// <summary>
    /// Add components into the jar, provided it is not at max capacity
    /// </summary>
    /// <param name="amount">Number of components to place into the jar</param>
    public void AddComponent(int amount = 1)
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
    public bool RemoveComponent(int amount = 1)
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
    /// Moves a component from the jar to the cauldron if there's at least one component in the jar
    /// </summary>
    public void OnMouseDown()
    {
        // Check if there's anything in the jar being clicked
        if (RemoveComponent())
        {
            GameObject.Find("AudioManager").GetComponent<AudioSource>().PlayOneShot(onJarClick);
            cauldron.GetComponent<CauldronManager>().AddItems(modifier);
        }
    }

    /// <summary>
    /// Scales the height of the liquid to its current capacity divided by its max capacity
    /// </summary>
    private void CalcLiquidHeight()
    {
        if(currentCapacityLerp - currentCapacity < 0.005f && currentCapacityLerp - currentCapacity > -0.005f)
        {
            currentCapacityLerp = currentCapacity;
        }
        else if(currentCapacityLerp < currentCapacity)
        {
            currentCapacityLerp = Mathf.Lerp(currentCapacityLerp, currentCapacity, 0.02f);
        }
        else
        {
            currentCapacityLerp = Mathf.Lerp(currentCapacityLerp, currentCapacity, 0.10f);
        }
        transform.GetChild(0).localScale = new Vector3(1, 0.01f + 0.99f * currentCapacityLerp / maxCapacity, 1);
    }

    /// <summary>
    /// Clears all contents from the jar and resets its fluid height
    /// </summary>
    public void EmptyJar()
    {
        currentCapacity = 0;
        currentCapacityLerp = 0;
        transform.GetChild(0).localScale = new Vector3(1, 0.01f, 1);
    }
}
