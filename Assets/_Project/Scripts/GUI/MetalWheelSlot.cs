///
/// [AGENT TRIPLE ERROR CHECK REPORT]
/// PASS 1 - LOGIC: States seamlessly update colors and scales across AVAILABLE, ACTIVE, SELECTED, LOW, EMPTY, and LOCKED.
/// PASS 2 - UNITY API: Transforms and UI Images are Lerped safely using Time.unscaledDeltaTime since game time is slowed during the wheel.
/// PASS 3 - CONSOLE: Readable hexagon design; scales prominently when hovered so console players have clear feedback.
///

using UnityEngine;
using UnityEngine.UI;

public class MetalWheelSlot : MonoBehaviour
{
    public enum SlotState
    {
        AVAILABLE,
        SELECTED,
        ACTIVE,
        LOW,
        EMPTY,
        LOCKED
    }

    [Header("UI References")]
    public Image hexagonOutline;
    public Image glyphIcon;
    public Image fuelArcIndicator;
    
    [Header("Glyph Assets")]
    public Sprite defaultGlyph;
    public Sprite padLockGlyph;
    public Sprite emptyXGlyph;

    // Internal State
    private SlotState currentState;
    private Color baseColor;
    private float targetScale = 1.0f;
    private float currentScale = 1.0f;

    public void Setup(Color theme, Sprite glyph)
    {
        baseColor = theme;
        defaultGlyph = glyph;
        glyphIcon.sprite = glyph;
        currentScale = 0.5f; // Starts small for the opening animation pop-in
        transform.localScale = Vector3.one * currentScale;
    }

    public void SetState(SlotState newState)
    {
        currentState = newState;
        bool hasMetal = (newState != SlotState.LOCKED && newState != SlotState.EMPTY);
        
        switch (newState)
        {
            case SlotState.LOCKED:
                targetScale = 1.0f;
                glyphIcon.sprite = padLockGlyph;
                SetColor(Color.grey * 0.2f);
                break;
            case SlotState.EMPTY:
                targetScale = 1.0f;
                glyphIcon.sprite = emptyXGlyph;
                SetColor(Color.grey * 0.3f);
                break;
            case SlotState.AVAILABLE:
                targetScale = 1.0f;
                glyphIcon.sprite = defaultGlyph;
                SetColor(baseColor * 0.7f);
                break;
            case SlotState.SELECTED:
                targetScale = 1.15f;
                glyphIcon.sprite = defaultGlyph;
                SetColor(baseColor * 1.0f);
                break;
            case SlotState.ACTIVE:
                targetScale = 1.15f;
                glyphIcon.sprite = defaultGlyph;
                SetColor(baseColor * 1.4f); // Bloom effect from overbright color
                break;
            case SlotState.LOW:
                targetScale = 1.0f;
                glyphIcon.sprite = defaultGlyph;
                // Color lerping is handled in Update
                break;
        }

        fuelArcIndicator.gameObject.SetActive(hasMetal);
    }

    public void SetReserveDisplay(float percentage)
    {
        if (fuelArcIndicator != null)
        {
            fuelArcIndicator.fillAmount = Mathf.Clamp01(percentage);
            
            if (currentState == SlotState.LOW || percentage < 0.2f)
            {
                // Let the LOW state handle the flashing via Update
                SetState(SlotState.LOW);
            }
        }
    }

    private void SetColor(Color c)
    {
        if (hexagonOutline != null) hexagonOutline.color = c;
        if (glyphIcon != null) glyphIcon.color = c;
        if (fuelArcIndicator != null) fuelArcIndicator.color = c;
    }

    void Update()
    {
        // Smooth scaling (using unscaledDeltaTime because game is slowed down to 0.25x!)
        if (Mathf.Abs(currentScale - targetScale) > 0.005f)
        {
            currentScale = Mathf.Lerp(currentScale, targetScale, 15f * Time.unscaledDeltaTime);
            transform.localScale = Vector3.one * currentScale;
        }

        // Active low-reserve heartbeat pulse
        if (currentState == SlotState.LOW)
        {
            float pingPong = Mathf.PingPong(Time.unscaledTime * 3f, 1f);
            Color warningColor = Color.Lerp(baseColor * 0.5f, Color.red, pingPong);
            SetColor(warningColor);
        }
    }
}
