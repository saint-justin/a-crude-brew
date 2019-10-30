using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveOrderTracker : MonoBehaviour
{
    // -------------------------------------------------------
    // Component attached to the order's visualized gameobject
    // -------------------------------------------------------

    public OrderInfo orderInfo;            // Contains all the necessary info about the order
    int orderNumber;

    public List<GameObject> orderIcons;    // Populated w/ the three order icons
    public GameObject orderText;           // Populated w/ the name of the order


    public OrderManager orderManager;      // Reference to the existing parent object's orderManager script 
    public ScoreSystem scoreRef;

    public GameObject parentTimer;         // Populated w/ the parent progress bar object
    private RectTransform innerTimer;           // Populated w/ the inner time component
    private float initialWidth;
    private float orderLength = 10.0f;
    private float timeElapsed;

    public Texture failedOrderTexture;
    public List<RawImage> failedOrderIcons;

    // Start is called before the first frame update
    void Start()
    {
        InitializeVariables();

        UpdateOrderIcons(orderInfo);

        UpdatePosition(orderManager.currentOrders);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
    }

    /// <summary>
    /// Initializes all variables
    /// NOTE: Only use in the Start fxn
    /// </summary>
    void InitializeVariables()
    {
        // Get a reference to the info for this order
        orderInfo = gameObject.GetComponent<OrderInfo>();

        // List of all the child components
        List<GameObject> children = new List<GameObject>();

        // Get a list of all the children of this object then distribute them to their given objects
        foreach (Transform child in gameObject.transform)
        {
            children.Add(child.gameObject);
        }
        orderIcons = new List<GameObject>() { children[1], children[2], children[3] };
        parentTimer = children[4];
        orderText = children[0];
        orderText.GetComponent<Text>().text = orderInfo.GetOrderName();

        // Set the timer up
        innerTimer = parentTimer.transform.GetChild(0).gameObject.GetComponent<RawImage>().GetComponent<RectTransform>();
        initialWidth = innerTimer.rect.width;
        timeElapsed = 0.0f;

        // Get reference to the OrderManager object
        orderManager = gameObject.transform.parent.gameObject.GetComponent<OrderManager>();

        // Set up for order failure
        failedOrderIcons = orderManager.failedOrderIcons;
        failedOrderTexture = orderManager.failedOrderTexture;
    }

    /// <summary>
    /// Updates the order icons in this order
    /// </summary>
    /// <param name="_orderInfo"></param>
    void UpdateOrderIcons(OrderInfo _orderInfo)
    {
        int currentSlot = 0;    // Refers to which of the three icon slots is being modified
        int[] orderComponentAmounts = _orderInfo.GetOrderComponents();

        // Cycle through all the different abilities and populate icons based on which are needed
        for (int i = 0; i < 6; i++)
        {
            // If there's an amount of components needed, add the icon to it
            if (orderComponentAmounts[i] > 0)
            {
                orderIcons[2 - currentSlot].GetComponent<RawImage>().texture = orderManager.icons[i];
                orderIcons[2 - currentSlot].transform.GetChild(0).GetComponent<Text>().text = orderComponentAmounts[i].ToString();
                currentSlot++;
            }

            // We've already filled all the slots, don't bother cycling through the others
            if (currentSlot >= 3)
            {
                return;
            }
        }

        // If we went through all the order components and there were less than 3 needed, disable the other slots so they don't display
        if (currentSlot >= 2)
        {
            while(currentSlot < 3)
            {
                orderIcons[2 - currentSlot].SetActive(false);
                currentSlot++;
            }
        }
    }

    /// <summary>
    /// Updates the local position of this item's gameObject based on which spot it's in
    /// </summary>
    /// <param name="orderNo"></param>
    void UpdatePosition(int _orderNumber)
    {
        this.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (_orderNumber * 20), transform.localPosition.z);
        orderManager.currentOrders++;
    }

    /// <summary>
    /// Remove this order from the list when it's fully filled
    /// </summary>
    public void OrderFilled(int[] _components)
    {
        // sum up total components
        int sum = 0;
        for (int i = 0; i < _components.Length; i++)
        {
            sum += _components[i];
        }

        // If initialized in Start(), the reference to Score.GetComponent<ScoreSystem>() is lost
        if (scoreRef == null)
        {
            scoreRef = orderManager.GetScoreRef();
        }

        // multiply by 25 and add score
        scoreRef.AddScore(sum * 25);

        // Mark the order as done
        OrderComplete();
    } 

    /// <summary>
    /// Update the timer based on how long it's been in deltatime since the last update
    /// </summary>
    private void UpdateTimer()
    {
        timeElapsed += Time.deltaTime;
        if (orderLength > timeElapsed)
        {
            parentTimer.transform.GetChild(0).localScale = new Vector3(Mathf.Lerp(0.0f, 1.0f, (orderLength - timeElapsed) / orderLength), 0.5f, 1.0f);
        }
        else
        {
            OrderFailed();
        }
    }

    /// <summary>
    /// Function that fires when a given order is not completed within it's timeframe;
    /// </summary>
    private void OrderFailed()
    {
        failedOrderIcons[orderManager.failedOrders].texture = failedOrderTexture;
        orderManager.failedOrders++; 

        OrderComplete();
    }

    /// <summary>
    /// Call this to destroy the order w/ fancy VFX
    /// </summary>
    private void OrderComplete()
    {
        //TODO: Add some fancy VFX in the future here
        Destroy(gameObject);
    }
}
