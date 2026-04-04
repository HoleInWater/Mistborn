/* GroundSlamEffect.cs
 *
 * Spawns a dust cloud and ground crack effect at a position.
 * Called by FallDamage.PewterSlam() for the superhero landing.
 * Self-destructs after the particles finish.
 */

using UnityEngine;

public class GroundSlamEffect : MonoBehaviour
{
    public static void Spawn(Vector3 position, float radius, float intensity)
    {
        var obj = new GameObject("GroundSlamFX");
        obj.transform.position = position;
        var effect = obj.AddComponent<GroundSlamEffect>();
        effect.CreateEffect(radius, intensity);
    }

    void CreateEffect(float radius, float intensity)
    {
        // Dust ring expanding outward
        var dustObj = new GameObject("DustRing");
        dustObj.transform.SetParent(transform);
        dustObj.transform.localPosition = Vector3.zero;

        var ps = dustObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.5f;
        main.startSpeed = radius * 2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startColor = new Color(0.4f, 0.35f, 0.3f, 0.5f);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.1f;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(15 * intensity)) });
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        // Size grows then fades
        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.3f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.5f)));

        // Fade out
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.4f, 0.35f, 0.3f), 0f),
                    new GradientColorKey(new Color(0.3f, 0.28f, 0.25f), 1f) },
            new[] { new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0.3f, 0.5f),
                    new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // Self-destruct
        Destroy(gameObject, 3f);
    }
}
