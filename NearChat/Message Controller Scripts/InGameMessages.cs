/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  NearChat  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is used to control how the next messages will appear in the game. It can   ║
    ║          either be a series of consecutive messages or single messsages as well. The player can ║
    ║          also add a gameobject that will serve as a loading box and can also chaneg the         ║
    ║          duration between which the messages will load.                                         ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Script should be added on the message prefab and settings can be adjusted accordingly.   ║                                        
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InGameMessages : MonoBehaviour {

    // This contains all the message box related details like names and texts
    [Header("Strings")]
    public string ParentName;
    public string ScrollName;
    private string[] deckText;
    private string[] instantiateChangeTexts;

    // This contains all the gamobjects that the messages are present in or instantiate in as
    [Header("GameObjects")]
    private GameObject[] deck;
    private GameObject[] instantiatedObjects;
    public GameObject LoadingMessage;
    public GameObject TheScroll;
    public GameObject mssg;

    // Varibales being used to count the texts and times
    [Header("Variables")]
    public int textCounter = 0;
    public float timermessage;

    [Header("Texts")]
    public Text TimefromMessage;
    public Text MainPhonTime;

    [Header("Other Scripts")]
    public ChatChildCheck CC;

    // Use this for initialization
    void Start()
    {
        mssg = GameObject.Find(ParentName);
        MainPhonTime = GameObject.Find("MainPhoneTime").GetComponent<Text>();
        TheScroll = GameObject.Find(ScrollName);
        StartCoroutine(Fills());
        Canvas.ForceUpdateCanvases();
        TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    // This fucntion will grab all the necessary details from teh gamobjects and start updating the canvas
    public void StartChat()
    {
        mssg = GameObject.Find(ParentName); // Getting the gameobject parent's name
        MainPhonTime = GameObject.Find("MainPhoneTime").GetComponent<Text>(); // Finding the text that displays the phone's time
        TheScroll = GameObject.Find(ScrollName); // Getting the gameobject that is doing the scrolling 
        StartCoroutine(LoadMessages()); 
        Canvas.ForceUpdateCanvases(); // After loading the messages the canvas will update, to make sure the new messages are displayed
        TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f; // The scroll will also be updated to 0, to accomodate the new messages and scroll again
        Canvas.ForceUpdateCanvases();
    }

    //Coroutine that will load all the messages added and also add the loading message prefab 
    IEnumerator LoadMessages()
    {
        instantiatedObjects = new GameObject[deck.Length]; // Getting the number of objects present
        for (int i = 0; i < deck.Length; i++) // Keep adding messages til "i" is equal to the number of objects present in the list
        {
            yield return new WaitForSeconds(2);
            GameObject mg = Instantiate(LoadingMessage, transform.position, Quaternion.identity); // Instantiates the loading message
            mg.transform.SetParent(mssg.transform); // Adds the message to the Parent
            mg.transform.localScale = new Vector3(1, 1, 1); // Sets their position
            Canvas.ForceUpdateCanvases(); // Updating canvas 
            TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
            yield return new WaitForSeconds(timermessage);
            Destroy(mg); // Destroying the loading message
            instanciatedObjects[i] = Instantiate((deck[i]) as GameObject, transform.position, Quaternion.identity); // Adding the actual message present in the list
            instanciatedObjects[i].transform.SetParent(mssg.transform); // Adds the message to the parent
            instanciatedObjects[i].transform.localScale = new Vector3(1, 1, 1); // Sets their position
            Canvas.ForceUpdateCanvases(); // Updating canvas 
            TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases(); // Updating canvas 
            textCounter += 1;
        }
    }
}
