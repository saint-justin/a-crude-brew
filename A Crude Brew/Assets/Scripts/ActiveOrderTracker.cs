using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveOrderTracker : MonoBehaviour
{
    private List<GameObject> orderIcons; // Populated w/ the three order icons
    private GameObject orderText;     // Populated w/ the name of the order
    private GameObject orderProgressBar;      // Populated w/ the parent progress bar object

    // Start is called before the first frame update
    void Start()
    {
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

        return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
