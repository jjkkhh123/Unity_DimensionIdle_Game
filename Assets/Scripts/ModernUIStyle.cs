using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ModernUIStyle : MonoBehaviour
{
    public enum StyleType
    {
        ButtonPrimary,
        ButtonSuccess,
        ButtonDanger,
        Panel,
        PanelAccent
    }

    [Header("Style Settings")]
    public StyleType styleType = StyleType.ButtonPrimary;
    public bool useGradient = true;
    public bool addBorder = true;
    public bool addShadow = true;

    [Header("Border Settings")]
    public float borderWidth = 2f;
    public Color borderColor = ColorScheme.BorderLight;

    [Header("Shadow Settings")]
    public Vector2 shadowOffset = new Vector2(0, -4);
    public Color shadowColor = ColorScheme.Shadow;

    private Image image;
    private GameObject borderObject;
    private GameObject shadowObject;

    void Start()
    {
        image = GetComponent<Image>();
        ApplyStyle();
    }

    public void ApplyStyle()
    {
        if (image == null)
            image = GetComponent<Image>();

        // Apply base colors based on style type
        ApplyColors();

        // Add border if enabled
        if (addBorder)
        {
            AddBorder();
        }

        // Add shadow if enabled
        if (addShadow)
        {
            AddShadow();
        }

        // Apply gradient
        if (useGradient)
        {
            ApplyGradient();
        }
    }

    void ApplyColors()
    {
        switch (styleType)
        {
            case StyleType.ButtonPrimary:
                image.color = ColorScheme.ButtonPrimaryTop;
                break;
            case StyleType.ButtonSuccess:
                image.color = ColorScheme.ButtonSuccessTop;
                break;
            case StyleType.ButtonDanger:
                image.color = ColorScheme.ButtonDangerTop;
                break;
            case StyleType.Panel:
                image.color = ColorScheme.PanelDark;
                break;
            case StyleType.PanelAccent:
                image.color = ColorScheme.PanelAccent;
                break;
        }
    }

    void ApplyGradient()
    {
        // Create gradient texture
        Color topColor = image.color;
        Color bottomColor = image.color;

        switch (styleType)
        {
            case StyleType.ButtonPrimary:
                topColor = ColorScheme.ButtonPrimaryTop;
                bottomColor = ColorScheme.ButtonPrimaryBottom;
                break;
            case StyleType.ButtonSuccess:
                topColor = ColorScheme.ButtonSuccessTop;
                bottomColor = ColorScheme.ButtonSuccessBottom;
                break;
            case StyleType.ButtonDanger:
                topColor = ColorScheme.ButtonDangerTop;
                bottomColor = ColorScheme.ButtonDangerBottom;
                break;
        }

        // Create gradient texture
        int height = 64;
        Texture2D gradientTexture = new Texture2D(1, height);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color color = Color.Lerp(bottomColor, topColor, t);
            gradientTexture.SetPixel(0, y, color);
        }

        gradientTexture.Apply();

        // Apply to image
        RectTransform rect = GetComponent<RectTransform>();
        image.sprite = Sprite.Create(
            gradientTexture,
            new Rect(0, 0, 1, height),
            new Vector2(0.5f, 0.5f)
        );
    }

    void AddBorder()
    {
        // Remove existing border
        if (borderObject != null)
        {
            DestroyImmediate(borderObject);
        }

        // Create border object
        borderObject = new GameObject("Border");
        borderObject.transform.SetParent(transform, false);
        borderObject.transform.SetSiblingIndex(0); // Put behind main image

        RectTransform borderRect = borderObject.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(borderWidth * 2, borderWidth * 2);
        borderRect.anchoredPosition = Vector2.zero;

        Image borderImage = borderObject.AddComponent<Image>();
        borderImage.color = borderColor;

        // Use outline/border sprite if available, otherwise use solid color
        borderImage.type = Image.Type.Sliced;
    }

    void AddShadow()
    {
        // Remove existing shadow
        if (shadowObject != null)
        {
            DestroyImmediate(shadowObject);
        }

        // Create shadow object
        shadowObject = new GameObject("Shadow");
        shadowObject.transform.SetParent(transform.parent, false);
        shadowObject.transform.SetSiblingIndex(transform.GetSiblingIndex()); // Put behind main object

        RectTransform shadowRect = shadowObject.AddComponent<RectTransform>();
        RectTransform thisRect = GetComponent<RectTransform>();

        // Copy rect properties
        shadowRect.anchorMin = thisRect.anchorMin;
        shadowRect.anchorMax = thisRect.anchorMax;
        shadowRect.pivot = thisRect.pivot;
        shadowRect.sizeDelta = thisRect.sizeDelta;
        shadowRect.anchoredPosition = thisRect.anchoredPosition + shadowOffset;

        Image shadowImage = shadowObject.AddComponent<Image>();
        shadowImage.color = shadowColor;
        shadowImage.sprite = image.sprite;
        shadowImage.type = image.type;

        // Make shadow non-interactive
        shadowImage.raycastTarget = false;
    }

    void OnDestroy()
    {
        if (borderObject != null)
        {
            DestroyImmediate(borderObject);
        }

        if (shadowObject != null)
        {
            DestroyImmediate(shadowObject);
        }
    }

    // Public method to update style
    public void SetStyleType(StyleType newType)
    {
        styleType = newType;
        ApplyStyle();
    }
}
