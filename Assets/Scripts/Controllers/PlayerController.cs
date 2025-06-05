using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerController : BaseController
{
    private NavMeshAgent _agent;
    public DungeonData dungeonData;
    MoveInDungeon moveInDungeon;
    bool isMoving = false;
    bool isChasing = false;
    public float detectionRadius = 1f;
    public LayerMask enemyLayer; // Enemy 레이어만 탐지
    [SerializeField] private List<GameObject> detectedEnemies = new List<GameObject>();
    public GameObject chaseTarget;
    public GameObject attackTarget;
    private DungeonSetEnemy dungeonEnemy;
    private int chaseIndex = 0;
    public override void Init()
    {
        moveInDungeon = GetComponent<MoveInDungeon>();
        dungeonEnemy = GameManager.Instance.Dungeon.setEnemy;
        _agent = GetComponent<NavMeshAgent>();
        moveInDungeon.playerController = this;
        State = Define.State.Idle;
    }

    public void SetDungeonData(DungeonData data)
    {
        dungeonData = data;
        moveInDungeon.SetDungeon(data._startPoint, data._sortedRooms, data._rectRooms);
        chaseIndex = 0;
        State = Define.State.Move;
    }
    protected override void UpdateMove()
    {
        moveInDungeon.StartPlayerMove();
    }

    protected override void UpdateChase()
    {
        // 1. 유효한 적 리스트가 비었을 경우 재탐색
        if (detectedEnemies == null || detectedEnemies.Count == 0)
        {
            detectedEnemies = dungeonEnemy.GetEnemiesInRoom(transform.position);
            detectedEnemies.RemoveAll(e => e == null); // 바로 정리
            chaseIndex = 0;

            if (detectedEnemies.Count == 0)
            {
                State = Define.State.Move;
                return;
            }
        }

        // 2. 유효하지 않은 인덱스일 경우 종료
        if (chaseIndex >= detectedEnemies.Count)
        {
            State = Define.State.Move;
            return;
        }

        // 3. 현재 타겟이 null이면 다음으로 넘어감
        if (detectedEnemies[chaseIndex] == null)
        {
            chaseIndex++;
            return;
        }

        // 4. 타겟 지정 및 추적
        chaseTarget = detectedEnemies[chaseIndex];
        _agent.SetDestination(chaseTarget.transform.position);

        // 5. 도착 확인
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            // 거리에 따라 도착 판정
            chaseIndex++;
        }
    }


    int CheckNull()
    {
        int count = 0;
        if(detectedEnemies != null && detectedEnemies.Count > 0)
        {
            foreach(var value in detectedEnemies)
            {
                if (value != null)
                    count++;
            }
        }
        Debug.Log(count);
        return count;
    }

    protected override void UpdateAttack()
    {
        Destroy(attackTarget);

        // 리스트에서 제거
        detectedEnemies.Remove(attackTarget);

        // 다음 상태 결정
        if (CheckNull() > 0)
        {
            State = Define.State.Chase;
        }
        else
        {
            State = Define.State.Move;
        }
    }

    bool IsEnemyNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
        {
            attackTarget = hits[0].gameObject;
        }
        else
        {
            attackTarget = null; 
        }
        return hits.Length > 0;
    }

    protected override void Update()
    {
        base.Update();
        if (IsEnemyNearby() && State == Define.State.Chase)
        {
            State = Define.State.Attack;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}