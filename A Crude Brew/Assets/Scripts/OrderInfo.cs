using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderInfo : MonoBehaviour
{
    // ---------------------------------------------------
    // Container object to hold the data for a given order
    // ---------------------------------------------------

    // Basic information for the order to hold
    public string orderName = "";

    //Denotes how many of each given item the order requires
    public int componentRaindrops = 0;
    public int componentTeeth = 0;
    public int componentVials = 0;
    public int componentFeathers = 0;
    public int componentHorns = 0;
    public int componentYarn = 0;

    /// <summary>
    /// Gets a list of integers that contain the amount of each given component needed to fill the order
    /// </summary>
    /// <returns>
    /// ORDER: Raindrops, Teeth, Vials, Feathers, Horns, Yarn
    /// </returns>
    public int[] GetOrderComponents()
    {
        return new int[] {
            componentRaindrops,
            componentTeeth,
            componentVials,
            componentFeathers,
            componentHorns,
            componentYarn
        };
    }

    /// <summary>
    /// Returns the name of the given order
    /// </summary>
    /// <returns></returns>
    public string GetOrderName()
    {
        return orderName;
    }

    /// <summary>
    /// Populates the order based on the given set of components that have been passed in
    /// </summary>
    /// <param name="_orderName">The name of the order</param>
    /// <param name="_components">The amount of each component of the order-- ORDER: Raindrops, Teeth, Vials, Feathers, Horns, Yarn</param>
    public void SetOrderInfo(string _orderName, string _components)
    {
        // Set the order name in
        orderName = _orderName;

        // Parse out the order components from the components string and place them into their containers
        char[] componentSets = _components.ToCharArray();
        for (int i = 0; i < componentSets.Length; i += 2)
        {
            switch (componentSets[i])
            {
                // Raindrops
                case 'r':
                    int.TryParse(componentSets[i + 1].ToString(), out componentRaindrops);
                    break;

                // Teeth
                case 't':
                    int.TryParse(componentSets[i + 1].ToString(), out componentTeeth);
                    break;

                // Vials
                case 'v':
                    int.TryParse(componentSets[i + 1].ToString(), out componentVials);
                    break;

                // Feathers
                case 'f':
                    int.TryParse(componentSets[i + 1].ToString(), out componentFeathers);
                    break;

                // Horns
                case 'h':
                    int.TryParse(componentSets[i + 1].ToString(), out componentHorns);
                    break;

                // Yarn
                case 'y':
                    int.TryParse(componentSets[i + 1].ToString(), out componentYarn);
                    break;

                default:
                    Debug.LogError("Unrecognized component type in file for component: " + orderName);
                    break;
            }
        }
    }
}
