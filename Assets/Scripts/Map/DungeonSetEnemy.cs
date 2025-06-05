using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSetEnemy : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 5;

    private List<RectInt> roomList = new List<RectInt>();
    private Dictionary<int, List<GameObject>> roomEnemies = new Dictionary<int, List<GameObject>>();

    public void SpawnEnemies(List<RectInt> rooms)
    {
        roomList = rooms;

        for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++)
        {
            RectInt room = rooms[roomIndex];
            int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

            if (!roomEnemies.ContainsKey(roomIndex))
                roomEnemies[roomIndex] = new List<GameObject>();

            for (int i = 0; i < enemyCount; i++)
            {
                int randX = Random.Range(room.x - room.width / 2, room.x + room.width / 2);
                int randZ = Random.Range(room.y - room.height / 2, room.y + room.height / 2);

                Vector3 spawnPos = new Vector3(randX, Constants.BasicHight, randZ);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                roomEnemies[roomIndex].Add(enemy);
            }
        }
    }

    public List<GameObject> GetEnemiesInRoom(Vector3 worldPos)
    {
        Vector2Int pos2D = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.z));

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].Contains(pos2D))
            {
                return roomEnemies.ContainsKey(i) ? roomEnemies[i] : new List<GameObject>();
            }
        }

        return new List<GameObject>(); // 위치에 해당하는 방이 없을 경우 빈 리스트 반환
    }
}
