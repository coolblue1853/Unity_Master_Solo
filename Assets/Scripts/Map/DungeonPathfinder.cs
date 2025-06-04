using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; // OrderBy, Contains ����� ����

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
                // ��θ� ã�� �� ���� ��� �ſ� ū �Ÿ��� ó��
                result.Add((target, float.MaxValue));
            }
        }

        // �Ÿ� �������� ����
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
