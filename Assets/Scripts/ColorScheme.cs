using UnityEngine;

public static class ColorScheme
{
    // Modern Minimal Theme - 세련된 그라데이션 색상

    // Background Gradient
    public static Color BackgroundTop = new Color(0.08f, 0.10f, 0.18f, 1f);      // Dark blue-grey #141D2E
    public static Color BackgroundBottom = new Color(0.05f, 0.07f, 0.12f, 1f);   // Darker #0D121F

    // Panel Colors (Glassmorphism style)
    public static Color PanelDark = new Color(0.12f, 0.14f, 0.22f, 0.92f);       // Semi-transparent dark
    public static Color PanelLight = new Color(0.15f, 0.17f, 0.25f, 0.88f);      // Lighter variant
    public static Color PanelAccent = new Color(0.18f, 0.20f, 0.30f, 0.85f);     // Accent panels

    // Button Gradients
    public static Color ButtonPrimaryTop = new Color(0.25f, 0.47f, 0.85f, 1f);   // Bright blue #4078D9
    public static Color ButtonPrimaryBottom = new Color(0.15f, 0.32f, 0.65f, 1f);// Darker blue #2752A6

    public static Color ButtonSuccessTop = new Color(0.20f, 0.73f, 0.55f, 1f);   // Teal #33BA8C
    public static Color ButtonSuccessBottom = new Color(0.12f, 0.58f, 0.42f, 1f);// Dark teal #1F946B

    public static Color ButtonDangerTop = new Color(0.91f, 0.30f, 0.35f, 1f);    // Red #E84D59
    public static Color ButtonDangerBottom = new Color(0.75f, 0.18f, 0.22f, 1f); // Dark red #BF2E38

    public static Color ButtonDisabled = new Color(0.25f, 0.27f, 0.35f, 1f);     // Grey

    // Text Colors
    public static Color TextPrimary = new Color(1f, 1f, 1f, 1f);                 // White
    public static Color TextSecondary = new Color(0.75f, 0.78f, 0.85f, 1f);      // Light grey
    public static Color TextAccent = new Color(0.52f, 0.76f, 1f, 1f);            // Light blue #85C2FF
    public static Color TextGold = new Color(1f, 0.85f, 0.40f, 1f);              // Gold #FFD966
    public static Color TextSuccess = new Color(0.40f, 0.95f, 0.70f, 1f);        // Bright green

    // UI Element Colors
    public static Color BorderLight = new Color(0.35f, 0.40f, 0.55f, 0.3f);      // Subtle border
    public static Color BorderAccent = new Color(0.52f, 0.76f, 1f, 0.6f);        // Highlighted border
    public static Color Shadow = new Color(0f, 0f, 0f, 0.4f);                    // Drop shadow

    // Progress Bar Colors
    public static Color ProgressFilled = new Color(0.30f, 0.70f, 1f, 1f);        // Bright blue
    public static Color ProgressEmpty = new Color(0.15f, 0.17f, 0.25f, 0.5f);    // Dark translucent
    public static Color ProgressGlow = new Color(0.52f, 0.76f, 1f, 0.8f);        // Glow effect

    // Tab Colors
    public static Color TabActive = new Color(0.25f, 0.47f, 0.85f, 1f);          // Active tab
    public static Color TabInactive = new Color(0.15f, 0.17f, 0.25f, 1f);        // Inactive tab
    public static Color TabHover = new Color(0.20f, 0.37f, 0.70f, 1f);           // Hover state

    // Dimension Tier Colors (for visual variety)
    public static Color[] DimensionColors = new Color[]
    {
        new Color(0.40f, 0.60f, 1.00f, 1f), // Tier 1 - Blue
        new Color(0.50f, 0.80f, 0.95f, 1f), // Tier 2 - Cyan
        new Color(0.60f, 0.90f, 0.70f, 1f), // Tier 3 - Green
        new Color(0.90f, 0.95f, 0.50f, 1f), // Tier 4 - Yellow
        new Color(1.00f, 0.70f, 0.40f, 1f), // Tier 5 - Orange
        new Color(1.00f, 0.50f, 0.50f, 1f), // Tier 6 - Red
        new Color(0.90f, 0.60f, 0.90f, 1f), // Tier 7 - Pink
        new Color(0.70f, 0.50f, 1.00f, 1f)  // Tier 8 - Purple
    };

    // Helper method to create gradient
    public static Gradient CreateGradient(Color top, Color bottom)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

        colorKeys[0] = new GradientColorKey(top, 0f);
        colorKeys[1] = new GradientColorKey(bottom, 1f);
        alphaKeys[0] = new GradientAlphaKey(top.a, 0f);
        alphaKeys[1] = new GradientAlphaKey(bottom.a, 1f);

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }
}
