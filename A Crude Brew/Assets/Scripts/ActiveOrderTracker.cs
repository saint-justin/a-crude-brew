using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveOrderTracker : MonoBehaviour
{
    // -------------------------------------------------------
    // Component attached to the order's visualized gameobject
    // -------------------------------------------------------

    private OrderInfo orderInfo;            // Contains all the necessary info about the order

    private List<GameObject> orderIcons;    // Populated w/ the three order icons
    private GameObject orderText;           // Populated w/ the name of the order
    private GameObject orderProgressBar;    // Populated w/ the parent progress bar object

    private OrderManager orderManager;      // Reference to the existing parent object's orderManager script 

    // Start is called before the first frame update
    void Start()
    {
        // List of all the child components
        List<GameObject> children = new List<GameObject>();

        // Get a list of all the children of this object then distribute them to their given objects
        foreach (Transform child in gameObject.transform)
        {
            children.Add(child.gameObject);
        }
        orderIcons = new List<GameObject>(){ children[1], children[2], children[3] };
        orderProgressBar = children[0];
        orderText = children[4];
        orderText.GetComponent<Text>().text = "Default Text";

        // Get a reference to the info for this order & update the visuals accordingly
        orderInfo = gameObject.GetComponent<OrderInfo>();
        UpdateOrderIcons(orderInfo);
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Add in the progress bar ticking away and possible updating w/ green for sections that have been met from the current brew
    }

    void UpdateOrderIcons(OrderInfo _orderInfo)
    {
        int[] orderComponentAmounts = _orderInfo.GetOrderComponents();
        for (int i = 0; i < orderComponentAmounts.Length; i++)
        {
            if (orderComponentAmounts[i] > 0)
            {
                orderIcons[i].GetComponent<RawImage>().texture = GetComponentInParent<OrderManager>().icons[i];
            }
        }
    }
}
