using System.Collections.Generic;
using UnityEngine;
using System.Linq; // OrderBy, Contains 사용을 위해

public class DungeonPathfinder
{
    private List<RectInt> _rooms;
    private List<Vector2Int> _corridors;

    public DungeonPathfinder(List<RectInt> rooms, List<Vector2Int> corridors)
    {
        _rooms = rooms;
        _corridors = corridors;
    }

    // 주어진 RectInt의 중심 좌표를 반환 (DungeonGenerator에 이미 있는 경우 복사X)
    private Vector2Int GetCenter(RectInt rect) =>
        new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

    public Queue<RectInt> FindPath(RectInt startRoom, RectInt endRoom)
    {
        Dictionary<RectInt, (RectInt? previous, float gCost)> cameFrom = new Dictionary<RectInt, (RectInt? previous, float gCost)>();
        Dictionary<RectInt, float> fCost = new Dictionary<RectInt, float>(); // gCost + hCost

        // 열린 목록 (탐색할 노드)
        PriorityQueue<RectInt> openSet = new PriorityQueue<RectInt>();

        // 모든 방의 gCost를 무한대로 초기화
        foreach (var room in _rooms)
        {
            cameFrom[room] = (null, float.PositiveInfinity);
            fCost[room] = float.PositiveInfinity;
        }

        // 시작 노드 초기화
        cameFrom[startRoom] = (null, 0f);
        fCost[startRoom] = Heuristic(startRoom, endRoom); 
        openSet.Enqueue(startRoom, fCost[startRoom]);

        while (openSet.Count > 0)
        {
            RectInt current = openSet.Dequeue();

            if (current.Equals(endRoom)) 
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var neighbor in _rooms)
            {
                if (neighbor.Equals(current)) continue; // 자기 자신은 인접 방이 아님

                float tentativeGCost = cameFrom[current].gCost + Vector2.Distance(GetCenter(current), GetCenter(neighbor));

                if (tentativeGCost < cameFrom[neighbor].gCost)
                {
                    cameFrom[neighbor] = (current, tentativeGCost);
                    fCost[neighbor] = tentativeGCost + Heuristic(neighbor, endRoom);
                    openSet.Enqueue(neighbor, fCost[neighbor]);
                }
            }
        }

        return new Queue<RectInt>(); // 경로를 찾지 못함
    }


    private float Heuristic(RectInt a, RectInt b)
    {
        Vector2Int centerA = GetCenter(a);
        Vector2Int centerB = GetCenter(b);
        return Vector2.Distance(centerA, centerB);
    }


    private Queue<RectInt> ReconstructPath(Dictionary<RectInt, (RectInt? previous, float gCost)> cameFrom, RectInt current)
    {
        Stack<RectInt> pathStack = new Stack<RectInt>();
        pathStack.Push(current);

        while (cameFrom[current].previous.HasValue)
        {
            current = cameFrom[current].previous.Value;
            pathStack.Push(current);
        }

        // 스택에서 큐로 옮겨 순서 뒤집기
        Queue<RectInt> pathQueue = new Queue<RectInt>();
        while (pathStack.Count > 0)
        {
            pathQueue.Enqueue(pathStack.Pop());
        }
        return pathQueue;
    }

    public Queue<RectInt> FindPathVisitingAllRooms(RectInt startRoom, RectInt endRoom)
    {
        Queue<RectInt> fullPath = new Queue<RectInt>();
        HashSet<RectInt> visitedRoomsInOrder = new HashSet<RectInt>();
        List<RectInt> unvisitedRooms = new List<RectInt>(_rooms);

        if (!unvisitedRooms.Contains(startRoom))
        {
            Debug.LogError("시작 방이 전체 방 목록에 없습니다.");
            return new Queue<RectInt>();
        }
        unvisitedRooms.Remove(startRoom);

        RectInt currentRoom = startRoom;
        // 시작 방을 fullPath에 먼저 추가합니다.
        fullPath.Enqueue(currentRoom);
        visitedRoomsInOrder.Add(currentRoom); // 방문했다고 표시

        while (unvisitedRooms.Count > 0)
        {
            RectInt? nextRoom = null;
            float shortestDistance = float.MaxValue;
            Queue<RectInt> pathToNext = null;

            foreach (var room in unvisitedRooms)
            {
                Queue<RectInt> tempPath = FindPath(currentRoom, room);

                if (tempPath.Count > 0)
                {
                    float currentPathCost = 0;
                    Vector2Int prevCenter = GetCenter(currentRoom);
                    foreach (var pathRoom in tempPath.Skip(1)) 
                    {
                        currentPathCost += Vector2.Distance(prevCenter, GetCenter(pathRoom));
                        prevCenter = GetCenter(pathRoom);
                    }

                    if (currentPathCost < shortestDistance)
                    {
                        shortestDistance = currentPathCost;
                        nextRoom = room;
                        pathToNext = tempPath;
                    }
                }
            }

            if (nextRoom.HasValue && pathToNext != null)
            {
                foreach (var pathRoom in pathToNext.Skip(1))
                {
                    if (!visitedRoomsInOrder.Contains(pathRoom))
                    {
                        fullPath.Enqueue(pathRoom);
                        visitedRoomsInOrder.Add(pathRoom);
                    }
                }

                currentRoom = nextRoom.Value;
                unvisitedRooms.Remove(currentRoom);
            }
            else
            {
                break;
            }
        }

        // 마지막으로 방문한 방에서 최종 목표 방으로 이동합니다.
        if (!currentRoom.Equals(endRoom))
        {
            Queue<RectInt> pathToEndFromLast = FindPath(currentRoom, endRoom);
            foreach (var pathRoom in pathToEndFromLast.Skip(1))
            {
                if (!visitedRoomsInOrder.Contains(pathRoom))
                {
                    fullPath.Enqueue(pathRoom);
                    visitedRoomsInOrder.Add(pathRoom);
                }
            }
        }

        return fullPath;
    }
}

public class PriorityQueue<T>
{
    private List<(T item, float priority)> _elements = new List<(T item, float priority)>();

    public int Count => _elements.Count;

    public void Enqueue(T item, float priority)
    {
        _elements.Add((item, priority));
        _elements.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public T Dequeue()
    {
        T item = _elements[0].item;
        _elements.RemoveAt(0);
        return item;
    }
}