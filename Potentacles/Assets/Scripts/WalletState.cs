using System;
using UnityEngine;

public class WalletState : MonoBehaviour
{
    public float CurrentFunds = 100;

    internal void ApplyCost(int cost)
    {
        CurrentFunds = Mathf.Max(0, CurrentFunds - cost);
    }

    internal void GiveFunds(int funds)
    {
        CurrentFunds += funds;
    }

    public void ResetWallet(int initialFunds)
    {
        CurrentFunds = initialFunds;
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
