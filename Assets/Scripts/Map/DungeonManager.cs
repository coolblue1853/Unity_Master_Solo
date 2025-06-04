using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] DungeonGenerator generator;
    [SerializeField] private List<RectInt> _allRooms;

    // RectInt�� RectInt? �� �����Ͽ� �η��� Ÿ������ ����ϴ�.
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
            // ���⼭ ���ο� �޼��带 ȣ���մϴ�.
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