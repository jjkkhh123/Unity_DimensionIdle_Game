using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GradientBackground : MonoBehaviour
{
    public enum GradientDirection
    {
        Vertical,
        Horizontal,
        Diagonal,
        Radial
    }

    [Header("Gradient Settings")]
    public Color topColor = ColorScheme.BackgroundTop;
    public Color bottomColor = ColorScheme.BackgroundBottom;
    public GradientDirection direction = GradientDirection.Vertical;

    [Header("Animation")]
    public bool animateGradient = false;
    public float animationSpeed = 0.2f;
    public Color alternateTopColor = new Color(0.10f, 0.12f, 0.20f, 1f);
    public Color alternateBottomColor = new Color(0.06f, 0.08f, 0.14f, 1f);

    private Image image;
    private Material gradientMaterial;
    private float animationTime = 0f;

    void Start()
    {
        image = GetComponent<Image>();
        SetupGradient();
    }

    void Update()
    {
        if (animateGradient && gradientMaterial != null)
        {
            animationTime += Time.deltaTime * animationSpeed;
            float t = (Mathf.Sin(animationTime) + 1f) / 2f; // 0 to 1

            Color currentTop = Color.Lerp(topColor, alternateTopColor, t);
            Color currentBottom = Color.Lerp(bottomColor, alternateBottomColor, t);

            gradientMaterial.SetColor("_TopColor", currentTop);
            gradientMaterial.SetColor("_BottomColor", currentBottom);
        }
    }

    void SetupGradient()
    {
        // Create gradient material
        Shader gradientShader = Shader.Find("UI/Default");

        // For simple vertical gradient, we can use a simple texture approach
        if (direction == GradientDirection.Vertical)
        {
            CreateVerticalGradientTexture();
        }
        else
        {
            // For more complex gradients, would need custom shader
            CreateVerticalGradientTexture();
        }
    }

    void CreateVerticalGradientTexture()
    {
        int height = 256;
        Texture2D gradientTexture = new Texture2D(1, height);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color color = Color.Lerp(bottomColor, topColor, t);
            gradientTexture.SetPixel(0, y, color);
        }

        gradientTexture.Apply();

        if (image != null)
        {
            image.sprite = Sprite.Create(
                gradientTexture,
                new Rect(0, 0, 1, height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }

    public void SetColors(Color top, Color bottom)
    {
        topColor = top;
        bottomColor = bottom;
        CreateVerticalGradientTexture();
    }
}
