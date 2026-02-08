using System;
using System.Collections;
using UnityEngine;

public enum GameState
{
    Startup,
    Walking,
    Down,
    InBrothel,
    InShop,
    ATM,
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameState CurrentState;
    public MapGenerator MapGenerator;

    public Player Player;


    public float StartingLevelLength = 100f;
    public float LevelLengthIncrease = 20f;
    public int InitialFunds = 100;

   



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;

        StartGame();
    }

    void StartGame()
    {
        MapGenerator.ResetMapGenerator(StartingLevelLength);


        SetGameState(GameState.Startup);

        StartCoroutine(StartWalking());
    }

    IEnumerator StartWalking()
    {
        yield return null;

        Player.transform.position = Vector3.zero;


        Player.walletState.ResetWallet(InitialFunds);
        Player.balanceState.ResetBalance();



        yield return new WaitForSeconds(1f);

        SetGameState(GameState.Walking);
    }

    public void SetGameState(GameState state)
    {
        CurrentState = state;

        if (CurrentState == GameState.Down)
        {
            StartGame();
        }
    }

    public void OnATMPassed()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal float GetLevelLengthIncrease(int currentLevel)
    {
        return LevelLengthIncrease;
    }
}
