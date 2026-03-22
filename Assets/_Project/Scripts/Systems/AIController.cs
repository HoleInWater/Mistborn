using UnityEngine;

public class AIController : MonoBehaviour
{
    public enum EmotionState
    {
        Neutral,
        Calm,
        Enraged,
        Afraid,
        Confused
    }
    
    [Header("AI Settings")]
    public float detectionRange = 20f;
    public float attackRange = 5f;
    public float moveSpeed = 3f;
    
    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    
    private EmotionState emotionState = EmotionState.Neutral;
    private float aggressionMultiplier = 1f;
    private float lastAttackTime = 0f;
    private Transform target;
    
    public void SetEmotionState(EmotionState state)
    {
        emotionState = state;
    }
    
    public EmotionState GetEmotionState()
    {
        return emotionState;
    }
    
    public void SetAggressionMultiplier(float multiplier)
    {
        aggressionMultiplier = multiplier;
    }
    
    public float GetAggressionMultiplier()
    {
        return aggressionMultiplier;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public Transform GetTarget()
    {
        return target;
    }
    
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }
    
    public void Attack()
    {
        if (!CanAttack()) return;
        
        lastAttackTime = Time.time;
        Debug.Log($"{gameObject.name} attacks!");
    }
    
    void Update()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance <= attackRange && CanAttack())
            {
                Attack();
            }
        }
    }
}
