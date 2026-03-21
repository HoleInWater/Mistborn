using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject pushEffect;
    public GameObject pullEffect;
    public GameObject metalPickupEffect;
    public GameObject damageEffect;
    public GameObject healEffect;
    
    public void PlayPushEffect(Vector3 position)
    {
        if (pushEffect != null)
        {
            GameObject effect = Instantiate(pushEffect, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    public void PlayPullEffect(Vector3 position)
    {
        if (pullEffect != null)
        {
            GameObject effect = Instantiate(pullEffect, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    public void PlayPickupEffect(Vector3 position)
    {
        if (metalPickupEffect != null)
        {
            GameObject effect = Instantiate(metalPickupEffect, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
    
    public void PlayDamageEffect(Vector3 position)
    {
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }
    
    public void PlayHealEffect(Vector3 position)
    {
        if (healEffect != null)
        {
            GameObject effect = Instantiate(healEffect, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
}
