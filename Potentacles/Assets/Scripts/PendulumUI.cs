using UnityEngine;
using UnityEngine.UI;


public class PendulumUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;

    [Header("Display Settings")]
    [SerializeField] private float displayScale = 80f; // Visual scale of pendulum
    [SerializeField] private Color rodColor = Color.yellow;
    [SerializeField] private Color centerLineColor = Color.green;
    [SerializeField] private Color boundColor = Color.red;
    [SerializeField] private float lineThickness = 3f;

    private RectTransform rectTransform;
    private Image rodImage;
    private Image centerImage;
    private Image leftBoundImage;
    private Image rightBoundImage;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        CreatePendulumVisuals();
    }

    void CreatePendulumVisuals()
    {
        // Create rod line
        GameObject rodObj = CreateLine("Rod", rodColor);
        rodImage = rodObj.GetComponent<Image>();

        // Create center reference line
        GameObject centerObj = CreateLine("CenterLine", centerLineColor);
        centerImage = centerObj.GetComponent<Image>();

        // Create left bound line
        GameObject leftBoundObj = CreateLine("LeftBound", boundColor);
        leftBoundImage = leftBoundObj.GetComponent<Image>();

        // Create right bound line
        GameObject rightBoundObj = CreateLine("RightBound", boundColor);
        rightBoundImage = rightBoundObj.GetComponent<Image>();
    }

    GameObject CreateLine(string name, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform, false);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = color;

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0f); // Pivot at bottom for rotation

        return lineObj;
    }

    void Update()
    {
        if (player == null || rodImage == null) return;

        UpdatePendulumDisplay();
    }

    void UpdatePendulumDisplay()
    {
        // Get player's balance angle (in radians)
        float balanceAngle = player.GetBalanceAngle();

        Vector2 pivot = Vector2.zero; // Center of the panel

        // Update rod
        RectTransform rodRect = rodImage.GetComponent<RectTransform>();
        rodRect.anchoredPosition = pivot;
        rodRect.sizeDelta = new Vector2(lineThickness, displayScale);
        rodRect.rotation = Quaternion.Euler(0, 0, -balanceAngle * Mathf.Rad2Deg);

        // Update center line (vertical reference)
        RectTransform centerRect = centerImage.GetComponent<RectTransform>();
        centerRect.anchoredPosition = pivot;
        centerRect.sizeDelta = new Vector2(lineThickness * 0.5f, displayScale);
        centerRect.rotation = Quaternion.identity;
        centerImage.color = new Color(centerLineColor.r, centerLineColor.g, centerLineColor.b, 0.3f);

        // Update left bound (-90 degrees)
        RectTransform leftRect = leftBoundImage.GetComponent<RectTransform>();
        leftRect.anchoredPosition = pivot;
        leftRect.sizeDelta = new Vector2(lineThickness * 0.5f, displayScale);
        leftRect.rotation = Quaternion.Euler(0, 0, 90);
        leftBoundImage.color = new Color(boundColor.r, boundColor.g, boundColor.b, 0.5f);

        // Update right bound (+90 degrees)
        RectTransform rightRect = rightBoundImage.GetComponent<RectTransform>();
        rightRect.anchoredPosition = pivot;
        rightRect.sizeDelta = new Vector2(lineThickness * 0.5f, displayScale);
        rightRect.rotation = Quaternion.Euler(0, 0, -90);
        rightBoundImage.color = new Color(boundColor.r, boundColor.g, boundColor.b, 0.5f);
    }
}