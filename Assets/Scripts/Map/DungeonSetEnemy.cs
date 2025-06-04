using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSetEnemy : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 5;

    public void SpawnEnemies(List<RectInt> rooms)
    {
        foreach (var room in rooms)
        {
            int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                // RectInt�� ��� x, y ��ǥ ���� ������ ���� �����մϴ�.
                // Random.Range(inclusiveMin, exclusiveMax)���� exclusiveMax�� ���Ե��� �����Ƿ�,
                // room.x + room.width �� maxExclusive�� ����ϸ� room.x + room.width - 1 ������ ���� �����˴ϴ�.
                int randX = Random.Range(room.x - room.width / 2, room.x + room.width/2);
                int randZ = Random.Range(room.y - room.height / 2, room.y + room.height/2);

                Vector3 spawnPos = new Vector3(randX, Constants.BasicHight, randZ);

                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}