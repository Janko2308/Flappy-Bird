using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameHandler gameHandler;
    private Vector3 direction;
    public float gravity = -10f;
    public float strength = 5f;
    public float tilt = 5f;

    private SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    private int index;
    public int maxIndex = 3;



    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(SpriteChange), 0.1f, 0.1f);
        gameHandler = FindObjectOfType<GameHandler>();
        //rotate bird by 0 -90 -90
        //transform.eulerAngles = new Vector3(0, -90, -90);
        //transform.eulerAngles = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // // if (Input.GetKeyDown(KeyCode.Space))
        // // {
        // //     direction = Vector3.up * strength;
        // // }

        // // direction.y += gravity * Time.deltaTime;
        // // transform.position += direction * Time.deltaTime;
        // // Vector3 rotation = transform.eulerAngles;
        // // rotation.z = direction.y * tilt;
        // // transform.eulerAngles = rotation;

        // Vector3 clampedPosition = transform.position;
        // //clampedPosition.y = Mathf.Clamp(clampedPosition.y, 0f, 5f);
        // transform.position = clampedPosition;
    }

    private void SpriteChange()
    {
        index = (index + 1) % maxIndex;

        spriteRenderer.sprite = sprites[index];
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Pipe")) {
    //         Debug.Log("Collision detected");
    //         AddReward(-1f);


    //     } 
    //     else if (other.gameObject.CompareTag("PointZone")) {
    //         Debug.Log("Point scored");
    //         gameHandler.AddPoint();

    //     }
    // }
}
