using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public DrunkState DrunkState;
    public DrunkMeterUI DrunkMeter;
    public WalletState WalletState;
    public TextMeshProUGUI TextFunds;
    public Image RedFade;

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 1f; // How fast the fade transitions

    private float targetAlpha = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrunkMeter.ScaleTransform.localScale = new Vector3(1f, 0.0f, 1f);

        // Initialize RedFade to transparent
        if (RedFade != null)
        {
            Color color = RedFade.color;
            color.a = 0f;
            RedFade.color = color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float ratio = DrunkState.GetCurrentImbalanceRatio();
        DrunkMeter.ScaleTransform.localScale = new Vector3(1f, ratio, 1f);

        // Set target alpha based on ratio
        if (ratio >= 1.0f)
        {
            targetAlpha = 0.5f;
        }
        else
        {
            targetAlpha = 0f;
        }

        // Smoothly fade RedFade towards target alpha
        if (RedFade != null)
        {
            Color color = RedFade.color;
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            RedFade.color = color;
        }

        TextFunds.SetText(WalletState.CurrentFunds.ToString());
    }
}