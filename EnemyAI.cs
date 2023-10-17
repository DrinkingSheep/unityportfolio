using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.AI;
using System;
using DG.Tweening;
public abstract class EnemyAI : MonoBehaviour,IDamagable
{
    [SerializeField] protected EnemyData enemyData;
    [SerializeField] protected AudioClip[] hitSFXs;


    protected LayerMask obstacle;
    protected AIPath myAgent;
    protected Seeker mySeeker;

    protected Animator myAnim;
    protected Transform target;
    protected GameObject player;
    protected Vector3 targetPos;

    protected float startingHp;
    protected float hp;
    protected float damage;

    protected bool isDead;
    protected bool isAttack;

    protected AudioSource myAudio;
    protected bool chooseDir = false;
    protected Transform myTr;
    public enum State
    {
        Idle, Chase, Die, Attack, Hurt
    }

    public State activeState = State.Idle;


    protected virtual void OnEnable()
    {   //필수 세팅

        player = GameObject.FindGameObjectWithTag("Player");
        target = player.transform;
        myAnim = GetComponentInChildren<Animator>();
        myAgent = GetComponent<AIPath>();

        //
        myTr = transform;
        myAgent.maxSpeed = enemyData.MoveSpeed;
        obstacle = 1 << LayerMask.NameToLayer("TestLayer");

        activeState = State.Idle;
        myAudio = GetComponent<AudioSource>();


    }



    protected void Update()
    {
        EnemyGFX();

        switch (activeState)
        {
            case State.Idle:
                EnemyPatrol();
                break;
            case State.Chase:
                ChasePlayer();
                break;
            case State.Attack:
                AttackPlayer();
                break;
            case State.Die:
                EnemyDie();
                break;
            case State.Hurt:
                EnemyHurt();
                break;
            default:
                break;
        }
    }

    void EnemyGFX()
    {
        if (myAgent.desiredVelocity.x >= 0.01f)
        {
            myTr.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (myAgent.desiredVelocity.x <= -0.01f)
        {
            myTr.localScale = new Vector3(1f, 1f, 1f);
        }
        else return;
    }
    private void EnemyHurt()
    {

    }

    private void AttackPlayer()
    {
        if (!isPlayerInRange(enemyData.SightRange)) activeState = State.Chase; //멀어지면 ?게끔.

        if (isAttack) return;

        StartCoroutine(AttackRoutine());

    }

    protected Vector2 hitBoxSize;

    float timeBetAtk = 0.3f;

    IEnumerator AttackRoutine()
    {
        //print(isAttack);
        myAnim.SetTrigger("attack");

        isAttack = true;

        float dist = Vector3.Distance(transform.position, target.position);
        targetPos = target.position;


        yield return new WaitForSeconds(0.35f);

        isAttack = false;
        player.GetComponent<PlayerMove>().onDamage(enemyData.Damage);
    }
    protected virtual void ChasePlayer()
    {

        float dist = Vector3.Distance(transform.position, target.position);
        targetPos = target.position;
        targetPos.z = 0;

        if (!myAgent.enabled) myAgent.enabled = true;

        if (dist < enemyData.SightRange)
        {
            activeState = State.Attack;
        }

        else if (isPlayerInRange(enemyData.SearchRange) && activeState != State.Die && !isPlayerInRange(enemyData.SightRange))
        {
            UpdateDestination();
            activeState = State.Chase;
        }
    }
    protected void UpdateDestination()
    {
        myAgent.destination = target.position;
    }
    protected bool isPlayerInRange(float range)
    {
        if (Vector3.Distance(player.transform.position, myTr.position) <= range) return true;
        else return false;
    }
    protected bool IsPlayerInRoom()
    {
        Room room = GetComponentInParent<Room>();
        Collider2D player = Physics2D.OverlapBox(room.transform.position, new Vector2(room.Width, room.Height), 0, 1 << 8);
        //print("Player is :" + player);
        if (player != null) return false;
        else return true;
    }
    protected void EnemyPatrol()
    {
        //플레이어가 방에 없으면 idle 유지. 방에 있으면 chase로 전이.
        if (!IsPlayerInRoom())
        {
            activeState = State.Idle;
        }
        else
        {
            activeState = State.Chase;
        }
    }
    protected virtual void EnemyDie()
    {
        Destroy(gameObject);
    }

    protected virtual void RestoreHealth(float newHp)
    {
        if (isDead)
        {
            return;
        }
        hp += newHp;
    }


    void OnDamageGFX()
    {
        SpriteRenderer[] spriteRendrers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer spriteRendrer in spriteRendrers)
        {
            spriteRendrer.DOColor(Color.blue, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }

    public virtual void OnDamage(float damage, Vector2 hitPoint, bool isCriticalHit)
    {
        if (isCriticalHit) damage *= UnityEngine.Random.Range(1.5f, 2f); //setting의 크리티컬 minmax 값 참조하게 바꾸기.
        damage = Mathf.CeilToInt(damage);
        hp -= damage;
        OnDamageGFX();
        DamagePopup.Create(myTr.position, damage, isCriticalHit);
        myAudio.PlayOneShot(hitSFXs[UnityEngine.Random.Range(0,hitSFXs.Length)]);
        if (hp <= 0 && !isDead)
        {
            activeState = State.Die;
            CameraManager.instance.ShakeCamera(0.4f, 0.6f);
            //CinemachineShake.instance.ShakeCamera(3f, 0.2f);
        }
    }

}
