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
                // RectInt의 모든 x, y 좌표 범위 내에서 적을 생성합니다.
                // Random.Range(inclusiveMin, exclusiveMax)에서 exclusiveMax는 포함되지 않으므로,
                // room.x + room.width 를 maxExclusive로 사용하면 room.x + room.width - 1 까지의 값이 생성됩니다.
                int randX = Random.Range(room.x - room.width / 2, room.x + room.width/2);
                int randZ = Random.Range(room.y - room.height / 2, room.y + room.height/2);

                Vector3 spawnPos = new Vector3(randX, Constants.BasicHight, randZ);

                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}