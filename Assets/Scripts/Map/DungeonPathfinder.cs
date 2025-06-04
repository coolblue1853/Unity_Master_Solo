using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; // OrderBy, Contains 사용을 위해

public class DungeonPathfinder
{
    public List<Vector3> targets;

   public List<Vector3> SortByNavMeshDistance(Vector3 startPosition, List<Vector3> positions)
    {
        var result = new List<(Vector3 pos, float distance)>();

        foreach (var target in positions)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(startPosition, target, NavMesh.AllAreas, path))
            {
                float length = GetPathLength(path);
                result.Add((target, length));
            }
            else
            {
                // 경로를 찾을 수 없는 경우 매우 큰 거리로 처리
                result.Add((target, float.MaxValue));
            }
        }

        // 거리 기준으로 정렬
        return result.OrderBy(x => x.distance).Select(x => x.pos).ToList();
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }
}
