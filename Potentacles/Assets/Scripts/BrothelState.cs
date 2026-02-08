using System.Collections;
using UnityEngine;

public class BrothelState : MonoBehaviour
{
    private float TimeEntered;

    public float TimeInBrothel = 2f;
    public int BrothelLevel = 1;

    private BoxCollider Collider;

    private Player ReferencedPlayer = null;
    private bool BrothelUsed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered: " + other.gameObject.name);
        // Your code here
        var player = other.gameObject.GetComponentInParent<Player>();

        if (BrothelUsed)
            return;

  
        if (player != null)
        {
            ReferencedPlayer = player;
            GameStateManager.Instance.SetGameState(GameState.InBrothel);
            TimeEntered = Time.time;

            StartCoroutine(LerpPlayer());

            BrothelUsed = true;
        }
    }

    IEnumerator LerpPlayer()
    {
        float targetTime = TimeInBrothel / 2f;
        float currentTime = 0;

        Vector3 playerStart = ReferencedPlayer.transform.position;

        Vector3 targetPosition = Collider.transform.TransformPoint(Collider.center);
        targetPosition.y = 0f;

        while (currentTime <= targetTime)
        {
            float ratio = currentTime / targetTime;

            ReferencedPlayer.transform.position = Vector3.Lerp(playerStart, targetPosition, ratio);

            currentTime += Time.deltaTime;
            yield return null;
        }


        currentTime = 0;

        while (currentTime <= targetTime)
        {
            float ratio = currentTime / targetTime;

            ReferencedPlayer.transform.position = Vector3.Lerp(targetPosition, playerStart, ratio);

            currentTime += Time.deltaTime;
            yield return null;
        }

        GameStateManager.Instance.SetGameState(GameState.Walking);

        //Add force in x direction in towards street
        Vector3 bouncer = Vector3.zero - targetPosition;
        bouncer.y = 0f;
        bouncer.z = 0f;

        var Force = BrothelManager.Instance.BouncerPushForce;
        if (targetPosition.x > 0f)
        {
            ReferencedPlayer.AddForce(-Force);
        }
        else
        {
            ReferencedPlayer.AddForce(Force);
        }

        var data = BrothelManager.Instance.GetBrothelData(this);

        ReferencedPlayer.balanceState.ReduceImbalance(data.BalanceImprovement);

        ReferencedPlayer.walletState.ApplyCost(data.Cost);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Something exited: " + other.gameObject.name);
        // Your code here

    }
}
