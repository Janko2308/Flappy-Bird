using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHandler : MonoBehaviour
{
    public GameObject pipePrefab; 
    public float spawnInterval = 1.5f; 
    public float pipeSpeed = 5f; 
    public float minHeight = -1f; 
    public float maxHeight = 1f; 

    public TextMeshProUGUI score;
    public int points = 0;
    private Vector3 spawnPosition = new Vector3(8, 0, 0); 

    void Start()
    {
        Debug.Log("GameHandler was started");
        InvokeRepeating(nameof(SpawnPipe), 2.0f, spawnInterval);
        score.text = "0";
    }

    public void AddPoint()
    {
        points++;
        score.text = points.ToString();
        Debug.Log("Score: " + score.text);
    }


    public void ResetGame()
    {
        // Reset points and UI
        points = 0;
        score.text = "0";

        // Destroy all existing pipes
        foreach (GameObject pipe in GameObject.FindGameObjectsWithTag("Pipe"))
        {
            Destroy(pipe);
        }
    }

    public GameObject GetClosestPipe(float birdX)
    {
        GameObject closestPipe = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject pipe in GameObject.FindGameObjectsWithTag("Pipe"))
        {
            float distance = pipe.transform.position.x - birdX;
            if (distance > 0 && distance < closestDistance)
            {
                closestPipe = pipe;
                closestDistance = distance;
            }
        }

        return closestPipe;
    }

    public GameObject GetPointZone(float birdX)
    {
    GameObject closestPointZone = null;
    float closestDistance = float.MaxValue;

    foreach (GameObject pointZone in GameObject.FindGameObjectsWithTag("PointZone"))
    {
        float distance = pointZone.transform.position.x - birdX;
        if (distance > 0 && distance < closestDistance)
        {
            closestPointZone = pointZone;
            closestDistance = distance;
        }
    }

    return closestPointZone;
    }
    void SpawnPipe()
    {
        float height = Random.Range(minHeight, maxHeight);

        GameObject pipe = Instantiate(pipePrefab, spawnPosition + new Vector3(0, height, 0), Quaternion.identity);

        pipe.AddComponent<PipeMover>().speed = pipeSpeed;
    }
}

public class PipeMover : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < -13f)
        {
            Destroy(gameObject); 
        }
    }
}