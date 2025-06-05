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
    public LayerMask enemyLayer; // Enemy ���̾ Ž��
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
            isChasing = false; // Move ���� ���� �� ���� ���� �ʱ�ȭ
            moveInDungeon.StartPlayerMove();
        }

        // �̵� �߿��� ��ó ���� Ž���Ͽ� ���� ���·� ��ȯ�� �� �ֵ��� ��
        if (IsEnemyNearby())
        {
            State = Define.State.Chase;
        }
    }

    protected override void UpdateChase()
    {
        // ���� ���¿� ó�� �����߰ų�, ���� ��ǥ�� ������� �� ���ο� ��ǥ�� ã��
        if (!isChasing || target == null)
        {
            isChasing = true;
            isMoving = false; // ���� �߿��� �̵� ����

            // ���� �濡 �ִ� ���� ����� �������ų� �����մϴ�.
            // ����: GetEnemiesInRoom�� � ������� �����ϴ��� Ȯ���ؾ� �մϴ�.
            // �̹� ���ŵ� ���ʹ� ����Ʈ���� �ڵ����� ���ŵǾ�� �մϴ�.
            detectedEnemies = dungeonEnemy.GetEnemiesInRoom(transform.position);

            // ��ȿ�� ���鸸 ���͸��մϴ�. (null�� �ƴ� ����)
            detectedEnemies.RemoveAll(enemy => enemy == null);

            if (detectedEnemies.Count == 0)
            {
                // ���� �濡 ���� �� �̻� ������ �̵� ���·� ���ư��ϴ�.
                State = Define.State.Move;
                target = null; // ��ǥ ����
                return; // �Լ� ����
            }

            // ���� ����� ���� ��ǥ�� �����ϰų�, �ܼ��� ����Ʈ�� ù ��° ���� ��ǥ�� �����մϴ�.
            // ���⼭�� ����Ʈ�� ù ��° ���� ����մϴ�.
            target = detectedEnemies[0];
            _agent.SetDestination(target.transform.position);
        }

        // ���� �� ��ǥ�� ����� ��� (�׾��ų� despawn)
        if (target == null)
        {
            isChasing = false; // ���� ���� �ʱ�ȭ
            State = Define.State.Chase; // �ٽ� Chase ���·� �����Ͽ� �� ��ǥ�� ã�ų� Move�� ��ȯ
            return;
        }

        // ��ǥ���� �Ÿ��� NavMeshAgent�� StoppingDistance �̳��̰�, ���� ���� ���� ���� ������ �ʾҴٸ�
        if (_agent.remainingDistance <= _agent.stoppingDistance && !IsEnemyNearby())
        {
            // ��ǥ�� ���������� ���� ���� ���� ���� ���� ������ �ٽ� ���� ������ �����Ͽ� ���� ���� ã�ų� �̵� ���·� ��ȯ
            isChasing = false;
            State = Define.State.Chase;
        }
        else if (IsEnemyNearby())
        {
            // ���� ���� ���� ���� ������ ���� ���·� ��ȯ
            State = Define.State.Attack;
        }
    }

    protected override void UpdateAttack()
    {
        if (attackTarget != null)
        {
            // ���� ���� (��: ������ ���, �ִϸ��̼� ���)
            Debug.Log($"Attacking {attackTarget.name}");

            // �ӽ÷� �� �ı�
            Destroy(attackTarget.gameObject);

            // ���� ��, ���� ���� ����� ��������Ƿ� detectedEnemies ����Ʈ���� �����մϴ�.
            detectedEnemies.Remove(attackTarget);
            attackTarget = null; // ���� ��� �ʱ�ȭ

            // ���� �� �ٷ� Chase ���·� ���ư��� ���� ���� ã�ų� �̵� ���·� ��ȯ�մϴ�.
            isChasing = false; // �� ��ǥ�� ã�� ���� �ʱ�ȭ
            State = Define.State.Chase;
        }
        else
        {
            // ���� ����� ����� ��� (��: �ٸ� ������ �̹� �׾��ų�)
            // �ٽ� ���� ���·� ���ư��� ���� ���� ã�ų� �̵� ���·� ��ȯ�մϴ�.
            isChasing = false;
            State = Define.State.Chase;
        }
    }

    bool IsEnemyNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
        {
            // ���� ����� ���� ���� ������� ������ �� �ֽ��ϴ�.
            // ���⼭�� �ܼ��� ù ��° ���� ����մϴ�.
            attackTarget = hits[0].gameObject;
        }
        else
        {
            attackTarget = null; // ���� ���� ���� ������ attackTarget�� null�� ����
        }
        return hits.Length > 0;
    }

    protected override void Update()
    {
        base.Update();
        // Update()���� IsEnemyNearby()�� ��� ȣ���Ͽ� ���� ��ȯ�� �����մϴ�.
        // ������ State �ӽ� ���� ������ ���� ��ȯ�� ����ϴ� ���� �� ��Ȯ�մϴ�.
        // ���� ���������� IsEnemyNearby() ȣ��� Attack ���·� ��� ��ȯ�Ǵ� ������ �ֽ��ϴ�.
        // �� �κ��� State.Chase ���ο��� ���� ������ ������ �� State.Attack���� ��ȯ�ϵ��� �����߽��ϴ�.
        // ���� Update() ������ ���� ���¸� �����ϴ� �ڵ�� �����ϴ� ���� �����ϴ�.
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}