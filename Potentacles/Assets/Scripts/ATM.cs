using System.Collections;
using UnityEngine;

public class ATM : MonoBehaviour
{
    private float TimeEntered;

    public float TimeInATM = 2f;
    public int ATMLevel = 1;

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
            GameStateManager.Instance.SetGameState(GameState.ATM);
            TimeEntered = Time.time;

            StartCoroutine(LerpPlayer());

            BrothelUsed = true;
        }
    }

    IEnumerator LerpPlayer()
    {
        float targetTime = TimeInATM / 2f;
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

        var Force = FundsManager.Instance.BouncerPushForce;
        if (targetPosition.x > 0f)
        {
            ReferencedPlayer.AddForce(-Force);
        }
        else
        {
            ReferencedPlayer.AddForce(Force);
        }

        var data = FundsManager.Instance.GetFundsData(this);

        ReferencedPlayer.walletState.GiveFunds(data.Funds);

        ReferencedPlayer.Visit();
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Something exited: " + other.gameObject.name);
        // Your code here

    }
}
