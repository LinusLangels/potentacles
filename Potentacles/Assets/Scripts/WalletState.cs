using System;
using UnityEngine;

public class WalletState : MonoBehaviour
{
    public float CurrentFunds = 100;

    internal void ApplyCost(int cost)
    {
        CurrentFunds = Mathf.Max(0, CurrentFunds - cost);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
