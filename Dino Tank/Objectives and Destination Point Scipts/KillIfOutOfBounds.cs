/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  DinoTank  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is used to determine if the player is going out of bounds in the game.     ║ 
    ║          If so, the timer present in the game will start reducing. Once it hits 0, the player   ║
    ║          will die and level will reset.                                                         ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Add a collider around the area that is going to be the out of bounds area.               ║
    ║        Then, add this script on to it.                                                          ║ 
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillIfOutOfBounds : MonoBehaviour
{
   
    public static float timeLeft = 0f;  //Float being used for the In-Game time that will be activated once you start playing a particular mode//
    public RTCTankController player;   //Accessing the Tank Controller script from the Player Tank//
    public bool outOfBounds;   //Bool that will switched on and off, if the player tank is Out of Bounds//

    // Use this for initialization
    void Start()
    {
        outOfBounds = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (outOfBounds) // player is out of bounds
        {
            timeLeft -= Time.deltaTime; // the In-Game time starts reducing
            if (timeLeft < 0)
            {
                timeLeft = 0;
                player.TankDies(); // this function is being called from the "RTCTankController" script of the player 
            }

        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "playerTank" || other.gameObject.tag == "Player") // checking if player tank is in the out of bouns collider
        {
            timeLeft = 0;
            outOfBounds = true;
            player = other.GetComponentInParent<RTCTankController>(); // grabbing the RTCTankController script from the player Tank
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("playerTank") || other.gameObject.tag == "Player")
        {
            outOfBounds = false;
            timeLeft = 0;
        }
    }
}
