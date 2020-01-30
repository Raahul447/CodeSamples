/////////////////////////////////////////////
//Author: Dmitrii and Colton
//Date: 20171111
//
//Purpose: Controls and sets all special attacks for each playable dinosuar
/////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAttackController : MonoBehaviour, INetworkOwner 
{

    //[Header("Common Data")]
    //public ParticleSystem chargeParticles;

    [Header("Common Attributes")]
    public Image reloadImage;
    public bool specialHasFired = false;
    public bool reloadComplete = false;
    public float specialChargeTime = 3;
    private float specialTimer = 0;
    public GameObject chargeParticles;
    public Transform chargeTrigger;

    //Lunge setup
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

    //Stomp Setup
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


    //Swipe Setup
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


    //Bite Setup
    [Header("Bite")]
    public SphereCollider biteRadiusCollider;
    public GameObject biteEffect;
    public GameObject biteDamageEffect;
    public GameObject biteCharge;
    //public Transform biteTrigger;
    public float coolDownBite;
    public float biteDamage;
    public float explosionForceBite;
    public Animator BiteAnim;
    public GameObject BiteObj;


    //Sonic Boom Setup
    [Header("Sonic Boom")]
    public SphereCollider sonicBoomRange;
    public GameObject sonicBoomEffect;
    public GameObject sonicBoomDamageEffect;
    public GameObject sonicBoomCharge;
    public Transform sonicBoomTrigger;
    public float coolDownSonicBoom;
    public float sonicboomDamage;
    public float explosionForceSonicBoom;


    //MISSING SONIC BOOM NEED TO ADD THIS

    LevelManager levelManagerRef;
    RTCTankController controller; // OWNER 
    PhotonPlayer sender;
    //  RTCTankGunController gunController;
    Rigidbody body;
    Animator animator;

    private float timerStomp = 0;
    private float timerLunge = 0;
    private float timerSwipe = 0;
    private float timerBite = 0;
    private float timerSonicBoom = 0;

    bool lunge = false;
    bool stomp = false;
    bool swipe = false;
    bool bite = false;
    bool sonicboom = false;
    float massBackup;
    float dragBckup;
    float angularDragBackup;
    private bool hitOnce = false;

    // Variables to manage the special attack button in UI
    Loadoutpanel loadoutPanel;

    //public GameObject Tail;
    //public Animator TailAnim;
    //public Animator StompAnim;

    void Start()
    {
        hitOnce = false;
        //Set references
        levelManagerRef = FindObjectOfType<LevelManager>();     
        controller = GetComponentInParent<RTCTankController>();
        body = controller.gameObject.GetComponent<Rigidbody>();
        animator = GetComponentInParent<Animator>();
		specialHasFired = false;
        reloadComplete = false;

        if (!loadoutPanel || !reloadImage) // nothing to update 
        {
            loadoutPanel = FindObjectOfType<Loadoutpanel>();    //Find the loadout panel and if not null, refill image
            if (loadoutPanel)
                reloadImage = loadoutPanel.fillBarImage;
        }
        if (loadoutPanel && !loadoutPanel.specialButton)
        {
            loadoutPanel.specialButton = loadoutPanel.transform.parent.Find("SpecialAttackBTN").GetComponent<Button>();
        }

        //Define variables
        massBackup = body.mass;
        dragBckup = body.drag;
        angularDragBackup = body.angularDrag;     
        timerStomp = coolDownStomp;
        timerLunge = coolDownLunge;
        timerSwipe = coolDownSwipe;
        timerBite = coolDownBite;
        timerSonicBoom = coolDownSonicBoom;


        //if animator exist, turn it off. Will enable in code when needed.
        if (animator)
            animator.enabled = false;
        //Tail.SetActive(false);

        if(BiteObj)
        {
            BiteObj.SetActive(false);
            BiteAnim.enabled = false;
        }
        
        //StompObj.SetActive(false);
        if(StegoSwipe)
        {
            StegoSwipe.SetActive(false);
        }
        
    }


    public void ChargeSpecial()
    {
        //reloadImage.color = Color.yellow;
        reloadImage.fillClockwise = true;

        switch(controller.currentDino)
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
            //Put charge particle on barrel
            specialTimer += Time.deltaTime;
            reloadImage.fillAmount = (specialChargeTime - specialTimer) / specialChargeTime;
            if(chargeParticles)
            {
                chargeParticles.SetActive(true);
                //Instantiate(chargeParticles, chargeTrigger);
                if(chargeParticles.GetComponentInChildren<Light>())
                {
                    chargeParticles.GetComponentInChildren<Light>().intensity += specialTimer * 0.01f;
                }
                
            }
            
        }
       

        else if (specialTimer > specialChargeTime)
        {
            FireSpecial();
        }
    }
    
    public void ResetSpecial()
    {
        
        if (chargeParticles)
        {
            if (chargeParticles.GetComponentInChildren<Light>())
            {
                chargeParticles.GetComponentInChildren<Light>().intensity = 0f;
                chargeParticles.GetComponentInChildren<Light>().color = Color.green;
                chargeParticles.GetComponentInChildren<Light>().range = 3f;
            }
            chargeParticles.SetActive(false);
        }        
        specialTimer = 0;
    }

    public void FireSpecial()
    {
       // print("Special Fired!");
        ResetSpecial();
        loadoutPanel.SpecialAttackClicked();
        //chargeParticles.SetActive(false);
    }

    //Lunge Special - Triceratops
    public void Lunge(Animator _dinoAnimator)           // Takes in an animator for the dino
    {
        if (timerLunge >= coolDownLunge)                // if lunge timer is greater or equal to my lunge cool down
        {            
            body.mass = 1000;                            // body mass is now 1000          
            if (animator) animator.enabled = true;       // if i have an animator, enable it
            EnableInput(false);                          // and disable input
            // GameObject effect = Instantiate(lungeEffect, transform);
            lunge = true;
            _dinoAnimator.SetTrigger("SpecialAttack");                                      //Set my dino animator trigger named Special attack

            animator.SetInteger("Dinotype", (int)controller.thisTank);
            if (animator) animator.SetTrigger("Lunge");                                     // damage triggered from animation 
            loadoutPanel.specialButton.GetComponent<Animator>().SetBool("IsReady", false);
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
			specialHasFired = true;
        }
    }

    //Sonic Boom - Duckbill
    public void SonicBoom(Animator _dinoAnimator) // Duckbill
    {
        Debug.Log("SonicBoom");
        //EnableInput(false);
        if (timerSonicBoom >= coolDownSonicBoom)
        {
            if (animator) animator.enabled = true;
            sonicboom = true;
            EnableInput(false);
            _dinoAnimator.SetTrigger("SpecialAttack");
            //animator.SetInteger("Dinotype", (int)controller.thisTank);
            if (animator) animator.SetTrigger("SonicBoom");
            if (loadoutPanel.specialButtonAnimator)
                loadoutPanel.specialButtonAnimator.SetBool("IsReady", false);
            loadoutPanel.SetSpecialGreyOutBadge(controller.currentDino);
            specialHasFired = true;
        }
    }


    //Stomp Special -// Bronto // bronto 2 
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
            //StartCoroutine(Shake.ShakeCamera(2f, severety.severe));
        }
    }

    //Swipe Special - Stego// kentrosourus
    public void Swipe(Animator _dinoAnimator)       // Takes in an animator for the dino
    {
        if (timerSwipe >= coolDownSwipe)
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
        }
    }    

    //Bite Special -TRex
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
        }
    }


    void Update()
    {
        if (!APP.IsReady)
            return;

        // bounce out if this tank is AI 
        if (controller.isAI )
            return;
  
        if (!loadoutPanel || !reloadImage) // nothing to update 
        {
            loadoutPanel = FindObjectOfType<Loadoutpanel>();    //Find the loadout panel and if not null, refill image
            if (loadoutPanel)
                reloadImage = loadoutPanel.fillBarImage;
        }
        if (loadoutPanel && !loadoutPanel.specialButton)
        {
            loadoutPanel.specialButton = loadoutPanel.transform.parent.Find("SpecialAttackBTN").GetComponent<Button>();      
        }

        if (lunge)      //If lunge is true
        {
            lunge = false;
            timerLunge = 0;
			specialHasFired = true;
            reloadComplete = false;
            //Instantiate(lungeEffect, lungeTrigger.position, lungeTrigger.rotation, transform.parent);
            StartCoroutine(LungeAnimation());
        }
        else if(controller.currentDino == dino.Tricera)
        {
            timerLunge += Time.deltaTime;
            if (timerLunge >= coolDownLunge) // restoring 
            {              
                timerLunge = coolDownLunge;
                reloadComplete = true;
                if (reloadImage)
                {
                    reloadImage.fillAmount = coolDownLunge;
                    reloadImage.color = Color.green;
                }

                if (loadoutPanel.specialButton.isActiveAndEnabled)
                {
                    if (loadoutPanel.specialButtonAnimator)
                        loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                    loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
					specialHasFired = false;

                }

            }
            else
            {
                //reloadImage.color = Color.yellow;
                reloadImage.fillClockwise = true;
                reloadImage.fillAmount = timerLunge / coolDownLunge;
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


        //if swipe is true
        if (swipe)
        {
            swipe = false;
            timerSwipe = 0;
            specialHasFired = true;
            reloadComplete = false;
            //Instantiate(stompTrigger, stopTriggerPosition.position, stopTriggerPosition.rotation, transform.parent);
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
                        loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                    loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                    specialHasFired = false;
                }

            }
            else
            {
                //reloadImage.color = Color.yellow;
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


        if (stomp)      //If stomp is true
        {
            stomp = false;
            timerStomp = 0;
            specialHasFired = true;
            reloadComplete = false;
            //Instantiate(StompObj, stopTriggerPosition.position, stopTriggerPosition.rotation);
            Instantiate(StompObj, controller.transform.position, controller.transform.rotation);
            //StartCoroutine(StompAnimation());
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
                        loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                    loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                    specialHasFired = false;
                }
            }
            else
            {
                //reloadImage.color = Color.yellow;
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

        //if bite is true
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
                            loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                        loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                        specialHasFired = false;
                    }
                }

            }
            else
            {
                //reloadImage.color = Color.yellow;
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


        //if sonicboom is true
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
                //reloadImage.fillAmount = 0;
                //animator.enabled = false;
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
                            loadoutPanel.specialButtonAnimator.SetBool("IsReady", true);
                        loadoutPanel.SetSpecialAttackBadge(controller.currentDino);
                        specialHasFired = false;
                    }
                }
            }
            else
            {
                //reloadImage.color = Color.yellow;
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

    public void StompAnimTrigger()
    {
        StopCoroutine("StopDamageAfterJump");
        StartCoroutine("StopDamageAfterJump");
    }

    #region Animation Events
    // called by animation through the TankController
    public void SwitchOffAnimatorFromTrigger()
    {
       // Debug.Log("SwitchOffTriggers mass " + controller.gameObject.GetComponent<Rigidbody>().mass + " to " + massBackup);
        body.mass = massBackup;
        body.drag = dragBckup;
        body.angularDrag = angularDragBackup;
      //  controller.GetComponentInChildren<MeshCollider>().enabled = true;
        if (animator) animator.enabled = false;
        EnableInput(true);    
    }

    // called by animation through the TankController
    public void SwipeAnimTrigger()
    {
        float swipeRadiusfloat = swipeRadiusCollider.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(swipeRadiusCollider.transform.position, swipeRadiusfloat);
        //Instantiate(swipeEffect, swipeRadiusCollider.transform.position, swipeRadiusCollider.transform.rotation);

        //Instantiate(tailEffect, swipeTrigger.transform.position, swipeTrigger.transform.rotation);  /*swipeTrigger.transform.position + new Vector3(0f, 0f, 10f)*//*, swipeTrigger.transform.localRotation);*/
        //Instantiate(afterEffect, swipeTrigger.transform.position, swipeTrigger.transform.rotation, transform.parent);

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
                    //hitOnce = true;
                }
            }
        }
        hitOnce = false;
    }

    // called by animation through the TankController
    public void SonicBoomAnimTrigger()
    {
        //float sonicboomRadiusfloat = SonicBoomRange.GetComponent<BoxCollider>().size.x;

        //Collider[] colliders = Physics.OverlapBox(SonicBoomRange.GetComponent<BoxCollider>().bounds.center, SonicBoomRange.GetComponent<BoxCollider>().bounds.size, SonicBoomRange.transform.localRotation);
        //Collider[] colliders = Physics.OverlapBox(SonicBoomRange.GetComponent<BoxCollider>().bounds.center, SonicBoomRange.GetComponent<BoxCollider>().bounds.size);

        float sonicboomRadiusfloat = sonicBoomRange.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(sonicBoomRange.transform.position, sonicboomRadiusfloat);
        /*Collider[] colliders = Physics.OverlapCapsule((SonicBoomRange.GetComponent<CapsuleCollider>().transform.position - new Vector3(0f, 0f, (SonicBoomRange.GetComponent<CapsuleCollider>().height / 2 - SonicBoomRange.GetComponent<CapsuleCollider>().radius))),
            ((SonicBoomRange.GetComponent<CapsuleCollider>().transform.position + new Vector3(0f, 0f, (SonicBoomRange.GetComponent<CapsuleCollider>().height / 2 - SonicBoomRange.GetComponent<CapsuleCollider>().radius)))),
            SonicBoomRange.GetComponent<CapsuleCollider>().radius);  //Physics.OverlapSphere(SonicBoomRange.transform.position, sonicboomRadiusfloat);*/
        //Instantiate(SonicBoomEffect, stopTriggerPosition.position, stopTriggerPosition.rotation, transform.parent);
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
                    //hitOnce = true;
                }
            }
        }
        hitOnce = false;
        /*foreach (Collider hit in colliders) //For every collider that my overlapshere hit, do this..
        {
            // if we are not the player 
            if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject != controller.gameObject)
            {
                // if we have a rigid body
                if (hit && hit.GetComponent<Rigidbody>())
                {
                    Rigidbody hitBody = hit.GetComponent<Rigidbody>();
                    hitBody.mass /= 5;
                    hitBody.isKinematic = false;
                    hitBody.AddExplosionForce(explosionForceSonicBoom, hit.gameObject.transform.position, sonicboomRadiusfloat, upwardModifier);
                    Instantiate(lungeEffect, hit.gameObject.transform.position, lungeRadius.transform.rotation);
                }

                // if we hit an enemy tank
                if (hit && hit.gameObject.tag == "playerTank")
                {
                    RTCTankController hitTank = hit.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();

                    if (hitTank)
                    {
                        hitTank.TakeDamage(sonicboomDamage, this.gameObject);

                        if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }

                if (hit.gameObject.transform.parent && hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>())
                {
                    hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy>().TakeDamage(sonicboomDamage, this.gameObject);
                }
            }
        }*/
    }

    // called by animation through the TankController
    public void LungeAnimTrigger()
    {
        float lungeRadiusfloat = lungeRadius.GetComponent<SphereCollider>().radius;
        Collider[] colliders = Physics.OverlapSphere(lungeRadius.transform.position, lungeRadiusfloat);
       // Instantiate(lungeEffect, lungeRadius.transform.position, lungeRadius.transform.rotation);
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
                        // TODO, addd interpolated damage amout AND fire explosion particle on hit enemy.
                        hitTank.TakeDamage(lungeDamage, this.gameObject);
                        hitOnce = true;
                        if (ArcadeController.instance && ArcadeController.instance.hasTankDied)
                            if (levelManagerRef != null)
                                levelManagerRef.AddKill(levelManagerRef.xpCollected);
                    }
                }
				if (hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy> ()) {
					hit.gameObject.transform.parent.gameObject.GetComponent<TargetToDestroy> ().TakeDamage (lungeDamage, this.gameObject);
                    //hitOnce = true;
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
                        // TODO, addd interpolated damage amout
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
                    //hitOnce = true;
                }
            }
        }
        hitOnce = false;
    }
    #endregion

    IEnumerator BiteEffectFadeAndDelay(GameObject _effect)
    {
        if (_effect.GetComponentInChildren<SpriteRenderer>() != null)
        {
            SpriteRenderer spriteRendere = _effect.GetComponentInChildren<SpriteRenderer>();
            Color col = spriteRendere.color;
            col.a = 1f;

            _effect.GetComponentInChildren<SpriteRenderer>().color = col;

            while (true)
            {
                // col = fade.color;
                col.a -= 0.02f;
                spriteRendere.color = col;

                yield return new WaitForSeconds(0.005f);

                if (spriteRendere.color.a <= 0)
                {
                    Destroy(_effect);

                    break;
                }
            }
        }
        

    }


    IEnumerator StopDamageAfterJump()
    {
        Instantiate(stompWave, stopTriggerPosition.position, stopTriggerPosition.rotation);

        if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
        {
            controller.photonView.RPC("PlayDinoGrowl", PhotonTargets.All, (int)controller.currentDino, transform.position);
        }
        else
            AudioManager.PlayDinoGrowl(dino.Bronto,transform.position);
         
      
        yield return new WaitForSeconds(stompEffectDelay);

        Collider[] colliders = Physics.OverlapSphere(transform.parent.position, stompRadius);
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
                    hitBody.AddExplosionForce(explosionForce, transform.position, stompRadius, upwardModifier);
                    Instantiate(stompDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);
                }
                if (hit && (hit.gameObject.tag == "playerTank" || hit.gameObject.tag == "Player")) // we hit an enemy tank.
                {
                    RTCTankController hitTank = hit.gameObject.transform.parent.gameObject.GetComponent<RTCTankController>();
                    Instantiate(stompDamageEffect, hit.gameObject.transform.position, hit.gameObject.transform.rotation);

                    if (hitTank)
                    { 
                        // TODO, addd interpolated damage amout
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

    IEnumerator BiteAnimation()
    {
        BiteObj.SetActive(true);
        BiteAnim.enabled = true;
        BiteAnim.Play("BiteAnimation");
        yield return new WaitForSeconds(0.5f);
        BiteAnim.enabled = false;
        BiteObj.SetActive(false);
        
    }

    IEnumerator LungeAnimation()
    {
        LungeObj.SetActive(true);
        LungeNewAnim.enabled = true;
        LungeNewAnim.Play("BiteAnimation");
        yield return new WaitForSeconds(0.5f);
        LungeObj.SetActive(false);
        LungeNewAnim.enabled = false;
    }

    //IEnumerator StompAnimation()
    //{
    //    StompObj.SetActive(true);
    //    //StompAnim.Play("StompAnimation");
    //    //yield return new WaitForSeconds(1);
    //    //Instantiate(stompTrigger, stopTriggerPosition.position, stopTriggerPosition.rotation, transform.parent);
    //    Instantiate(sonicBoomEffect, stompTriggerPosition.position, stompTriggerPosition.rotation, transform.parent);
    //    StompObj.SetActive(false);
    //}

    IEnumerator StegoSwipeAnimation()
    {
        StegoSwipe.SetActive(true);
        StegoSwipeAnim.Play("SwipeStego");
        yield return new WaitForSeconds(0.4f);
        StegoSwipe.SetActive(false);
    }

    void EnableInput(bool _enable)
    {
        // controller.canControl = _enable;
        controller.enabled = _enable;
       // controller.GetComponentInChildren<MeshCollider>().enabled = _enable;
        
        if(controller.GetComponentInChildren<RTCTankGunController>())
        controller.GetComponentInChildren<RTCTankGunController>().enabled = _enable;

        if (!_enable)
        {
            body.drag = 10;
            body.angularDrag = 10;

            controller.gasInput = 0;
            controller.steerInput = 0;
        }
    }

    public void SetOwner(GameObject _owner)
    {
        // controller is the owner in this case
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
}
