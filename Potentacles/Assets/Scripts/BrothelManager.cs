using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct BrothelData
{
    public float BalanceImprovement;
    public int Cost;
}

public class BrothelManager : MonoBehaviour
{
    public float BouncerPushForce = 5f;


    public static BrothelManager Instance;

    public List<BrothelData> Data;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal BrothelData GetBrothelData(BrothelState brothelState)
    {
        BrothelData data = GetLevelData(brothelState);

        return data;
    }

    private BrothelData GetLevelData(BrothelState state)
    {
        if (state.BrothelLevel < Data.Count)
        {
            return Data[state.BrothelLevel];
        }

        return default;
    }
}
