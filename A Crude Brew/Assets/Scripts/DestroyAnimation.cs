using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAnimation : MonoBehaviour
{
    public Sprite[] spriteSheet;
    public int framesPerSprite = 2;
    private int index;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        gameObject.GetComponent<SpriteRenderer>().sprite = spriteSheet[0];
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = spriteSheet[(index / framesPerSprite)];
        index++;
        if(index >= spriteSheet.Length * framesPerSprite)
        {
            Destroy(gameObject);
        }
    }
}
