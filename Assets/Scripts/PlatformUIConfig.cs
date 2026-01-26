using UnityEngine;

[CreateAssetMenu(fileName = "PlatformUIConfig", menuName = "Game/Platform UI Config")]
public class PlatformUIConfig : ScriptableObject
{
    [Header("Display Settings")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    public float matchWidthOrHeight = 0.5f;

    [Header("Scaling")]
    public float fontScaleMultiplier = 1.0f;
    public float buttonSizeMultiplier = 1.0f;

    [Header("Layout")]
    public Vector2 dimensionButtonSize = new Vector2(550, 200);
    public Vector2[] dimensionButtonColumnPositions = { new Vector2(-600, 0), new Vector2(0, 0) };
    public float dimensionButtonRowSpacing = 225f;

    [Header("Side Panels")]
    public Vector2 sidePanelSize = new Vector2(300, 300);
    public Vector3 prestigePanelPosition = new Vector3(775, -300, 0);
    public Vector3 tickspeedPanelPosition = new Vector3(450, -300, 0);
    public Vector3 dimBoostPanelPosition = new Vector3(450, 25, 0);
}
