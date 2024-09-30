using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameHandler gameHandler;
    private SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    private int index;
    public int maxIndex = 3;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        InvokeRepeating(nameof(SpriteChange), 0.1f, 0.1f);
        gameHandler = FindObjectOfType<GameHandler>();
    }
    
    private void SpriteChange()
    {
        index = (index + 1) % maxIndex;

        spriteRenderer.sprite = sprites[index];
    }
}
