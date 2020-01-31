/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  NearChat  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is a simple Outline changer that checks which message is in the scene and  ║
    ║          based on that it will automatically change the color of the message and its outline.   ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Script is added on to the UI prefab with all its compnents intact.                       ║                                                   
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxOutline : MonoBehaviour
{
    [Header("UI Elements")] // All the UI elements for the messages
    public Outline textOutline;
    public Image imageColor; // Background for the message
    public Text nameText; // Text for the character's names that appear beside the message

	// Use this for initialization
	void Start ()
    {
        // Getting all the necessary UI components to the gameobjects
        textOutline = gameObject.GetComponent<Outline>();
        imageColor = gameObject.GetComponent<Image>();
        nameText = this.gameObject.transform.GetChild(1).GetComponent<Text>();
        
        // Checking to see if gameobject has a textOutline
        if(textOutline == null)
        {
            Debug.LogError(gameObject + "Does not have a outline!"); 
        }
        // Checking to see if gameobject has a nameText 
        if (nameText == null)
        {
            Debug.LogError(gameObject + "Does not have a name!");
        }
        textOutline.effectDistance = new Vector2(4, 4);

        // Will check the gameobjects tag and based on that the colors will be assigned
        switch(textOutline.gameObject.tag)
        {
            case "Yeong-Suk":
                textOutline.effectColor = new Color32(180, 134, 4,255);
                imageColor.color = new Color32(231, 196, 97, 255);
                nameText.text = "Yeong-Suk".ToUpper(); 
                break;
            case "Lilliana":
                textOutline.effectColor = new Color32(197, 20, 60,255);
                imageColor.color = new Color32(250, 86, 123, 255);
                nameText.text = "Lilliana".ToUpper();
                break;
            case "Basil":
                textOutline.effectColor = new Color32(236, 54, 43,255);
                imageColor.color = new Color32(247, 107, 99, 255);
                nameText.text = "Basil".ToUpper();
                break;
            case "Avery":
                textOutline.effectColor = new Color32(20, 183, 20,255);
                imageColor.color = new Color32(119, 221, 119, 255);
                nameText.text = "Avery".ToUpper();
                break;
            case "Idris":
                textOutline.effectColor = new Color32(5, 152, 214,255);
                imageColor.color = new Color32(71, 200, 255, 255);
                nameText.text = "Idris".ToUpper();
                break;
            case "Paige":
                textOutline.effectColor = new Color32(135, 84, 231,255);
                imageColor.color = new Color32(177, 156, 217, 255);
                nameText.text = "Paige".ToUpper();
                break;
            case "Player":
                textOutline.effectColor = new Color32(14, 100, 106, 255);
                imageColor.color = new Color32(0, 197, 200, 255);
                break;
            case "Untagged":
                Debug.LogError("Hey this " + textOutline.gameObject.name + "does not have a tag associated with it!");
                break;
            case null:
                Debug.LogError("Hey this " + textOutline.gameObject.name + "does not have a tag associated with it!");
                break;
        }

	}
}
