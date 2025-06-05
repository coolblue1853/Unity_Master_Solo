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
    public GameObject target;
    public GameObject attackTarget;
    private DungeonSetEnemy dungeonEnemy;

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
        State = Define.State.Move;
    }

    protected override void UpdateMove()
    {
        if (!isMoving)
        {
            isMoving = true;
            isChasing = false; // Move 상태 진입 시 추적 상태 초기화
            moveInDungeon.StartPlayerMove();
        }

        // 이동 중에도 근처 적을 탐지하여 추적 상태로 전환할 수 있도록 함
        if (IsEnemyNearby())
        {
            State = Define.State.Chase;
        }
    }

    protected override void UpdateChase()
    {
        // 추적 상태에 처음 진입했거나, 기존 목표가 사라졌을 때 새로운 목표를 찾음
        if (!isChasing || target == null)
        {
            isChasing = true;
            isMoving = false; // 추적 중에는 이동 정지

            // 현재 방에 있는 적들 목록을 가져오거나 갱신합니다.
            // 주의: GetEnemiesInRoom이 어떤 방식으로 동작하는지 확인해야 합니다.
            // 이미 제거된 몬스터는 리스트에서 자동으로 제거되어야 합니다.
            detectedEnemies = dungeonEnemy.GetEnemiesInRoom(transform.position);

            // 유효한 적들만 필터링합니다. (null이 아닌 적들)
            detectedEnemies.RemoveAll(enemy => enemy == null);

            if (detectedEnemies.Count == 0)
            {
                // 현재 방에 적이 더 이상 없으면 이동 상태로 돌아갑니다.
                State = Define.State.Move;
                target = null; // 목표 제거
                return; // 함수 종료
            }

            // 가장 가까운 적을 목표로 설정하거나, 단순히 리스트의 첫 번째 적을 목표로 설정합니다.
            // 여기서는 리스트의 첫 번째 적을 사용합니다.
            target = detectedEnemies[0];
            _agent.SetDestination(target.transform.position);
        }

        // 추적 중 목표가 사라진 경우 (죽었거나 despawn)
        if (target == null)
        {
            isChasing = false; // 추적 상태 초기화
            State = Define.State.Chase; // 다시 Chase 상태로 진입하여 새 목표를 찾거나 Move로 전환
            return;
        }

        // 목표와의 거리가 NavMeshAgent의 StoppingDistance 이내이고, 아직 공격 범위 내에 들어오지 않았다면
        if (_agent.remainingDistance <= _agent.stoppingDistance && !IsEnemyNearby())
        {
            // 목표에 도달했지만 아직 공격 범위 내에 적이 없으면 다시 추적 로직을 실행하여 다음 적을 찾거나 이동 상태로 전환
            isChasing = false;
            State = Define.State.Chase;
        }
        else if (IsEnemyNearby())
        {
            // 공격 범위 내에 적이 있으면 공격 상태로 전환
            State = Define.State.Attack;
        }
    }

    protected override void UpdateAttack()
    {
        if (attackTarget != null)
        {
            // 공격 로직 (예: 데미지 계산, 애니메이션 재생)
            Debug.Log($"Attacking {attackTarget.name}");

            // 임시로 적 파괴
            Destroy(attackTarget.gameObject);

            // 공격 후, 현재 공격 대상이 사라졌으므로 detectedEnemies 리스트에서 제거합니다.
            detectedEnemies.Remove(attackTarget);
            attackTarget = null; // 공격 대상 초기화

            // 공격 후 바로 Chase 상태로 돌아가서 다음 적을 찾거나 이동 상태로 전환합니다.
            isChasing = false; // 새 목표를 찾기 위해 초기화
            State = Define.State.Chase;
        }
        else
        {
            // 공격 대상이 사라진 경우 (예: 다른 곳에서 이미 죽었거나)
            // 다시 추적 상태로 돌아가서 다음 적을 찾거나 이동 상태로 전환합니다.
            isChasing = false;
            State = Define.State.Chase;
        }
    }

    bool IsEnemyNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
        {
            // 가장 가까운 적을 공격 대상으로 설정할 수 있습니다.
            // 여기서는 단순히 첫 번째 적을 사용합니다.
            attackTarget = hits[0].gameObject;
        }
        else
        {
            attackTarget = null; // 범위 내에 적이 없으면 attackTarget을 null로 설정
        }
        return hits.Length > 0;
    }

    protected override void Update()
    {
        base.Update();
        // Update()에서 IsEnemyNearby()를 계속 호출하여 상태 전환을 결정합니다.
        // 하지만 State 머신 로직 내에서 상태 전환을 담당하는 것이 더 명확합니다.
        // 현재 로직에서는 IsEnemyNearby() 호출로 Attack 상태로 즉시 전환되는 문제가 있습니다.
        // 이 부분은 State.Chase 내부에서 공격 범위에 들어왔을 때 State.Attack으로 전환하도록 조정했습니다.
        // 따라서 Update() 내에서 직접 상태를 변경하는 코드는 삭제하는 것이 좋습니다.
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}