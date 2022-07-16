using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

// 문제1. 몬스터의 시체를 지나갈때 손에 닿은 판정이 나서 데미지
// 문제2. 오브젝트 풀링으로 몬스터가 다시 스폰될때 죽은상태로 살아남

public class MonsterCtrl : MonoBehaviour
{
    public enum State
    {
        IDLE,
        PATROL,
        TRACE,
        ATTACK,
        DIE
    }

    public State state = State.IDLE;
    public float traceDist = 10.0f;
    public float attackDist = 1.7f;
    public bool isDie = false;

    private float distance = 0.0f;

    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator anim;

    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");

    private GameObject bloodEffect;

    private int hp = 100;

    // 손에 있는 sphere collider를 담는 객체
    private SphereCollider[] handColliders;

    void OnEnable()
    {
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;

        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction());
    }

    void OnDisable()
    {
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    }

    void Awake()
    {
        monsterTr = GetComponent<Transform>();

        playerTr = GameObject.FindWithTag("PLAYER").GetComponent<Transform>();

        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;

        anim = GetComponent<Animator>();

        bloodEffect = Resources.Load<GameObject>("BloodSprayEffect");

        handColliders = gameObject.GetComponentsInChildren<SphereCollider>();

    }

    void Update()
    {
        if (agent.remainingDistance >= 2.0f)
        {
            Vector3 directon = agent.desiredVelocity;

            Quaternion rot = Quaternion.LookRotation(directon);

            monsterTr.rotation = Quaternion.Slerp(monsterTr.rotation, rot, Time.deltaTime * 10.0f);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("BULLET"))
        {
            Destroy(coll.gameObject);
            anim.SetTrigger(hashHit);

            Vector3 pos = coll.GetContact(0).point;
            Quaternion rot = Quaternion.LookRotation(-coll.GetContact(0).normal);
            ShowBloodEffect(pos, rot);

            hp -= 10;
            if (hp <= 0)
            {
                state = State.DIE;

                GameManager.instance.DisplayScore(50);
            }
        }
    }

    public void OnDamage(Vector3 pos, Vector3 normal)
    {
        anim.SetTrigger(hashHit);
        Quaternion rot = Quaternion.LookRotation(normal);

        ShowBloodEffect(pos, rot);

        hp -= 30;
        if(hp <= 0)
        {
            state = State.DIE;
            GameManager.instance.DisplayScore(50);
        }
    }
    
    void ShowBloodEffect(Vector3 pos, Quaternion rot)
    {
        GameObject blood = Instantiate<GameObject>(bloodEffect, pos, rot, monsterTr);
        Destroy(blood, 1.0f);
    }

    IEnumerator CheckMonsterState()
    {
        while (!isDie) {
            yield return new WaitForSeconds(0.3f);

            if (state == State.DIE) yield break;

            distance = Vector3.Distance(playerTr.position, monsterTr.position);

            if(distance <= attackDist)
            {
                state = State.ATTACK;
            }
            else if(distance <= traceDist) 
            {
                state = State.TRACE;
            }
            else
            {
                state = State.IDLE;
            }
            
        }
    }

    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.IDLE:
                    agent.isStopped = true;
                    anim.SetBool(hashTrace, false);
                    break;

                case State.TRACE:
                    agent.SetDestination(playerTr.position);
                    agent.isStopped = false;
                    anim.SetBool(hashTrace, true);
                    anim.SetBool(hashAttack, false);
                    break;

                case State.ATTACK:
                    anim.SetBool(hashAttack, true);
                    break;

                case State.DIE:
                    isDie = true;
                    agent.isStopped = true;
                    anim.SetTrigger(hashDie);
                    GetComponent<CapsuleCollider>().enabled = false;
                    foreach (SphereCollider coll in handColliders) coll.enabled = false;

                    yield return new WaitForSeconds(3.0f);

                    // 사망 후 부활 초기화
                    hp = 100;
                    isDie = false;
                    GetComponent<CapsuleCollider>().enabled = true;
                    foreach (SphereCollider coll in handColliders) coll.enabled = true;
                    this.gameObject.SetActive(false);
                    state = State.IDLE;

                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void OnDrawGizmos()
    {
        if(state == State.TRACE)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, traceDist);
        }
        if(state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDist);
        }
    }

    void OnPlayerDie()
    {
        StopAllCoroutines();

        agent.isStopped = true;
        anim.SetFloat(hashSpeed, Random.Range(0.8f, 1.2f));
        anim.SetTrigger(hashPlayerDie);
    }
}
