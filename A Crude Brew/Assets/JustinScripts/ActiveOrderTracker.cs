using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOrderTracker : MonoBehaviour
{
    public GameObject emptyOrder;   // Populated w/ a generic order

    private Texture2D[] orderIcons; // Populated w/ the three order icons
    private TextMesh orderText;     // Populated w/ the name of the order
    private Texture2D orderProgressBar;      // Populated w/ the parent progress bar object

    // Start is called before the first frame update
    void Start()
    {
        Texture2D[] allTex2D = emptyOrder.GetComponents<Texture2D>();
        orderIcons = new Texture2D[3] { allTex2D[1], allTex2D[2], allTex2D[3] };
        orderProgressBar = allTex2D[0];
        orderText = emptyOrder.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
