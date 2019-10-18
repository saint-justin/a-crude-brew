using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JarManager : MonoBehaviour
{
    public Camera camera;
    public GameObject cauldron;

    // The UI Element overlay jars
    List<GameObject> jars;
    GameObject jarHorns;
    GameObject jarFeathers;
    GameObject jarRaindrops;
    GameObject jarTeeth;
    GameObject jarVials;
    GameObject jarYarn;

    List<int[]> modifiers;

    // The actual jars
    List<GameObject> trueJars;
    public GameObject trueJarHorns;
    public GameObject trueJarFeathers;
    public GameObject trueJarRaindrops;
    public GameObject trueJarTeeth;
    public GameObject trueJarVials;
    public GameObject trueJarYarn;

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to all children
        jarTeeth = this.transform.GetChild(0).gameObject;
        jarVials = this.transform.GetChild(1).gameObject;
        jarFeathers = this.transform.GetChild(2).gameObject;
        jarRaindrops = this.transform.GetChild(3).gameObject;
        jarYarn = this.transform.GetChild(4).gameObject;
        jarHorns = this.transform.GetChild(5).gameObject;

        // Populate the tracker list of jars
        jars = new List<GameObject>() { jarHorns, jarFeathers, jarRaindrops, jarTeeth, jarVials, jarYarn };
        trueJars = new List<GameObject>() { trueJarHorns, trueJarFeathers, trueJarRaindrops, trueJarTeeth, trueJarVials, trueJarYarn };

        // Make all the modifier int arrays needed
        modifiers = new List<int[]>()
        {
            new int[]{1, 0, 0, 0, 0, 0},
            new int[]{0, 1, 0, 0, 0, 0},
            new int[]{0, 0, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 0, 0},
            new int[]{0, 0, 0, 0, 1, 0},
            new int[]{0, 0, 0, 0, 0, 1}
        };
    }

    // Update is called once per frame
    void Update()
    {
        CheckForJarClick();
    }

    private void CheckForJarClick()
    {
        // Check up on the mouse whenever the player clicks
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // If the mouse position is over top of one of the jars, test for collision
            Vector3 mousePos = Input.mousePosition;
            int i = 0;
            foreach (GameObject jar in jars)
            {
                Vector2 rectPos = jar.GetComponent<Transform>().position;
                float halfWidth = jar.GetComponent<RectTransform>().rect.width / 2;
                float halfHeight = jar.GetComponent<RectTransform>().rect.height / 2;

                // Checking true collision
                if (mousePos.x < rectPos.x + halfWidth && mousePos.x > rectPos.x - halfWidth)
                {
                    if (mousePos.y < rectPos.y + halfHeight && mousePos.y > rectPos.y - halfHeight)
                    {
                        // Check if there's anything in the jar being clicked
                        if (trueJars[i].GetComponent<Jar>().RemoveComponent())
                        {
                            cauldron.GetComponent<CauldronManager>().AddItems(modifiers[i]);
                        }
                    }
                }

                // Increment incrementer
                i++;
            }
        }
    }
}
