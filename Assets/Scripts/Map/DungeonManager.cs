using System.Collections.Generic;
using UnityEngine;

public class DungeonData
{
    public Vector3 _startPoint;
    public List<Vector3> _sortedRooms;
    public List<RectInt> _rectRooms;
    public DungeonData(Vector3 startPoint, List<Vector3> sortedRooms, List<RectInt> rectRooms)
    {
        _startPoint = startPoint;
        _sortedRooms = sortedRooms;
        _rectRooms = rectRooms;
    }
}

public class DungeonManager : MonoBehaviour
{   
    private GameManager gameManager;
    private DungeonPathfinder pathfinder;
    [SerializeField] DungeonGenerator generator;
    public DungeonSetEnemy setEnemy;
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
        var list = _rooms.Rooms;
        var startPositon = new Vector3(startRoomRect.x, Constants.BasicHight, startRoomRect.y);
        _sortedRooms = pathfinder.SortByNavMeshDistance(startPositon, _rooms.RoomCentersExcludingStart);
        setEnemy.SpawnEnemies(list);

        //gameManager.inDungeon.SetDungeon(startPositon,_sortedRooms, _rooms.Rooms);
        gameManager.player.SetDungeonData(new DungeonData(startPositon, _sortedRooms, _rooms.Rooms));
    }

}