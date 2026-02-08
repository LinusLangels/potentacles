using UnityEngine;

public class UIManager : MonoBehaviour
{
    public DrunkState DrunkState;
    public DrunkMeterUI DrunkMeter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrunkMeter.ScaleTransform.localScale = new Vector3(1f, 0.0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        float ratio = DrunkState.GetCurrentImbalanceRatio();

       DrunkMeter.ScaleTransform.localScale = new Vector3(1f, ratio, 1f);
    }
}
