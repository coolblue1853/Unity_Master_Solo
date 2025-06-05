using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MoveInDungeon : MonoBehaviour
{
    public PlayerController playerController;
    private Vector3 _startPoint;
    private List<Vector3> _sortedRooms;
    private List<RectInt> _rectRooms;
    private NavMeshAgent _agent;
    private bool _isMoving = false;
    private bool _isFirstRoom;
    private int _currentRoomIndex = 0;

    public bool IsInRoom;

    private void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>();
    }
    public void SetDungeon(Vector3 startPoint , List<Vector3> sortedRooms, List<RectInt> rectRooms )
    {
        _startPoint = startPoint;
        _sortedRooms = sortedRooms;
        _rectRooms = rectRooms;
        _agent.enabled = false;
        _isFirstRoom = false;
    }


    public void StartPlayerMove()
    {
        if(_currentRoomIndex == 0)
        {
            transform.position = _startPoint;
            _agent.enabled = true;
            _isFirstRoom = true;
        }
        MoveToNextDestination();
    }

    private void Update()
    {
    
        if (_isFirstRoom && _isMoving && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance
            && (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f))
        {
            // MoveToNextDestination();
            playerController.State = Define.State.Chase;
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
    private IEnumerator CheckPlayerInRoomRoutine()
    {
        while (true)
        {
            bool inRoom = IsPlayerInRoom(transform.position);
            IsInRoom = inRoom;

            if (inRoom)
            {
                playerController.State = Define.State.Chase;
                _agent.ResetPath();
                _isMoving = false;
                StopCoroutine(CheckPlayerInRoomRoutine());
            }
            yield return new WaitForSeconds(0.05f); 
        }
    }
    public bool IsPlayerInRoom(Vector3 playerPosition)
    {

        Vector2Int playerPos2D = new Vector2Int(Mathf.FloorToInt(playerPosition.x), Mathf.FloorToInt(playerPosition.z));


        foreach (var room in _rectRooms)
        {
            if (room.Contains(playerPos2D))
                return true;
        }
        return false;
    }
}
