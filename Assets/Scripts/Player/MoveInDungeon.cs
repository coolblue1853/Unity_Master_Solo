using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveInDungeon : MonoBehaviour
{
    private DungeonManager dungeonManager;
    private NavMeshAgent _agent;
    private const float BasicHight = 1.387f;
    private RectInt _currentDestinationRoom;
    private Queue<RectInt> _pathQueue;
    private bool _isMoving = false;
    private bool _isFirstRoom;

    private void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>();
        _agent.enabled = false;
        _isFirstRoom = false;
        dungeonManager = GameManager.Instance.Dungeon;
        _pathQueue = new Queue<RectInt>(dungeonManager._pathToEnd); // 원본 손상 방지

        Invoke("StartPlayerMove", 0.1f);
    }

    public void StartPlayerMove()
    {
        if (_pathQueue.Count == 0) return;

        RectInt startRoomRect = _pathQueue.Dequeue();
        transform.position = new Vector3(startRoomRect.x, BasicHight, startRoomRect.y);
        _agent.enabled = true;
        _isFirstRoom = true;
        MoveToNextDestination();
    }

    private void MoveToNextDestination()
    {
        if (_pathQueue.Count > 0)
        {
            _currentDestinationRoom = _pathQueue.Dequeue();
            Vector3 targetPosition = new Vector3(_currentDestinationRoom.x, BasicHight, _currentDestinationRoom.y);
            _agent.SetDestination(targetPosition);
            _isMoving = true;
        }
        else
        {
            _agent.ResetPath();
            _isMoving = false;
        }
    }

    private void Update()
    {
        if (_isFirstRoom && _isMoving && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance
            && (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f))
        {
            MoveToNextDestination();
        }

    }


}
