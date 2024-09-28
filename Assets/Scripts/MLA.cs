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

    RaycastHit hit; // 3D RaycastHit

    void Start()
    {
        gameHandler = FindObjectOfType<GameHandler>();
        birdRigidbody = GetComponent<Rigidbody>();  // 3D Rigidbody
        m_Agent = GetComponent<Agent>();
        rayPerceptionSensor = GetComponent<RayPerceptionSensor>();
    }

    public override void Initialize()
    {
        // Assuming your child GameObject is named "RaySensorHolder"
        Transform raySensorHolder = transform.Find("RaySensorHolder");  // Find the child object
        RayPerceptionSensorComponent3D rayPerception = raySensorHolder.GetComponent<RayPerceptionSensorComponent3D>();
    }

    void Update()
    {
        // Gravity should act continuously to pull the bird down
        direction.y += gravity * Time.deltaTime;

        // Apply movement based on gravity and any previous flaps
        transform.position += direction * Time.deltaTime;

        Transform raySensorHolder = transform.Find("RaySensorHolder");
        RayPerceptionSensorComponent3D rayPerception = raySensorHolder.GetComponent<RayPerceptionSensorComponent3D>();

        if (rayPerception != null)
        {
            // Assuming the ray length and direction are set correctly in the editor
            Vector3 rayDirection = rayPerception.transform.forward;  // Direction of the ray
            Debug.DrawRay(rayPerception.transform.position, rayDirection * rayPerception.RayLength, Color.green);
        }

        // // 3D Raycast
        // if (Physics.Raycast(transform.position, Vector3.forward, out hit, 10f))  // Changed to Physics.Raycast for 3D
        // {
        //     // Visualize the ray in the Scene view
        //     Debug.DrawRay(transform.position, Vector3.forward * 10f, Color.green);  // Adjust for 3D visualization

        //     // Check if the bird hit a pipe or the ground
        //     if (hit.collider != null)
        //     {
        //         if (hit.collider.CompareTag("Pipe"))
        //         {
        //             Debug.Log("Pipe detected");
        //         }
        //         else if (hit.collider.CompareTag("Ground"))
        //         {
        //             Debug.Log("Ground detected");
        //         }
        //     }
        // }
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

        // Reset the bird's Rigidbody velocities
        birdRigidbody.velocity = Vector3.zero;        // Reset linear velocity
        birdRigidbody.angularVelocity = Vector3.zero; // Reset angular velocity (no spinning)
        direction = Vector3.zero;                     // Reset vertical movement

        // Reset the bird's position above the ground (e.g., y = 1)
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;      // Reset rotation to no tilt

        // Reset the game environment (e.g., pipes and score)
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
    
    // If Ray Perception Sensor is attached and valid, collect raycast observations
    if (rayPerception != null)
    {
        Debug.Log("Ray Perception Sensor is valid");
        

    }   
}



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get the discrete actions output
        var discreteActions = actionsOut.DiscreteActions;

        // Set the action based on player input (spacebar to flap)
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

        // Apply gravity continuously
        direction.y += gravity * Time.deltaTime;

        // If action == 1, apply a flap force upwards
        if (action == 1)
        {
            //birdRigidbody.AddForce(Vector2.up * strength, ForceMode2D.Impulse);
            direction = Vector3.up * strength;
            Debug.Log("Flap");

            // Small penalty for flapping to prevent unnecessary flaps
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
