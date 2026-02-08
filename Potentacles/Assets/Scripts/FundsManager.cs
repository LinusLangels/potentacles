using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct FundsData
{
    public int Funds;
}

public class FundsManager : MonoBehaviour
{
    public float BouncerPushForce = 5f;


    public static FundsManager Instance;

    public List<FundsData> Data;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    internal FundsData GetFundsData(ATM atm)
    {
        FundsData data = GetLevelData(atm);

        return data;
    }

    private FundsData GetLevelData(ATM state)
    {
        if (state.ATMLevel < Data.Count)
        {
            return Data[state.ATMLevel];
        }

        return default;
    }
}
