using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public DungeonManager Dungeon;
    public MoveInDungeon inDungeon; // 추후 Player 로 변경
    public PlayerController player; 
    public GameObject PlayerPrefab;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
    
    }
}
