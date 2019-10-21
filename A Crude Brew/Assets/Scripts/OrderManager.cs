using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine; 

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

    
    // Start is called before the first frame update
    void Start()
    {
        // Populates the 'orders' list based on all the input text file
        ReadInOrders(@"Assets\Scripts\OrderTextFiles\order_inputs.txt");

        // Setup score vars
        scoreRef = GameObject.Find("Score").GetComponent<ScoreSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        string[] textOrders = fullText.Split('\r');

        // Split each of those seperate lines into their order name and components   
        Stack<string> orderNames = new Stack<string>();
        Stack<string> orderComponents = new Stack<string>();

        // Cycle through all entries and split them off into their own respective orders
        foreach (string currentOrder in textOrders)
        {
            // Create a new order instance to populate
            GameObject newOrder = Instantiate(emptyOrder, gameObject.transform);

            // Split the order into name and components
            string[] orderSplit = currentOrder.Split('#');

            // Push the order info into the order object
            newOrder.GetComponent<OrderInfo>().SetOrderInfo(orderSplit[0], orderSplit[1]);

            // Add this to the list of orders
            orders.Add(newOrder);
        }
    }
}
