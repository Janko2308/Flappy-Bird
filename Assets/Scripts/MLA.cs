using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class MLA : Agent
{
    public GameHandler gameHandler;
    public Rigidbody birdRigidbody; // Note: Changed from Rigidbody2D to Rigidbody for 3D
    float m_TimeSinceDecision;
    public float timeBetweenDecisionsAtInference;

    public float gravity = -5f;  // Increase gravity for more realistic falling
    public float strength = 3f;   // Flap strength (adjust based on gravity)
    public float tilt = 5f;       // Tilt factor for rotation
    private Vector3 direction;    // Direction for movement
    Agent m_Agent;
    RayPerceptionSensor rayPerceptionSensor;


    void Start()
    {
        gameHandler = FindObjectOfType<GameHandler>();
        birdRigidbody = GetComponent<Rigidbody>(); 
        m_Agent = GetComponent<Agent>();
        rayPerceptionSensor = GetComponent<RayPerceptionSensor>();
    }

    public override void Initialize()
    {
        Transform raySensorHolder = transform.Find("RaySensorHolder");  
        RayPerceptionSensorComponent3D rayPerception = raySensorHolder.GetComponent<RayPerceptionSensorComponent3D>();
    }

    void Update()
    {
        direction.y += gravity * Time.deltaTime;

        transform.position += direction * Time.deltaTime;

        Transform raySensorHolder = transform.Find("RaySensorHolder");
        RayPerceptionSensorComponent3D rayPerception = raySensorHolder.GetComponent<RayPerceptionSensorComponent3D>();

        if (rayPerception != null)
        {
            //Debug
            Vector3 rayDirection = rayPerception.transform.forward;  
            Debug.DrawRay(rayPerception.transform.position, rayDirection * rayPerception.RayLength, Color.green);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Pipe"))
        {
            Debug.Log("Collision detected. Ending episode.");

            // Give a negative reward for hitting an obstacle or the ground
            AddReward(-1.0f);
            EndEpisode(); // End the current episode
        }
        else if (other.gameObject.CompareTag("PointZone"))
        {
            Debug.Log("Point scored.");

            // Reward the agent for successfully passing a pipe
            AddReward(1.0f);
            gameHandler.AddPoint();
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin");

        birdRigidbody.velocity = Vector3.zero;        
        birdRigidbody.angularVelocity = Vector3.zero; 
        direction = Vector3.zero;      

        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;      

        gameHandler.ResetGame();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    // Observe bird's y-position and velocity
    sensor.AddObservation(transform.position.y);
    sensor.AddObservation(birdRigidbody.velocity.y);

    // Reference the child Ray Perception Sensor component
    Transform raySensorHolder = transform.Find("RaySensorHolder");
    RayPerceptionSensorComponent3D rayPerception = raySensorHolder.GetComponent<RayPerceptionSensorComponent3D>();
    
    if (rayPerception != null)
    {
        Debug.Log("Ray Perception Sensor is valid");
        
    }   
}



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActions[0] = 1;  // Action 1 (flap)
        }
        else
        {
            discreteActions[0] = 0;  // Action 0 (do nothing)
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // The action: 0 = do nothing, 1 = flap
        int action = actions.DiscreteActions[0];
        Debug.Log("Action: " + action);

        direction.y += gravity * Time.deltaTime;

        // If action == 1, apply a flap force upwards
        if (action == 1)
        {
            direction = Vector3.up * strength;
            Debug.Log("Flap");

            //Small penalty to prevent excessive flapping
            AddReward(-0.01f);
        }

        // Apply movement based on gravity and flap
        transform.position += direction * Time.deltaTime;

        // Small reward for staying alive
        AddReward(0.01f);

        // End episode if bird flies out of bounds (below or above a set threshold)
        if (transform.position.y < -5f || transform.position.y > 5f)
        {
            AddReward(-1.0f);  // Moderate penalty for going out of bounds
            EndEpisode();
        }

        // Debug log reward
        Debug.Log("Reward: " + GetCumulativeReward());
    }


    public void WaitTimeInference()
    {
        Debug.Log("WaitTimeInference");
        m_TimeSinceDecision = Time.time;
        if (m_Agent == null)
        {
            return;
        }
        if (Academy.Instance.IsCommunicatorOn)
        {
            m_Agent?.RequestDecision();
        }
        else
        {
            if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                m_TimeSinceDecision = 0f;
                m_Agent?.RequestDecision();
            }
            else
            {
                m_TimeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
