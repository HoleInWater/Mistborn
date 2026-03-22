/// <summary>
/// Simple animation events handler.
/// Connect Unity animations to game logic.
/// </summary>
public class AnimationEvents : MonoBehaviour
{
    // EVENTS - Connect in Animator > Animation Events
    
    // Called from animation
    public void OnAnimationComplete()
    {
        Debug.Log("Animation complete!");
    }
    
    // Footstep events
    public void OnFootstep()
    {
        AudioManager.Instance?.PlaySound("footstep");
    }
    
    // Attack events
    public void OnAttackHit()
    {
        Debug.Log("Attack hit frame!");
        // Enable hitbox, deal damage, etc.
    }
    
    public void OnAttackEnd()
    {
        Debug.Log("Attack animation ended!");
        // Disable hitbox, return to idle, etc.
    }
    
    // Dodge events
    public void OnDodgeComplete()
    {
        Debug.Log("Dodge complete!");
    }
    
    // Ability events
    public void OnAbilityCast()
    {
        Debug.Log("Ability cast!");
    }
    
    public void OnLanding()
    {
        AudioManager.Instance?.PlaySound("land");
    }
}
