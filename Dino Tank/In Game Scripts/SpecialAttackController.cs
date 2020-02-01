/*  
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    ╔═════════════════════════════╡  DinoTank  ╞══════════════════════════════════════════════════════╗            
    ║ Authors:  Rahul Yerramneedi                       Email:    yr020409@gmail.com                  ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Purpose: This script is used as manager to control all the special attacks present in the game. ║
    ║          It adds all the damamge aspects of it from the colliders to all the visual affects     ║
    ║          animations as well.                                                                    ║
    ╟─────────────────────────────────────────────────────────────────────────────────────────────────╢ 
    ║ Usage: Script is added on to the special attack game object insdie the player tank gameobject   ║
    ║        prefab with all its compnents intact.                                                    ║                             
    ╚═════════════════════════════════════════════════════════════════════════════════════════════════╝
    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAttackController : MonoBehaviour, INetworkOwner 
{
    // Common attributes across all special attacks
    [Header("Common Attributes")]
    public Image reloadImage;
    public bool specialHasFired = false;
    public bool reloadComplete = false;
    public float specialChargeTime = 3;
    private float specialTimer = 0;
    public GameObject chargeParticles;
    public Transform chargeTrigger;

    // Lunge attack setup
    [Header("Lunge")]
    public GameObject lungeRadius;
    public GameObject lungeEffect;
    public GameObject lungeDamageEffect;
    public GameObject lungeCharge;
    public GameObject LungeObj;
    public Animator LungeNewAnim;
    public Transform lungeTrigger;
    public float lungeDamage;
    public float coolDownLunge;
    private float timerLunge = 0;
    bool lunge = false;

    //Stomp attack Setup
    [Header("Stomp")]
    public GameObject stompTrigger;
    public Transform stopTriggerPosition;
    public GameObject stompDamageEffect;
    public GameObject stompWave;
    public GameObject stompCharge;
    public GameObject StompObj;
    public float coolDownStomp;
    public float jumpForceMultiplier;
    public float stompEffectDelay;
    public float stompDamage;
    public float stompRadius;
    public float explosionForce;
    public float upwardModifier;
    private float timerStomp = 0;
    bool stomp = false;


    //Swipe attack Setup
    [Header("Swipe")]
    public SphereCollider swipeRadiusCollider;
    public GameObject swipeEffect;
    public GameObject swipeDamageEffect;
    public GameObject swipeCharge;
    public GameObject tailEffect;
    public GameObject afterEffect;
    public Animator StegoSwipeAnim;
    public GameObject StegoSwipe;
    public Transform swipeTrigger;
    public float coolDownSwipe;
    public float swipeDamage;
    public float explosionForceSwipe;
    private float timerSwipe = 0;
    bool swipe = false;

    //Bite attack Setup
    [Header("Bite")]
    public SphereCollider biteRadiusCollider;
    public GameObject biteEffect;
    public GameObject biteDamageEffect;
    public GameObject biteCharge;
    public float coolDownBite;
    public float biteDamage;
    public float explosionForceBite;
    public Animator BiteAnim;
    public GameObject BiteObj;
    private float timerBite = 0;
    bool bite = false;

    //Sonic Boom attack Setup
    [Header("Sonic Boom")]
    public SphereCollider sonicBoomRange;
    public GameObject sonicBoomEffect;
    public GameObject sonicBoomDamageEffect;
    public GameObject sonicBoomCharge;
    public Transform sonicBoomTrigger;
    public float coolDownSonicBoom;
    public float sonicboomDamage;
    public float explosionForceSonicBoom;
    private float timerSonicBoom = 0;
    bool sonicboom = false;

    // External references to other game components
    LevelManager levelManagerRef;
    RTCTankController controller;
    PhotonPlayer sender;
    Rigidbody body;
    Animator animator;
    bool hitOnce = false;

    // Variables that effect the physics of the player tank
    float massBackup;
    float dragBckup;
    float angularDragBackup;

    // Variables to manage the special attack button within UI 
    Loadoutpanel loadoutPanel;

    void Start()
    {
        hitOnce = false;
        //Setting references
        levelManagerRef = FindObjectOfType<LevelManager>();     
        controller = GetComponentInParent<RTCTankController>();
        body = controller.gameObject.GetComponent<Rigidbody>();
        animator = GetComponentInParent<Animator>();
		specialHasFired = false;
        reloadComplete = false;


        // Checking if the UI compnents are present
        if (!loadoutPanel || !reloadImage) // nothing to update 
        {
            loadoutPanel = FindObjectOfType<Loadoutpanel>();    // Find the loadout panel and if not null, refill image
            if (loadoutPanel)
                reloadImage = loadoutPanel.fillBarImage;
        }
        if (loadoutPanel && !loadoutPanel.specialButton)
        {
            loadoutPanel.specialButton = loadoutPanel.transform.parent.Find("SpecialAttackBTN").GetComponent<Button>();
        }

        // Define variables
        massBackup = body.mass;
        dragBckup = body.drag;
        angularDragBackup = body.angularDrag;     
        timerStomp = coolDownStomp;
        timerLunge = coolDownLunge;
        timerSwipe = coolDownSwipe;
        timerBite = coolDownBite;
        timerSonicBoom = coolDownSonicBoom;


        // if animator exists, turn it off. Will enable in code when needed.
        if (animator)
        {
            animator.enabled = false;
        }

        if(BiteObj)
        {
            BiteObj.SetActive(false);
            BiteAnim.enabled = false;
        }
        
        if(StegoSwipe)
        {
            StegoSwipe.SetActive(false);
        }
        
    }

    // Charging the special attack when the appropriate button is pressed
    public void ChargeSpecial()
    {
        //reloadImage.color = Color.yellow;
        reloadImage.fillClockwise = true;

        switch(controller.currentDino) // Will switch the special attack based on the player's current tank
        {
            case dino.Tricera:
                chargeParticles = lungeCharge;
                break;
            case dino.Trex:
                chargeParticles = biteCharge;
                break;
            case dino.Bronto:
                chargeParticles = stompCharge;
                break;
            case dino.Stego:
                chargeParticles = swipeCharge;
                break;
            case dino.Kentrosaurus:
                chargeParticles = swipeCharge;
                break;
            case dino.Duckbill:
                chargeParticles = sonicBoomCharge;
                break;
            default:
                break;
        }
        
        if (specialTimer <= specialChargeTime)
        {
            // Setting the charge particles on the barrel
            specialTimer += Time.deltaTime;
            reloadImage.fillAmount = (specialChargeTime - specialTimer) / specialChargeTime;
            if(chargeParticles)
            {
                chargeParticles.SetActive(true);
                if(chargeParticles.GetComponentInChildren<Light>())
                {
                    chargeParticles.GetComponentInChildren<Light>().intensity += specialTimer * 0.01f;
                }  
            }
        } 

        // Once the charge is complete, it will trigger the FireSpecial()
        else if (specialTimer > specialChargeTime)
        {
            FireSpecial();
        }
    }
    
    // Will reset the special attack once it has been used
    public void ResetSpecial()
    {
        if (chargeParticles)
        {
            if (chargeParticles.GetComponentInChildren<Light>()) // changing the particle effects while its resetting
            {
                chargeParticles.GetComponentInChildren<Light>().intensity = 0f;
                chargeParticles.GetComponentInChildren<Light>().color = Color.green;
                chargeParticles.GetComponentInChildren<Light>().range = 3f;
            }
            chargeParticles.SetActive(false);
        }        
        specialTimer = 0;
    }

    // Fucntion is called when the special attaack has been a performed and will then to proceed to reset the special attack for use again
    public void FireSpecial()
    {
        ResetSpecial();
        loadoutPanel.SpecialAttackClicked();
    }

    // Lunge Special Attack - Triceratops
    public void Lunge(Animator _dinoAnimator)           // Takes in an animator for the dino
    {
        if (timerLunge >= coolDownLunge)                // If lunge timer is greater or equal to the lunge cool down
        {            
            body.mass = 1000;                            // Body mass is now 1000          
            if (animator) animator.enabled = true;       // If i have an animator, enable it
            EnableInput(false);                          // And disable input
            lunge = true;
            _dinoAnimator.SetTrigger("SpecialAttack");   // Set the dino animator trigger to be named Special attack
            animator.SetInteger("Dinotype", (int)controller.thisTank);
            if (animator) animator.SetTrigger("Lunge");  // Damage triggered from the animation 
            loadoutPanel.specialButton.GetComponent<Animator>().SetBool("IsReady", false);
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
			specialHasFired = true;
            DEBUG.Log("Lunge special attack has been carried out witht the all the animator triggers set");
        }
    }

    // Sonic Boom Special Attack - Duckbill
    public void SonicBoom(Animator _dinoAnimator) // Takes in an animator for the dino
    {
        if (timerSonicBoom >= coolDownSonicBoom) // if sonic boom timer is greater or equal to the sonic boom cool down
        {
            if (animator) animator.enabled = true;
            sonicboom = true;
            EnableInput(false);
            _dinoAnimator.SetTrigger("SpecialAttack"); // Set the dino animator trigger to be named Special attack
            if (animator)
            {
                animator.SetTrigger("SonicBoom"); // Set the dino animator trigger for Sonic Boom
            }
            if (loadoutPanel.specialButtonAnimator)
            {
                loadoutPanel.specialButtonAnimator.SetBool("IsReady", false);
            }
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
            specialHasFired = true;
            DEBUG.Log("Sonic boom special attack has been carried out witht the all the animator triggers set");
        }
    }

    // Stomp Special Attack - Bronto
    public void Stomp(Animator _dinoAnimator)            // Takes in an animator for the dino
    {
        if (timerStomp >= coolDownStomp)                // if stomp timer is greater or equal to my stomp cool down
        {
            if (animator) animator.enabled = true;
            stomp = true;
            EnableInput(false);
            _dinoAnimator.SetTrigger("SpecialAttack");
            animator.SetInteger("Dinotype", (int)controller.thisTank);
            if (animator) animator.SetTrigger("Stomp");
            if (loadoutPanel.specialButtonAnimator)
                loadoutPanel.specialButtonAnimator.SetBool("IsReady", false);
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
            DEBUG.Log("Stomp special attack has been carried out witht the all the animator triggers set");
        }
    }

    // Swipe Special Attack - Stego
    public void Swipe(Animator _dinoAnimator)       
    {
        if (timerSwipe >= coolDownSwipe)            // if swipe is greater or equal to my swipe cool down
        {
            if (animator) animator.enabled = true;
            swipe = true;
            EnableInput(false);
            _dinoAnimator.SetTrigger("SpecialAttack");
            animator.SetInteger("Dinotype", (int)controller.thisTank);
            if (animator) animator.SetTrigger("Swipe");
            if (loadoutPanel.specialButtonAnimator)
                loadoutPanel.specialButtonAnimator.SetBool("IsReady", false);
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
            specialHasFired = true;
            DEBUG.Log("Swipe special attack has been carried out witht the all the animator triggers set");
        }
    }    

    //Bite Special Attack -TRex
    public void Bite(Animator _dinoAnimator)
    {
        if (timerBite >= coolDownBite)
        {
            if (animator) animator.enabled = true;
            bite = true;
            EnableInput(false);
            if (animator) animator.SetTrigger("Bite");
            animator.SetInteger("Dinotype", (int)controller.thisTank);
            _dinoAnimator.SetTrigger("SpecialAttack");
            if (loadoutPanel.specialButtonAnimator)
                loadoutPanel.specialButtonAnimator.SetBool("IsReady", false);
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
            specialHasFired = true;
            DEBUG.Log("Bite special attack has been carried out witht the all the animator triggers set");
        }
    }


    void Update()
    {
        if (!APP.IsReady)
            return;

        // Bounce out if this tank is AI 
        if (controller.isAI )
            return;
  
        if (!loadoutPanel || !reloadImage) 
        {
            loadoutPanel = FindObjectOfType<Loadoutpanel>();    
            if (loadoutPanel)
            {
                reloadImage = loadoutPanel.fillBarImage;
            }
        }
        if (loadoutPanel && !loadoutPanel.specialButton)
        {
            loadoutPanel.specialButton = loadoutPanel.transform.parent.Find("SpecialAttackBTN").GetComponent<Button>();      
        }

        if (lunge)  // If lunge is true, updating the animations for the sepcial attack and the icons
        {
            lunge = false;
            timerLunge = 0;
			specialHasFired = true;
            reloadComplete = false;
            StartCoroutine(LungeAnimation());
        }
        else if(controller.currentDino == dino.Tricera)
        {
            timerLunge += Time.deltaTime;
            if (timerLunge >= coolDownLunge) // restoring the cool down
            {              
                timerLunge = coolDownLunge;
                reloadComplete = true;
                if (reloadImage) // will fill the cooldown bar with the appropriate color
                {
                    reloadImage.fillAmount = coolDownLunge;
                    reloadImage.color = Color.green;
                }

                if (loadoutPanel.specialButton.isActiveAndEnabled)
                {
                    if (loadoutPanel.specialButtonAnimator)
                    {
                        loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                    }
                    loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
					specialHasFired = false;

                }
            }
            else
            {
                reloadImage.fillClockwise = true;
                reloadImage.fillAmount = timerLunge / coolDownLunge;
            }

            // Changing colors of the cooldown bar based on the fill amounts
            if (reloadImage.fillAmount >= 0.75)
            {
                reloadImage.color = Color.green;
            }
            else if (reloadImage.fillAmount > 0.25)
            {
                reloadImage.color = Color.yellow;
            }
            else if (reloadImage.fillAmount < 0.25)
            {
                reloadImage.color = Color.red;
            }
        }

        //if swipe is true
        if (swipe)
        {
            swipe = false;
            timerSwipe = 0;
            specialHasFired = true;
            reloadComplete = false;
            StartCoroutine(StegoSwipeAnimation());
        }
        else if (controller.currentDino == dino.Stego && reloadImage || controller.currentDino == dino.Kentrosaurus && reloadImage)
        {
            timerSwipe += Time.deltaTime;
            if (timerSwipe >= coolDownSwipe)
            {
                timerSwipe = coolDownSwipe;
                reloadComplete = true;
                if (reloadImage)
                {
                    reloadImage.fillAmount = coolDownLunge;
                    reloadImage.color = Color.green;
                }
                if (loadoutPanel.specialButton.isActiveAndEnabled)
                {
                    if (loadoutPanel.specialButtonAnimator)
                    {
                        loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                    }
                    loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                    specialHasFired = false;
                }

            }
            else
            {
                reloadImage.fillClockwise = true;
                reloadImage.fillAmount = timerSwipe / coolDownSwipe;
            }

            if (reloadImage.fillAmount >= 0.75)
            {
                reloadImage.color = Color.green;
            }
            else if (reloadImage.fillAmount > 0.25)
            {
                reloadImage.color = Color.yellow;
            }
            else if (reloadImage.fillAmount < 0.25)
            {
                reloadImage.color = Color.red;
            }
        }

        //If stomp is true
        if (stomp)      
        {
            stomp = false;
            timerStomp = 0;
            specialHasFired = true;
            reloadComplete = false;
            Instantiate(StompObj, controller.transform.position, controller.transform.rotation);
        }
        else if (controller.currentDino == dino.Bronto)
        {
            timerStomp += Time.deltaTime;
            if (timerStomp >= coolDownStomp)
            {
                timerStomp = coolDownStomp;
                reloadComplete = true;
                if (reloadImage)
                {
                    reloadImage.fillAmount = coolDownLunge;
                    reloadImage.color = Color.green;
                }
                if (loadoutPanel.specialButton.isActiveAndEnabled)
                {
                    if (loadoutPanel.specialButtonAnimator)
                    {
                        loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                    }
                    loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                    specialHasFired = false;
                }
            }
            else
            {
                reloadImage.fillClockwise = true;
                reloadImage.fillAmount = timerStomp / coolDownStomp;
            }
            if (reloadImage.fillAmount >= 0.75)
            {
                reloadImage.color = Color.green;
            }
            else if (reloadImage.fillAmount > 0.25)
            {
                reloadImage.color = Color.yellow;
            }
            else if (reloadImage.fillAmount < 0.25)
            {
                reloadImage.color = Color.red;
            }
        }

        // If bite is true
        if (bite)
        {
            bite = false;
            timerBite = 0;
            specialHasFired = true;
            reloadComplete = false;
            Instantiate(stompTrigger, stopTriggerPosition.position, stopTriggerPosition.rotation, transform.parent);
            StartCoroutine(BiteAnimation());
        }
        else if (controller.currentDino == dino.Trex)
        {
            timerBite += Time.deltaTime;
            if (timerBite >= coolDownBite) // restoring 
            {
                timerLunge = coolDownLunge;
                reloadComplete = true;
                if (reloadImage)
                    reloadImage.fillAmount = 0;
                if (loadoutPanel)
                {
                    if (reloadImage)
                    {
                        reloadImage.fillAmount = coolDownLunge;
                        reloadImage.color = Color.green;
                    }
                    if (loadoutPanel.specialButton.isActiveAndEnabled)
                    {
                        if (loadoutPanel.specialButtonAnimator)
                        {
                            loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                        }
                        loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                        specialHasFired = false;
                    }
                }
            }
            else
            {
                reloadImage.fillClockwise = true;
                reloadImage.fillAmount = timerBite / coolDownBite;

            }
            if (reloadImage.fillAmount >= 0.75)
            {
                reloadImage.color = Color.green;
            }
            else if (reloadImage.fillAmount > 0.25)
            {
                reloadImage.color = Color.yellow;
            }
            else if (reloadImage.fillAmount < 0.25)
            {
                reloadImage.color = Color.red;
            }
        }


        // If sonicboom is true
        if (sonicboom)
        {
            Debug.Log("SonicBoom in Update");
            sonicBoomEffect.SetActive(true);
            sonicboom = false;
            timerSonicBoom = 0;
            specialHasFired = true;
            reloadComplete = false;
            Instantiate(sonicBoomEffect, sonicBoomTrigger.position, sonicBoomTrigger.rotation, transform.parent);
        }
        else if (controller.currentDino == dino.Duckbill)
        {
            timerSonicBoom += Time.deltaTime;
            if (timerSonicBoom >= coolDownSonicBoom)
            {
                timerSonicBoom = coolDownSonicBoom;
                reloadComplete = true;
                if (reloadImage)
                    reloadImage.fillAmount = 0;
                if (loadoutPanel)
                {
                    if (reloadImage)
                    {
                        reloadImage.fillAmount = coolDownSonicBoom;
                        reloadImage.color = Color.green;
                    }
                    if (loadoutPanel.specialButton.isActiveAndEnabled)
                    {
                        if (loadoutPanel.specialButtonAnimator)
                        {
                            loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                        }
                        loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                        specialHasFired = false;
                    }
                }
            }
            else
            {
                reloadImage.fillClockwise = true;
                reloadImage.fillAmount = timerSonicBoom / coolDownSonicBoom;
            }
            if (reloadImage.fillAmount >= 0.75)
            {
                reloadImage.color = Color.green;
            }
            else if (reloadImage.fillAmount > 0.25)
            {
                reloadImage.color = Color.yellow;
            }
            else if (reloadImage.fillAmount < 0.25)
            {
                reloadImage.color = Color.red;
            }
        }    
    }

    #region Animation Events
    // called by animation through the TankController
    public void SwitchOffAnimatorFromTrigger()
    {
        body.mass = massBackup;
        body.drag = dragBckup;
        body.angularDrag = angularDragBackup;
        if (animator) animator.enabled = false;
        EnableInput(true);    
    }

    // called by animation through the TankController and also calculates the damage being done to the tanks in range of the user.
    public void SwipeAnimTrigger()
    {
        float swipeRadiusfloat = swipeRadiusCollider.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(swipeRadiusCollider.transform.position, swipeRadiusfloat);

        foreach (Collider hit in colliders) //For every collider that my overlapshere hit, do this..
        {
            // if we are not the player 
            if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject != controller.gameObject && !hitOnce)
            {
                // if we have a rigid body
                if (hit && hit.GetComponent<Rigidbody>())
                {
                    Rigidbody hitBody = hit.GetComponent<Rigidbody>();
                    hitBody.mass /= 5;
                    hitBody.isKinematic = false;
                    hitBody.AddExplosionForce(explosionForceSwipe, swipeRadiusCollider.transform.position, swipeRadiusfloat, upwardModifier);
                }

                // if we hit an enemy tank
                if (hit && (hit.gameObject.tag == "playerTank" || hit.gameObject.tag == "Player"))
                {
                    RTCTankController hitTank = hit.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();

                    if (hitTank)
                    {
                        hitTank.TakeDamage(swipeDamage, this.gameObject);
                        hitOnce = true;
                        if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }

                if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>())
                {
                    hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>().TakeDamage(swipeDamage, this.gameObject);
                }
            }
        }
        hitOnce = false;
    }

    // called by animation through the TankController and also calculates the damage that is being in the line of sight and if any enemies are in it.
    public void SonicBoomAnimTrigger()
    {
        float sonicboomRadiusfloat = sonicBoomRange.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(sonicBoomRange.transform.position, sonicboomRadiusfloat);
        RaycastHit[] hits = Physics.SphereCastAll(sonicBoomRange.transform.position, 3 * sonicboomRadiusfloat, sonicBoomRange.transform.forward, 100 * sonicboomRadiusfloat);

        foreach(RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.transform.parent && hit.collider.gameObject.transform.parent.gameObject != controller.gameObject && !hitOnce)
            {
                // if we have a rigid body
                if (hit.collider && hit.collider.gameObject.GetComponent<Rigidbody>())
                {
                    Rigidbody hitBody = hit.collider.gameObject.GetComponent<Rigidbody>();
                    hitBody.mass /= 5;
                    hitBody.isKinematic = false;
                    hitBody.AddExplosionForce(explosionForceSonicBoom, hit.collider.gameObject.transform.position, sonicboomRadiusfloat, upwardModifier);
                    Instantiate(sonicBoomDamageEffect, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);
                }

                // if we hit an enemy tank
                if (hit.collider && (hit.collider.gameObject.tag == "playerTank" || hit.collider.gameObject.tag == "Player"))
                {
                    RTCTankController hitTank = hit.collider.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();
                    Instantiate(sonicBoomDamageEffect, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);

                    if (hitTank)
                    {
                        hitTank.TakeDamage(sonicboomDamage, this.gameObject);
                        hitOnce = true;
                        if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }
                
                if (hit.collider.gameObject.transform.parent && hit.collider.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>())
                {
                    hit.collider.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>().TakeDamage(sonicboomDamage, this.gameObject);
                }
            }
        }
        hitOnce = false;

                if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>())
                {
                    hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>().TakeDamage(sonicboomDamage, this.gameObject);
                }
            }
    }

    // called by animation through the TankController
    public void LungeAnimTrigger()
    {
        float lungeRadiusfloat = lungeRadius.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(lungeRadius.transform.position, lungeRadiusfloat);
        foreach (Collider hit in colliders)
        {
            // we we are not the player 
            if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject != controller.gameObject && !hitOnce)
            {
                // if we have a rigid body
                if (hit && hit.GetComponent<Rigidbody>())
                {
                    Rigidbody hitBody = hit.GetComponent<Rigidbody>();
                    hitBody.mass /= 5;
                    hitBody.isKinematic = false;
                    hitBody.AddExplosionForce(explosionForce, hit.gameObject.transform.position, lungeRadiusfloat, upwardModifier);
					Instantiate(lungeDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);
                }
                if (hit && (hit.gameObject.tag == "playerTank" || hit.gameObject.tag == "Player")) // we hit an enemy tank.
                {
                    RTCTankController hitTank = hit.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();
					Instantiate(lungeDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);
			

                    if (hitTank)
                    {
                        hitTank.TakeDamage(lungeDamage, this.gameObject);
                        hitOnce = true;
                        if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }
				if (hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy> ()) {
					hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy> ().TakeDamage (lungeDamage, this.gameObject);
				} else {
					Instantiate (lungeEffect, lungeRadius.transform.position, lungeRadius.transform.rotation);
				}
            }
        }
        hitOnce = false;
    }

    // called by animation through the TankController
    public void BiteAnimTrigger()
    {
        float biteRadiusfloat = biteRadiusCollider.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(biteRadiusCollider.transform.position, biteRadiusfloat);
        GameObject biteeffectInstance = Instantiate(biteEffect, biteRadiusCollider.transform.position, biteRadiusCollider.transform.rotation);
        StartCoroutine("BiteEffectFadeAndDelay", biteeffectInstance);
        foreach (Collider hit in colliders)
        {
            // we we are not the player 
            if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject != controller.gameObject && !hitOnce)
            {
                // if we have a rigid body
                if (hit && hit.GetComponent<Rigidbody>())
                {
                    Rigidbody hitBody = hit.GetComponent<Rigidbody>();
                    hitBody.mass /= 5;
                    hitBody.isKinematic = false;
                    hitBody.AddExplosionForce(explosionForce, biteRadiusCollider.transform.position, biteRadiusfloat, upwardModifier);
                    Instantiate(biteDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);
                }
                if (hit && (hit.gameObject.tag == "playerTank" || hit.gameObject.tag == "Player")) // we hit an enemy tank.
                {
                    RTCTankController hitTank = hit.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();
                    Instantiate(biteDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);

                    if (hitTank)
                    {
                        hitTank.TakeDamage(biteDamage,  this.gameObject);
                        hitOnce = true;
                       
                            if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }
                if (hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>()!=null)
                {
                    hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>().TakeDamage(biteDamage, this.gameObject);
                }
            }
        }
        hitOnce = false;
    }

    IEnumerator StopDamageAfterJump()
    {
        Instantiate(stompWave, stopTriggerPosition.position, stopTriggerPosition.rotation);

        if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
        {
            controller.photonView.RPC("PlayDinoGrowl", PhotonTargets.All, (int)controller.currentDino, transform.position);
        }
        else
        {
            AudioManager.PlayDinoGrowl(dino.Bronto, transform.position);
        }
         
      
        yield return new WaitForSeconds(stompEffectDelay);

        Collider[] colliders = Physics.OverlapSphere(transform.parent.position, stompRadius);
        foreach (Collider hit in colliders)
        {
            // we are not the player 
            if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject != controller.gameObject && !hitOnce)
            {
                // if we have a rigid body
                if (hit && hit.GetComponent<Rigidbody>())
                {
                    Rigidbody hitBody = hit.GetComponent<Rigidbody>();
                    hitBody.mass /= 5;
                    hitBody.isKinematic = false;
                    hitBody.AddExplosionForce(explosionForce, transform.position, stompRadius, upwardModifier);
                    Instantiate(stompDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);
                }
                if (hit && (hit.gameObject.tag == "playerTank" || hit.gameObject.tag == "Player")) // we hit an enemy tank.
                {
                    RTCTankController hitTank = hit.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();
                    Instantiate(stompDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);

                    if (hitTank)
                    { 
                        hitTank.TakeDamage(stompDamage, this.gameObject);
                        hitOnce = true;
                        if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }
                if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>())
                {
                    hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>().TakeDamage(stompDamage,  this.gameObject);
                    //hitOnce = true;
                }
            }
        }
        hitOnce = false;
    }
#endregion

    // Coroutine to carry out all the Bite Animation effects 
    IEnumerator BiteAnimation()
    {
        BiteObj.SetActive(true);
        BiteAnim.enabled = true;
        BiteAnim.Play("BiteAnimation");
        yield return new WaitForSeconds(0.5f);
        BiteAnim.enabled = false;
        BiteObj.SetActive(false);
    }

    // Coroutine to carry out all the Lunge Animation effects 
    IEnumerator LungeAnimation()
    {
        LungeObj.SetActive(true);
        LungeNewAnim.enabled = true;
        LungeNewAnim.Play("BiteAnimation");
        yield return new WaitForSeconds(0.5f);
        LungeObj.SetActive(false);
        LungeNewAnim.enabled = false;
    }

    // Coroutine to carry out all the Swipe Animation effects 
    IEnumerator StegoSwipeAnimation()
    {
        StegoSwipe.SetActive(true);
        StegoSwipeAnim.Play("SwipeStego");
        yield return new WaitForSeconds(0.4f);
        StegoSwipe.SetActive(false);
    }

    public void SetOwner(GameObject _owner)
    {
        owner = _owner;
    }

    public GameObject GetOwner()
    {
        return controller.gameObject;
    }

    public void SetSender(PhotonPlayer _sender)
    {
        sender = _sender;
    }

    public PhotonPlayer GetSender()
    {
        return sender;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Debug.DrawLine(sonicBoomRange.transform.position, sonicBoomRange.transform.position + sonicBoomRange.transform.forward * (100 * sonicBoomRange.GetComponent<SphereCollider>().radius));
        Gizmos.DrawWireSphere(sonicBoomRange.transform.position + sonicBoomRange.transform.forward * (100 * sonicBoomRange.GetComponent<SphereCollider>().radius), 3 * sonicBoomRange.GetComponent<SphereCollider>().radius);
    }

