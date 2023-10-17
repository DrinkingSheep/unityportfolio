using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerMove : MonoBehaviour,ISave //PlayerController
{
    [SerializeField] float moveSpeed;
    [SerializeField] bool isGround;
    [SerializeField] Transform projectile;
    [SerializeField] AudioClip[] footSteps;

    
    [SerializeField] ParticleSystem myPs;

    [SerializeField] HealthBar healthBar;
    [SerializeField] StaminaBar staminaBar;

    [Header("------------------Sound-----------------")]
    [SerializeField] SoundChannel playerSoundChannel;

    public SFXData footSoundSFXs;
    public SFXData DashSFX;


    [Header("------------------PlayerSpec-----------------")]
    //컴포넌트
    Transform myTr;
    Animator myAnim;
    Rigidbody2D myRigid;
    AudioSource myAudio;
    SpriteRenderer[] myRenders;

    //각종 변수
    Vector2 moveDir;
    int dir; // 1 or -1. 플레이어 방향전환.
    const float playerScale = 1f; //플레이어 크기 조절.

    bool isDead;
    bool isMoving;

    [SerializeField] float dashSpeed = 4;
    [SerializeField] float dashTime = .5f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] Transform footPoint;
    float dashCounter; //대시 지속시간
    float dashCoolCounter; //대시 쿨타임.



    bool isDashEnabled = false;
    bool isDashing = false;
    TrailRenderer tr;


    Vector2 moveInput;
    float activeMoveSpeed;

    Transform bodyTr;

    public float startingHP;
    public float startingStamina;

    public float hp;
    public float stamina;

    public int numOfKey = 0;
    public int numOfCoin = 0;

    GameObject eKeyUI;
    void Start()
    {
        myTr = transform;
        myAnim = GetComponentInChildren<Animator>(); //컨트롤러가 자식인 body에 있다.        
        myRigid = GetComponent<Rigidbody2D>();
        isDashEnabled = SkillManager.instance.dash.isEnabled;
        myAudio = GetComponent<AudioSource>();
        myPs = GetComponent<ParticleSystem>();
        myRenders = GetComponentsInChildren<SpriteRenderer>();
        tr = GetComponent<TrailRenderer>();

        activeMoveSpeed = moveSpeed;

        startingHP = 100f;
        startingStamina = 100f;

        hp = startingHP;
        stamina = startingStamina;

        healthBar.SetMaxHealth(startingHP);
        staminaBar.SetMaxStamina(startingStamina);

        UIManager.instance.UpdateCoinCount(numOfCoin);
        UIManager.instance.UpdateKeyCount(numOfKey);

        //머리위의 E버튼 숨기자.
        eKeyUI = myTr.GetChild(1).gameObject; //Player 자식으로 새로 추가하거나 하면 순서가 바뀔 수 있다.
        eKeyUI.SetActive(false);

        healthBar.SetHealth(hp);
        staminaBar.SetStamina(stamina);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        PlayerAttack();
        DashPlayer();        
        OpenDoor();
        StaminaDecreasing();
        WeaponSwticher();
    }
    [SerializeField] PlayerShooter[] weapons;
    private void WeaponSwticher()
    {
                
        if (SkillManager.instance.fireBall.isEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {                
                weapons[0].gameObject.SetActive(true);
                weapons[1].gameObject.SetActive(false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {                
                weapons[0].gameObject.SetActive(false);
                weapons[1].gameObject.SetActive(true);
            }
        }
                
    }

    float staminaDecreaseCoolDown = 1f;
    float staminaTimer = 1f;
    void StaminaDecreasing()
    {
        staminaTimer -= Time.deltaTime;
        if (staminaTimer <= 0) //초기화
        {
            stamina--;
            staminaTimer = staminaDecreaseCoolDown;
            staminaBar.SetStamina(stamina);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        moveInput.Normalize();
        myRigid.velocity = moveInput * activeMoveSpeed;

        if (moveInput.x > 0.1f || moveInput.x < -0.1f)
        {
            myRigid.AddForce(new Vector2(moveInput.x * activeMoveSpeed, 0f), ForceMode2D.Impulse);
            StartCoroutine(FootStepEffect());
        }

        if (moveInput.y > 0.1f || moveInput.y < -0.1f)
        {
            myRigid.AddForce(new Vector2(0f, moveInput.y * activeMoveSpeed), ForceMode2D.Impulse);
            StartCoroutine(FootStepEffect());
        }

        SetMoveAnimation();

        FlipPlayer(moveInput.x);
    }

    IEnumerator FootStepEffect()
    {
        if (this.footSoundSFXs)
        {
            this.playerSoundChannel.PlaySFX(this.footSoundSFXs);
        }
        yield return null;
        FootDust();
    }


    void FlipPlayer(float key)
    {
        if (key == 0) return;

        dir = (key > 0) ? -1 : 1;
        Vector3 scale = transform.GetChild(0).localScale;
        scale.x = dir * playerScale;
        scale.y = playerScale;
        transform.GetChild(0).localScale = scale;
    }
    void FootSound(int kind)
    {

    }

    void FootDust()
    {
        footPoint.GetComponent<FootStep>().FootDust();
    }
    void SetMoveAnimation()
    {
        float speed = Mathf.Sqrt(Mathf.Sqrt(Mathf.Pow(moveInput.x, 2) + Mathf.Pow(moveInput.y, 2)));
        if (myAnim != null) myAnim.SetFloat("playerSpeed", speed);
        else Debug.Log("Player Animator Null");
    }
    void PlayerDead()
    {
        isDead = true;
    }

    void PlayerAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            myAnim.SetTrigger("attack");
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {

        }
        else if (Input.GetKeyDown(KeyCode.C))
        {

        }
    }


    void DashPlayer()
    {

        if (Input.GetKeyDown(KeyCode.Space) && SkillManager.instance.dash.isEnabled)
        {
            isDashing = true;
            //print("dash");

            if (dashCoolCounter <= 0 && dashCounter <= 0)
            {
                activeMoveSpeed = dashSpeed;
                dashCounter = dashTime;
            }
        }

        if (dashCounter > 0)
        {
            dashCounter -= Time.deltaTime;
            if (dashCounter <= 0) //초기화
            {
                activeMoveSpeed = moveSpeed;
                dashCoolCounter = dashCooldown;
            }
        }

        if (dashCoolCounter > 0)
        {
            dashCoolCounter -= Time.deltaTime;
        }

    }
    //Enemy와 물리충돌 안나게 해서 무적판정
    public void onDamage(float damage)
    {
        if (!GameManager.instance.playerInvincible)
        {
            hp -= damage;
        }
        GameManager.instance.playerInvincible = true;
        //gameObject.layer = 10; //layer:PlayerDamaged


        healthBar.SetHealth(hp);
        foreach (var myRender in myRenders)
        {
            myRender.color = new Color(1, 1, 1, 0.4f);
        }
        Invoke("OffDamaged", 1.2f);
    }
    public void OffDamaged()
    {
        GameManager.instance.playerInvincible = false;
        foreach (var myRender in myRenders)
        {
            myRender.color = new Color(1, 1, 1, 1f);
        }
    }

    public void RestoreHealth(float newHealth)
    {
        if (isDead) return;

        if (hp + newHealth < startingHP)
        {
            hp += newHealth;
        }
        else if (hp + newHealth > startingHP)
        {
            hp = startingHP;
        }

        healthBar.SetHealth(hp);
    }

    public void RestoreStamina(float newStamina)
    {
        if (isDead) return;
        if (stamina + newStamina < startingStamina)
        {
            stamina += newStamina;
        }
        else if (stamina + newStamina > startingStamina)
        {
            stamina = startingStamina;
        }
        staminaBar.SetStamina(stamina);
    }
    void PlayerKnokback()
    {

    }

    Door door; //문에 닿았을 때 문 정보를 저장하는 변수.

    private void OnTriggerEnter2D(Collider2D coll)
    {
        //print(coll.tag);
        if (coll.transform.CompareTag("Key"))
        {
            numOfKey++;
            UIManager.instance.UpdateKeyCount(numOfKey);
            Destroy(coll.gameObject);
        }
        if (coll.transform.CompareTag("Coin"))
        {
            numOfCoin++;
            UIManager.instance.UpdateCoinCount(numOfCoin);
            Destroy(coll.gameObject);
        }
        /*
        if (coll.transform.CompareTag("DoorCensor"))//해당 door가 잠긴 상태라면 UI띄우기. 
        {
            if (numOfKey > 0) eKeyUI.SetActive(true);
            door = coll.transform.GetComponentInParent<Door>();

        }
        */
      
    }

    void OpenDoor()
    {
        if (Input.GetKeyDown(KeyCode.E) && numOfKey > 0 && eKeyUI.activeSelf && door != null)
        {

            door.SetDoor(Door.Status.Open);
            numOfKey--;
            UIManager.instance.UpdateKeyCount(numOfKey);
            eKeyUI.SetActive(false);
        }
       
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.transform.CompareTag("DoorCensor"))
        {
            eKeyUI.SetActive(false);
            door = null;
        }
    }


    // 세이브 로드할 데이터 정해주기 ->세이브매니저에서 해당 함수 일괄 호출해서 저장/로드 함.
    public void LoadData(GameData data)
    {
        this.numOfCoin = data.coinCount;
        this.numOfKey = data.keyCount;
        this.stamina = data.playerStamina;
        this.hp = data.playerHp;
    }

    public void SaveData(ref GameData data)
    {
        data.coinCount = this.numOfCoin;
        data.keyCount = this.numOfKey;
        data.playerHp = this.hp;
        data.playerStamina = this.stamina;
    }
}
