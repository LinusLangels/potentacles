using UnityEngine;

public enum GameState
{
    Walking,
    Down,
    InBrothel,
    InShop
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameState CurrentState;
    public MapGenerator MapGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;

        CurrentState = GameState.Walking;

        MapGenerator.SetLevelLength(100f);


    }

    public void SetGameState(GameState state)
    {
        CurrentState = state;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
