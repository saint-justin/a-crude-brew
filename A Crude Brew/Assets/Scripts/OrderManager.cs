using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    // Icons
    public List<Texture2D> icons;

    // Orders
    public int currentOrders = 0;
    public GameObject emptyOrder;
    List<GameObject> orders = new List<GameObject>();

    // Score-keeping
    public GameObject scoreObj;
    private ScoreSystem scoreRef;

    // Failed order tracking
    public List<RawImage> failedOrderIcons;
    public Texture failedOrderTexture;
    public int failedOrders = 0;

    // Order spawning
    private float timeToNextSpawn = 8.0f;
    private int orderIncrementer = 0;
    string[] textOrders;

    public AudioClip onOrderFill;

    // Start is called before the first frame update
    void Start()
    {
        // Populates the 'orders' list based on all the input text file
        ReadInOrders(@"Assets\Scripts\OrderTextFiles\order_inputs.txt");

        // Setup score vars
        scoreRef = scoreObj.GetComponent<ScoreSystem>();

        // Spawn the first order
        SpawnOrder();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForNextOrder();
    }

    // allows access to the score system from individual orders, so they can add to the score
    public ScoreSystem GetScoreRef()
    {
        return scoreRef;
    }

    // Reads in all the orders from the text file that holds them
    void ReadInOrders(string _filePath)
    {
        // Read in all of the text as a single string
        string fullText = File.ReadAllText(_filePath);

        // Split that single string into all the different lines
        textOrders = fullText.Split('\r');
    }

    /// <summary>
    /// Checks for the next order being spawned
    /// </summary>
    private void CheckForNextOrder()
    {
        timeToNextSpawn -= Time.deltaTime;

        if (timeToNextSpawn <= 0)
        {
            SpawnOrder();
            timeToNextSpawn = 8.0f;
        }
    }

    /// <summary>
    /// Spawns a given order into the game
    /// </summary>
    private void SpawnOrder()
    {
        // Create a new order instance to populate
        GameObject newOrder = Instantiate(emptyOrder, gameObject.transform);

        // Split the order into name and components
        string[] orderSplit = textOrders[orderIncrementer].Split('#');

        // Remove /n from lines following the first one
        if (orderSplit[0].Contains("\n"))
        {
            orderSplit[0] = orderSplit[0].Remove(0, 1);
        }

        // Push the order info into the order object
        newOrder.GetComponent<OrderInfo>().SetOrderInfo(orderSplit[0], orderSplit[1]);
        newOrder.GetComponent<ActiveOrderTracker>().scoreRef = scoreRef;

        // Add this to the list of orders
        orders.Add(newOrder);

        orderIncrementer++;
    }

    /// <summary>
    /// Function to call from active orders marking them as completed
    /// </summary>
    /// <param name="_index"></param>
    public void RemoveOrder(int _index)
    {
        //orders.RemoveAt(_index);

        int i = _index;

        while (i < orders.Count - 1)
        {
            orders[i + 1] = orders[i];
            orders[i].GetComponent<ActiveOrderTracker>().MoveUp();
            i++;
        }

        orders.RemoveAt(orders.Count - 1);
    }
}
