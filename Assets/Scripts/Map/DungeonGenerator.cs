using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Unity.AI.Navigation; // for OrderBy, Contains in DungeonPathfinder (if it's in the same file or a partial class)
public class DungeonGenerationResult
{
    public RectInt? StartRoom { get; set; }
    public List<Vector3> RoomCentersExcludingStart { get; set; }
}
public class DungeonGenerator : MonoBehaviour
{
    public GameObject RoomPrefab, CorridorPrefab;
    public Vector2Int MapSize = new Vector2Int(100, 100);
    public List<RectInt> Rooms = new List<RectInt>();
    public List<Vector2Int> Corridors = new List<Vector2Int>();

    public Transform dungeonParent;

    public DungeonGenerationResult Init()
    {
        Rooms.Clear();
        Corridors.Clear();

        if (dungeonParent != null)
        {
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in dungeonParent)
                childrenToDestroy.Add(child.gameObject);

            foreach (GameObject child in childrenToDestroy)
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        List<Leaf> leaves = new List<Leaf>();
        Leaf root = new Leaf(new RectInt(0, 0, MapSize.x, MapSize.y));
        leaves.Add(root);

        bool split = true;
        while (split)
        {
            split = false;
            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i].Left == null && leaves[i].Right == null)
                {
                    if (leaves[i].Area.width > 20 || leaves[i].Area.height > 20)
                    {
                        if (leaves[i].Split())
                        {
                            leaves.Add(leaves[i].Left);
                            leaves.Add(leaves[i].Right);
                            split = true;
                        }
                    }
                }
            }
        }

        root.CreateRooms();
        CollectRooms(root);
        ConnectRooms(root);
        SpawnRoomsAndCorridors();

        // ���� ���� ã�´�
        RectInt? startRoom = FindEdgeStartRoom();

        // ���� ���� ������ �߾� ��ǥ ����Ʈ ����
        List<Vector3> roomCentersExcludingStart = Rooms
            .Where(r => !startRoom.HasValue || !r.Equals(startRoom.Value))
            .Select(r =>
            {
                return new Vector3(r.x, 0, r.y);
            }).ToList();

        return new DungeonGenerationResult
        {
            StartRoom = startRoom,
            RoomCentersExcludingStart = roomCentersExcludingStart
        };
    }

    void CollectRooms(Leaf node)
    {
        if (node == null) return;

        if (node.Left == null && node.Right == null && node.Room.HasValue)
        {
            Rooms.Add(node.Room.Value);
        }
        CollectRooms(node.Left); // ���� �ڽ� ��� ȣ��
        CollectRooms(node.Right); // ������ �ڽ� ��� ȣ��
    }
    void ConnectRooms(Leaf node)
    {
        if (node == null) return;

        // �ڽ� ��尡 �ִٸ�, ��������� �ڽ� ��带 ���� ó��
        if (node.Left != null && node.Right != null)
        {
            ConnectRooms(node.Left);
            ConnectRooms(node.Right);

            RectInt? room1 = GetRoomRecursive(node.Left);
            RectInt? room2 = GetRoomRecursive(node.Right);

            if (room1.HasValue && room2.HasValue)
            {

                Vector2Int center1 = GetCenter(room1.Value);
                Vector2Int center2 = GetCenter(room2.Value);

                if (Random.value > 0.5f)
                {
                    for (int x = Mathf.Min(center1.x, center2.x); x <= Mathf.Max(center1.x, center2.x); x++)
                        Corridors.Add(new Vector2Int(x, center1.y));
                    for (int y = Mathf.Min(center1.y, center2.y); y <= Mathf.Max(center1.y, center2.y); y++)
                        Corridors.Add(new Vector2Int(center2.x, y));
                }
                else
                {
                    for (int y = Mathf.Min(center1.y, center2.y); y <= Mathf.Max(center1.y, center2.y); y++)
                        Corridors.Add(new Vector2Int(center1.x, y));
                    for (int x = Mathf.Min(center1.x, center2.x); x <= Mathf.Max(center1.x, center2.x); x++)
                        Corridors.Add(new Vector2Int(x, center2.y));
                }
            }
        }
    }

    RectInt? GetRoomRecursive(Leaf node)
    {
        if (node == null) return null;
        if (node.Room.HasValue) return node.Room;

        if (node.Left != null)
        {
            RectInt? room = GetRoomRecursive(node.Left);
            if (room.HasValue) return room;
        }
        if (node.Right != null)
        {
            RectInt? room = GetRoomRecursive(node.Right);
            if (room.HasValue) return room;
        }
        return null; // ���� ã�� ����
    }
    Vector2Int GetCenter(RectInt rect) =>
        new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

    void SpawnRoomsAndCorridors()
    {
        if (dungeonParent == null)
        {
            GameObject newParent = new GameObject("DungeonRoot");
            dungeonParent = newParent.transform;
            Debug.LogWarning("DungeonGenerator: dungeonParent�� �Ҵ���� �ʾ� 'DungeonRoot' GameObject�� �����߽��ϴ�. ���̾��Ű���� 'DungeonRoot'�� DungeonGenerator�� dungeonParent �ʵ忡 �Ҵ��ϴ� ���� ����ϼ���.");
        }

        foreach (var room in Rooms)
        {
            Vector3 pos = new Vector3(room.x, 0, room.y);
            GameObject obj = Instantiate(RoomPrefab, pos, Quaternion.identity);
            obj.transform.localScale = new Vector3(room.width, 1, room.height);

            obj.transform.SetParent(dungeonParent);
            obj.name = "Room_" + room.x + "_" + room.y + "_" + room.width + "x" + room.height;
        }

        foreach (var tile in Corridors)
        {
            Vector3 pos = new Vector3(tile.x, 0, tile.y);
            GameObject obj = Instantiate(CorridorPrefab, pos, Quaternion.identity);

            obj.transform.SetParent(dungeonParent);
            obj.name = "Corridor_" + tile.x + "_" + tile.y;
        }

        // ������ �׺�޽� Bake
        // NavMeshSurface �߰� (���� ���)
        var surface = dungeonParent.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = dungeonParent.gameObject.AddComponent<NavMeshSurface>();
        }

        // �ɼ� ���� (�ʿ��)
        surface.collectObjects = CollectObjects.Children;
        surface.layerMask = LayerMask.GetMask("Default"); // �ʿ��� ���̾ ����

        // Bake ����
        surface.BuildNavMesh();
    }

    public RectInt? FindEdgeStartRoom()
    {
        Dictionary<RectInt, int> connectedCorridorCount = new Dictionary<RectInt, int>();

        foreach (var room in Rooms)
        {
            Vector2Int center = GetCenter(room);
            int count = 0;

            // 4���� Ž��
            Vector2Int[] directions = new Vector2Int[]
            {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
            };

            foreach (var dir in directions)
            {
                Vector2Int neighbor = center + dir;
                if (Corridors.Contains(neighbor))
                {
                    count++;
                }
            }

            connectedCorridorCount[room] = count;
        }

        // ���� ������ 1������ ��鸸 ���͸�
        var oneCorridorRooms = connectedCorridorCount
            .Where(kv => kv.Value == 1)
            .Select(kv => kv.Key);

        if (!oneCorridorRooms.Any()) return null;

        // �ܰ� ��: �߽� ��ǥ�� ���� �ָ� �ִ� �� (0,0 ����)
        RectInt furthestRoom = oneCorridorRooms
            .OrderByDescending(room => GetCenter(room).sqrMagnitude)
            .First();

        return furthestRoom;
    }

}