using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MoveInDungeon : MonoBehaviour
{
    private DungeonManager dungeonManager;
    private Vector3 _startPoint;
    private List<Vector3> _sortedRooms;
    private List<RectInt> _rectRooms;
    private NavMeshAgent _agent;
    private Queue<RectInt> _pathQueue;
    private bool _isMoving = false;
    private bool _isFirstRoom;
    private int _currentRoomIndex = 0;

    public bool IsInRoom;

    private void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>();
        dungeonManager = GameManager.Instance.Dungeon;
    }
    public void SetDungeon(Vector3 startPoint , List<Vector3> sortedRooms, List<RectInt> rectRooms )
    {
        _startPoint = startPoint;
        _sortedRooms = sortedRooms;
        _rectRooms = rectRooms;
        _agent.enabled = false;
        _isFirstRoom = false;

        Invoke("StartPlayerMove", 0.1f);
        StartCoroutine(CheckPlayerInRoomRoutine());
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
    private IEnumerator CheckPlayerInRoomRoutine()
    {
        while (true)
        {
            bool inRoom = IsPlayerInRoom(transform.position);
            IsInRoom = inRoom;
            yield return new WaitForSeconds(0.05f); 
        }
    }
    public bool IsPlayerInRoom(Vector3 playerPosition)
    {
        // 플레이어 위치를 2D 좌표로 변환 (x, z)
        Vector2Int playerPos2D = new Vector2Int(Mathf.FloorToInt(playerPosition.x), Mathf.FloorToInt(playerPosition.z));

        // Rooms 리스트 순회하며 포함 여부 검사
        foreach (var room in _rectRooms)
        {
            if (room.Contains(playerPos2D))
                return true; // 방 안에 있음
        }
        return false; // 어떤 방에도 없음 (복도거나 빈 공간)
    }
}
