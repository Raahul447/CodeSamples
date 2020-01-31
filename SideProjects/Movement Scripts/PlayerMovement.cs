/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  FPS Project  ╞═══════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is a basic first person character controller with the player movement.     ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Script is added on to the player gameobject and making sure it has a characterController.║
    ║                                                                                                 ║
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Variables for movement
    private CharacterController CharacController;
    [SerializeField]
    private float speed = 5f;
    private float gravity = 9.18f;

    // Start is called before the first frame update
    void Start()
    {
        CharacController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();
    }

    void CalculateMovement()
    {
        // Getting the Player Inputs for the controls
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        // Determining the player location with the inputs
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        Vector3 velocity = direction * speed;
        // To keep the player grounded with gravity
        velocity.y -= gravity;
        velocity = transform.transform.TransformDirection(velocity);
        CharacController.Move(velocity * Time.deltaTime);
    }
}
