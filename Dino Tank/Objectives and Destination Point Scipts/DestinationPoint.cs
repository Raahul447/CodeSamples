/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  DinoTank  ╞══════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com  ║
    ╟─────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose:                                                                        ║
    ║ Usage:                                                                          ║
    ╚═════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DestinationPoint : Objective
{

    //Bool to be used to 
    public bool hideUntilActivated = true;
  
    void OnEnable()
    {
        if (GetComponent<MapMarker>())
        {
            GetComponent<MapMarker>().isActive = true;
        }              
    }


    void OnDisable()
    {
        if (GetComponent<MapMarker>())
        {
            GetComponent<MapMarker>().isActive = false;
        }        
    }


    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.parent && collider.transform.parent.gameObject == PlayerTankmanager.GetPlayer())
        {
            FindObjectOfType<DestinationController>().UnregisterDestination(this);
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
