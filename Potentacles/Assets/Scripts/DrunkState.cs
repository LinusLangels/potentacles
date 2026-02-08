using UnityEngine;

public class DrunkState : MonoBehaviour
{
    [Header("Imbalance Settings")]
    [SerializeField] private float currentImbalance = 0f;
    [SerializeField] private float maxImbalance = 100f;

    [Header("Time Settings")]
    [SerializeField] private float minTimeBetweenAdds = 2f;
    [SerializeField] private float maxTimeBetweenAdds = 5f;

    [Header("Amount Settings")]
    [SerializeField] private float minImbalanceAmount = 5f;
    [SerializeField] private float maxImbalanceAmount = 15f;

    [Header("Player Effect Settings")]
    [SerializeField] private float minPlayerControlMultiplier = 0.2f; // At max imbalance
    [SerializeField] private float maxPlayerControlMultiplier = 1f;   // At zero imbalance
    [SerializeField] private float minPendulumForceMultiplier = 1f;   // At zero imbalance
    [SerializeField] private float maxPendulumForceMultiplier = 1.5f; // At max imbalance

    private float nextAddTime;
    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
        ScheduleNextAdd();
    }

    void Update()
    {
        if (Time.time >= nextAddTime && currentImbalance < maxImbalance)
        {
            AddImbalance();
            ScheduleNextAdd();
        }

        CheckGameOver();
    }

    void CheckGameOver()
    {

    }

    private void AddImbalance()
    {
        float amountToAdd = Random.Range(minImbalanceAmount, maxImbalanceAmount);
        currentImbalance = Mathf.Min(currentImbalance + amountToAdd, maxImbalance);

        Debug.Log($"Imbalance added: {amountToAdd:F2}. Total: {currentImbalance:F2}/{maxImbalance}");

        if (currentImbalance >= maxImbalance)
        {
            Debug.Log("Maximum imbalance reached!");
            OnMaxImbalanceReached();
        }
    }

    private void ScheduleNextAdd()
    {
        float waitTime = Random.Range(minTimeBetweenAdds, maxTimeBetweenAdds);
        nextAddTime = Time.time + waitTime;
    }

    private void OnMaxImbalanceReached()
    {
        // Add your logic here for when imbalance hits 100
    }

    public float GetCurrentImbalanceRatio()
    {
        float t = currentImbalance / maxImbalance;
        return t;
    }

    // Called by Player script to get reduced control strength
    public float GetPlayerControlMultiplier()
    {
        float t = currentImbalance / maxImbalance;
        return Mathf.Lerp(maxPlayerControlMultiplier, minPlayerControlMultiplier, t);
    }

    // Called by Player script to get increased pendulum swing force
    public float GetPendulumForceMultiplier()
    {
        float t = currentImbalance / maxImbalance;
        return Mathf.Lerp(minPendulumForceMultiplier, maxPendulumForceMultiplier, t);
    }

    // Optional: Method to reduce imbalance
    public void ReduceImbalance(float amount)
    {
        currentImbalance = Mathf.Max(currentImbalance - amount, 0f);
    }

    public float GetCurrentImbalance()
    {
        return currentImbalance;
    }
}