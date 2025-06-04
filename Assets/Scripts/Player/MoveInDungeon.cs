using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveInDungeon : MonoBehaviour
{
    private DungeonManager dungeonManager;
    private Vector3 _startPoint;
    private List<Vector3> _sortedRooms;
    private NavMeshAgent _agent;
    private Queue<RectInt> _pathQueue;
    private bool _isMoving = false;
    private bool _isFirstRoom;
    private int _currentRoomIndex = 0;

    private void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>();
        dungeonManager = GameManager.Instance.Dungeon;
    }
    public void SetDungeon(Vector3 startPoint , List<Vector3> sortedRooms)
    {
        _startPoint = startPoint;
        _sortedRooms = sortedRooms;
        _agent.enabled = false;
        _isFirstRoom = false;

        Invoke("StartPlayerMove", 0.1f);
    }


    public void StartPlayerMove()
    {
        transform.position = _startPoint;
        _agent.enabled = true;
        _isFirstRoom = true;
        MoveToNextDestination();
    }

    private void Update()
    {
        if (_isFirstRoom && _isMoving && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance
            && (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f))
        {
            MoveToNextDestination();
        }
    }

    private void MoveToNextDestination()
    {
        if (_currentRoomIndex < _sortedRooms.Count)
        {
            Vector3 targetPosition = _sortedRooms[_currentRoomIndex];
            _agent.SetDestination(targetPosition);
            _isMoving = true;
            _currentRoomIndex++;
        }
        else
        {
            _agent.ResetPath();
            _isMoving = false;
        }
    }


}
