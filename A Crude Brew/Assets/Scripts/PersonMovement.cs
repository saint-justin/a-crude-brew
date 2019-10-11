// Author: Kevin Kulp and Peter Vitello
// Date: 10/10/2019
// Purpose: ceate a wandering method that allows persons to walk up to the shop and leave or wander left to right 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonMovement : MonoBehaviour
{

    private Vector3 wanderLeft;
    private Vector3 wanderRight;
    private Vector3[] orderLocations = new Vector3[3];
    public Vector3 position;
    public Vector3 objective;
    public float velocity;
    public float timer;
    public enum FiniteState { Inactive, Wandering, WalkingToOrder, WaitingForOrder, Leaving };
    public FiniteState movementState = FiniteState.Inactive;

    // Start is called before the first frame update
    void Start()
    {
        wanderLeft = GameObject.Find("WanderDimensions/WanderLeft").transform.position;
        wanderRight = GameObject.Find("WanderDimensions/WanderRight").transform.position;
        orderLocations[0] = GameObject.Find("WanderDimensions/OrderLocation1").transform.position;
        orderLocations[1] = GameObject.Find("WanderDimensions/OrderLocation2").transform.position;
        orderLocations[2] = GameObject.Find("WanderDimensions/OrderLocation3").transform.position;
        ChangeToInactive();
    }

    // Update is called once per frame
    void Update()
    {
        // Update finite state
        UpdateState();

        // Move forward as a function of velocity
        position += transform.forward * velocity * Time.deltaTime;
        transform.position = position;
    }

    // Update the current finite state for movement options
    void UpdateState()
    {
        switch(movementState)
        {
            case FiniteState.Inactive:
                // If paused, immediately start walking
                if(true)
                {
                    ChangeToWandering();
                }
                // Test function of WalkingToOrder
                else
                {
                    ChangeToWalkingToOrder();
                }
                break;
            case FiniteState.Wandering:
                // If the person has walked past the right wander point, pause
                if (position.x >= objective.x)
                {
                    ChangeToInactive();
                }
                break;
            case FiniteState.WalkingToOrder:
                // If the person is within 0.5 units of the objective, start the order
                if(Vector3.Distance(position, objective) < 0.5)
                {
                    ChangeToWaitingForOrder();
                }
                break;
            case FiniteState.WaitingForOrder:
                // If the person has been waiting for more than 5 seconds, leave
                if(timer >= 5)
                {
                    ChangeToLeaving();
                }
                else
                {
                    timer += Time.deltaTime;
                }
                break;
            case FiniteState.Leaving:
                // If the person walks beyond the right objective, change them to inactive
                if(position.x >= objective.x)
                {
                    ChangeToInactive();
                }
                break;
        }
    }

    // Move the person out of the rendered area without deallocating it from memory
    void ChangeToInactive()
    {
        movementState = FiniteState.Inactive;
        position = new Vector3(9999, 9999, 9999);
        transform.position = position;
        velocity = 0.0f;
    }

    // Initialize the position at wanderLeft, look towards wanderRight, then walk towards it
    void ChangeToWandering()
    {
        movementState = FiniteState.Wandering;
        position = wanderLeft;
        transform.position = position;
        objective = wanderRight;
        transform.forward = objective - transform.position;
        velocity = 2.0f;
    }

    // Initialize the position at wanderLeft, look at the first available order slot, then walk towards it
    void ChangeToWalkingToOrder()
    {
        movementState = FiniteState.WalkingToOrder;
        position = wanderLeft;
        transform.position = position;
        objective = orderLocations[2];
        transform.forward = objective - transform.position;
        velocity = 2.0f;
    }

    // Pause the position and set the rotation to face the shopkeeper
    void ChangeToWaitingForOrder()
    {
        movementState = FiniteState.WaitingForOrder;
        position = objective;
        transform.forward = new Vector3(0, 0, -1);
        velocity = 0.0f;
        timer = 0.0f;
    }

    // Set the objective to wanderRight and walk away
    void ChangeToLeaving()
    {
        movementState = FiniteState.Leaving;
        objective = wanderRight;
        transform.forward = objective - transform.position;
        velocity = 2.0f;
    }
}
