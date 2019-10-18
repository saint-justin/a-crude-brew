using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CauldronManager : MonoBehaviour
{
    // Counter categories to keep track of how much of a given thing exists in the cauldron
    List<int> CounterComponents;

    // Stores all the base gui components
    List<GameObject> GuiComponents;
    GameObject orderSheet;

    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
            AddItems(new int[6] { -1, -1, -1, -1, -1, -1 });

        if (Input.GetKeyDown(KeyCode.RightBracket))
            AddItems(new int[6] { 1, 1, 1, 1, 1, 1 });

        CheckForFilledOrders();
    }

    /// <summary>
    /// Initializes all components
    /// NOTE -- Only use in the start function
    /// </summary>
    void InitializeComponents()
    {
        // Add all the components to the components list
        CounterComponents = new List<int>() { 0, 0, 0, 0, 0, 0 };

        // Initialize GuiComponents and populate it w/ the 6 icons
        GuiComponents = new List<GameObject>();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GuiComponents.Add(gameObject.transform.GetChild(i).gameObject);
        }

        // Get a reference to the order sheet
        orderSheet = this.transform.parent.GetChild(0).gameObject;
    }

    /// <summary>
    /// Publically accessible function to pass items into the cauldron
    /// </summary>
    /// <param name="_itemArr"></param>
    public void AddItems(int[] _itemArr)
    {
        // Update all the active component counts in the cauldron
        for (int i = 0; i < CounterComponents.Count; i++)
        {
            CounterComponents[i] += _itemArr[i];
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Updates the counts visually on the GUI based on the components here
    /// </summary>
    private void UpdateVisuals()
    {
        for(int i=0; i < GuiComponents.Count; i++)
        {
            GuiComponents[i].transform.GetChild(0).GetComponent<Text>().text = CounterComponents[i].ToString();
        }
    }

    /// <summary>
    /// Empties out the cauldron
    /// </summary>
    private void EmptyCauldron()
    {
        for (int i = 0; i < CounterComponents.Count; i++)
        {
            CounterComponents[i] = 0;
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Checks the orders for matches to the current order
    /// </summary>
    public void CheckForFilledOrders()
    {
        // Get all the active orders and populate the list
        for (int i = 0; i < orderSheet.transform.childCount; i++)
        {
            // Get reference to the items in the active order
            int[] orderItems = orderSheet.transform.GetChild(i).gameObject.GetComponent<OrderInfo>().GetOrderComponents();
            bool exactSet = true;

            // Check if the active order's components match exactly the cauldron's components
            for (int j = 0; j < orderItems.Length; j++)
            {
                if (orderItems[j] != CounterComponents[j])
                    exactSet = false;
            }

            // If the order being checked matches, mark it as filled 
            if (exactSet)
            {
                // Clear all contents from the cauldron and mark the order as filled
                orderSheet.transform.GetChild(i).gameObject.GetComponent<ActiveOrderTracker>().OrderFilled();
                EmptyCauldron();
            }
        }
    }
}
