using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] DungeonGenerator generator;
    [SerializeField] private List<RectInt> _allRooms;

    // RectInt를 RectInt? 로 변경하여 널러블 타입으로 만듭니다.
    [SerializeField] private RectInt? _startRoom;
    [SerializeField] private RectInt? _endRoom;

    public Queue<RectInt> _pathToEnd;

    public void GenerateDungeon()
    {
        _allRooms = generator.Init();

        _startRoom = generator.GetStartRoom();
        if (_startRoom.HasValue)
        {
            _endRoom = generator.GetEndRoom(_startRoom.Value);
        }
        else
        {
            _endRoom = null;
        }

        if (_startRoom.HasValue && _endRoom.HasValue)
        {
            DungeonPathfinder pathfinder = new DungeonPathfinder(_allRooms, generator.Corridors);
            // 여기서 새로운 메서드를 호출합니다.
            _pathToEnd = pathfinder.FindPathVisitingAllRooms(_startRoom.Value, _endRoom.Value);

            foreach (var room in _pathToEnd)
            {
                Debug.Log($"- {room}");
            }
        }
        else
        {
            _pathToEnd = new Queue<RectInt>();
        }
    }

}