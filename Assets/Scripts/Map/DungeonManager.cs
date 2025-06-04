using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{    private const float BasicHight = 1.387f;
    private GameManager gameManager;
    private DungeonPathfinder pathfinder;
    [SerializeField] DungeonGenerator generator;
    [SerializeField] private DungeonGenerationResult _rooms;
    [SerializeField] private List<Vector3> _sortedRooms;

    // RectInt를 RectInt? 로 변경하여 널러블 타입으로 만듭니다.
    [SerializeField] private RectInt? _startRoom;
    [SerializeField] private RectInt? _endRoom;

    public Queue<RectInt> _pathToEnd;

    private void Awake()
    {
        pathfinder = new DungeonPathfinder();
    }
    private void Start()
    {
        gameManager = GameManager.Instance;
        Invoke("GenerateDungeon", 1f);
    }

    public void GenerateDungeon()
    {
        _rooms = generator.Init();
        RectInt startRoomRect = _rooms.StartRoom.Value;
        var startPositon = new Vector3(startRoomRect.x, 0, startRoomRect.y);
        _sortedRooms = pathfinder.SortByNavMeshDistance(startPositon, _rooms.RoomCentersExcludingStart);
        gameManager.inDungeon.SetDungeon(startPositon,_sortedRooms);
    }

}