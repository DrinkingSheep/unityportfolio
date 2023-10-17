using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Boss : MonoBehaviour
{
    public enum States
    {
        Idle,        
        Attack1,
        Attack2,
        Attack3,
        Groggy,
        Die        
    }
    [SerializeField] AudioClip[] hitSFXs;
    AudioSource myAudio;
    StateMachine <States, StateDriverUnity> fsm;
    Transform player;
    [SerializeField]GameObject nextDoor;
    [SerializeField]Transform[] bossPattern1Positions;
    int startIdx;
    [SerializeField] float maxHp = 2000;
    float hp;
    [SerializeField] HealthBar healthBar;
    public bool isDead;

    [SerializeField]BossGroggyBar bossGroggyBar;
    Room room;
    private void Awake()
    {
        fsm = new StateMachine<States, StateDriverUnity>(this);        
    }
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        fsm.ChangeState(States.Idle);
        startIdx = Random.Range(0, bossPattern1Positions.Length); //플레이어 공격 시 보스 처음위치 결정.
        hp = maxHp;
        nextDoor.SetActive(false);
        healthBar.SetMaxHealth(maxHp);
        myAudio = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        room = GetComponentInParent<Room>();
        bossGroggyBar.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf) fsm.Driver.Update.Invoke();
        if (bossGroggyBar.isFull) fsm.ChangeState(States.Groggy);
        if (hp <= 0) fsm.ChangeState(States.Die);
        if (room.HasPlayer())
        {
            bossGroggyBar.gameObject.SetActive(true);
            healthBar.gameObject.SetActive(true);
        }
       
    }

    void Idle_Enter()
    {
        //Debug.Log("Idle_Enter");
    }
    void Idle_Update() //state change 관련해서 기술하기.
    {
        //플레이어가 맵에 들어갔을때부터 공격모드or 컷씬 끝나고부터 공격모드.
        
        if (room != null)
        {
            if (room.HasPlayer())
            {
                fsm.ChangeState(States.Attack1);
            }
        }
        else return;               
    }

    void Attack1_Enter()
    {
        //Debug.Log("Attack1_Enter");
        StartCoroutine(MoveAndAttack());
    }
    void Attack1_Update()
    {
        //Debug.Log("Attack1_Update");
        //hp 70퍼센트 아래로 내려가면 패턴2
        if (hp/maxHp<0.6) fsm.ChangeState(States.Attack2);
        else if(hp/maxHp<0.5) fsm.ChangeState(States.Groggy);
        
    }
    void Attack1_Exit()
    {
        StopAllCoroutines();
    }

    void Attack2_Enter()
    {
        //Debug.Log("Attack2_Enter");
    }
    void Attack2_Update()
    {
        //Debug.Log("Attack2_Update");
        //hp 50퍼센트 이하로 내려가면 그로기 상태 진입
        if (hp/maxHp<0.5) fsm.ChangeState(States.Groggy);
        else if (hp <= 0)
        {
            fsm.ChangeState(States.Die);
        }
    }
    void Groggy_Enter()
    {
        //Debug.Log("Groggy_Enter");
        //그로기 애니메이션..
        //StartCoroutine(MoveToCenter());
      
        StartCoroutine(GroggyAnim());
    }
    void Groggy_Update()
    {
        //Debug.Log("Groggy_Update");
        //hp 20퍼센트 이하로 내려가면 공격패턴 3
        if (hp/maxHp<0.2)
        {
            fsm.ChangeState(States.Attack3);
        }
    }
    void Groggy_Exit()
    {
        StopAllCoroutines();
    }
    void Attack3_Enter()
    {
        //Debug.Log("Attack3_Enter");
    }
    void Attack3_Update()
    {
        
    }
    void Die_Enter()
    {                
        nextDoor.SetActive(true);
        bossGroggyBar.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);

        gameObject.SetActive(false);
        isDead = true;

        //골드 많이 뿌리기. 보스는 확정으로 7개의 아이템을 랜덤하게 뿌린다. 주로 골드.
    }

    IEnumerator DeadRoutine()
    {
        gameObject.SetActive(false); //Dead Anim으로 교체 하자.
        
        yield return new WaitForSeconds(1f);
    }

    float radius = 0.2f;

    IEnumerator MoveToCenter()
    {
        Room room = GetComponentInParent<Room>();
        var tween = transform.DOMove(room.GetRoomCentre(),0.5f);
        yield return tween.WaitForCompletion();
    }

    IEnumerator GroggyAnim()
    {
        Room room = GetComponentInParent<Room>();
        var tween_moveToCenter = transform.DOMove(room.GetRoomCentre(), 0.5f);
        yield return tween_moveToCenter.WaitForCompletion();


        float movingAmount = 0.3f;
        while (true)
        {
            var tween = transform.DOMove(transform.position+Vector3.up* movingAmount, 1f);
            yield return tween.WaitForCompletion();
            var tweenEnd = transform.DOMove(transform.position - Vector3.up * movingAmount, 1f);
            yield return tweenEnd.WaitForCompletion();
        }
    }
    float timer;
    float atk2BetTime = 6.5f;
    IEnumerator MoveAndAttack()
    {        
        int i = 0;
        timer = atk2BetTime; 
        while (true)
        {            
            i++;
            var tween = transform.DOMove(radius * Random.insideUnitCircle + (Vector2)bossPattern1Positions[(startIdx + i) % bossPattern1Positions.Length].position, 1f);
            yield return tween.WaitForCompletion();
            StartCoroutine(Attack1());
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                var tweenPat2Move = transform.DOMove(radius * Random.insideUnitCircle + (Vector2)GetComponentInParent<Room>().GetRoomCentre(), 1f);
                yield return tween.WaitForCompletion();
                timer = atk2BetTime;
            }

            yield return new WaitForSeconds(1f);
        }        
    }

    int shotCnt = 12;
    float bulletSpeed = 200f;
    IEnumerator Attack1()
    {
        float angle = 360 / shotCnt;
        
        for (int i = 0; i < shotCnt; i++)
        {
            GameObject bossBullet = ObjectManager.instance.PopFromPool("BossBullet");
            bossBullet.SetActive(true);
            bossBullet.transform.position = transform.position;
            bossBullet.transform.rotation = Quaternion.identity;    
            bossBullet.transform.Rotate(new Vector3(0f,0f,angle*i));            
        }           
        yield return new WaitForSeconds(1f);
    }
    IEnumerator Attack2()
    {
        float angle = 360 / shotCnt;

        for (int i = 0; i < shotCnt; i++)
        {
            GameObject bossBullet = ObjectManager.instance.PopFromPool("BossBullet");
            bossBullet.SetActive(true);
            bossBullet.transform.position = transform.position;
            bossBullet.transform.rotation = Quaternion.identity;
            bossBullet.transform.Rotate(new Vector3(0f, 0f, angle * i));
            yield return new WaitForSeconds(0.08f);
        }
        yield return new WaitForSeconds(1f);
    }
    public void OnDamage(float damage)
    {
        hp -= damage;
        healthBar.SetHealth(hp);
        myAudio.PlayOneShot(hitSFXs[Random.Range(0,hitSFXs.Length)]);
    }
   public void DropItem()
    {
        int rand = Random.Range(0, 10);
        if (rand<10) //100퍼센트 확률로
        {
            GameObject groggyObject = ObjectManager.instance.PopFromPool("BossGroggyObject");
            groggyObject.SetActive(true);
            groggyObject.transform.position = transform.position;
            groggyObject.transform.rotation = Quaternion.identity;
            //노랑 오브젝트 떨구기.
        }
    }
}
