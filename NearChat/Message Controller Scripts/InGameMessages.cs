using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InGameMessages : MonoBehaviour {

    [Header("Strings")]
    public string ParentName;
    public string ScrollName;
    private string[] deckText;
    private string[] instantiateChangeTexts;

    [Header("GameObjects")]
    private GameObject[] deck;
    private GameObject[] instanciatedObjects;
    public GameObject LoadingMessage;
    public GameObject TheScroll;
    public GameObject mssg;

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

    public void StartChat()
    {
        mssg = GameObject.Find(ParentName);
        MainPhonTime = GameObject.Find("MainPhoneTime").GetComponent<Text>();
        TheScroll = GameObject.Find(ScrollName);
        StartCoroutine(Fills());
        Canvas.ForceUpdateCanvases();
        TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    IEnumerator Fills()
    {
        instanciatedObjects = new GameObject[deck.Length];
        for (int i = 0; i < deck.Length; i++)
        {
            yield return new WaitForSeconds(2);
            GameObject mg = Instantiate(LoadingMessage, transform.position, Quaternion.identity);
            mg.transform.SetParent(mssg.transform);
            mg.transform.localScale = new Vector3(1, 1, 1);
            Canvas.ForceUpdateCanvases();
            TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
            yield return new WaitForSeconds(timermessage);
            Destroy(mg);
            instanciatedObjects[i] = Instantiate((deck[i]) as GameObject, transform.position, Quaternion.identity);
            instanciatedObjects[i].transform.SetParent(mssg.transform);
            instanciatedObjects[i].transform.localScale = new Vector3(1, 1, 1);
            Canvas.ForceUpdateCanvases();
            TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
            textCounter += 1;
        }
    }

    public void stratif()
    {
        mssg = GameObject.Find(ParentName);
        MainPhonTime = GameObject.Find("MainPhoneTime").GetComponent<Text>();
        TheScroll = GameObject.Find(ScrollName);
        StartCoroutine(Fills());
        Canvas.ForceUpdateCanvases();
        TheScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }
}
