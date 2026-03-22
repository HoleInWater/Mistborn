using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common animation operations
    /// </summary>
    public class AnimationUtils : MonoBehaviour
    {
        /// <summary>
        /// Cross-fades to an animation state with optional parameters
        /// </summary>
        public static void CrossFade(Animator animator, string stateName, float normalizedTransitionDuration = 0.25f, int layer = -1, float normalizedTimeOffset = 0f)
        {
            if (animator == null) return;
            
            if (layer >= 0)
            {
                animator.CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset);
            }
            else
            {
                animator.CrossFade(stateName, normalizedTransitionDuration);
            }
        }

        /// <summary>
        /// Plays an animation clip
        /// </summary>
        public static void Play(Animator animator, string stateName, int layer = -1)
        {
            if (animator == null) return;
            
            if (layer >= 0)
            {
                animator.Play(stateName, layer);
            }
            else
            {
                animator.Play(stateName);
            }
        }

        /// <summary>
        /// Sets a boolean parameter
        /// </summary>
        public static void SetBool(Animator animator, string name, bool value)
        {
            if (animator == null) return;
            animator.SetBool(name, value);
        }

        /// <summary>
        /// Sets an integer parameter
        /// </summary>
        public static void SetInt(Animator animator, string name, int value)
        {
            if (animator == null) return;
            animator.SetInt(name, value);
        }

        /// <summary>
        /// Sets a float parameter
        /// </summary>
        public static void SetFloat(Animator animator, string name, float value)
        {
            if (animator == null) return;
            animator.SetFloat(name, value);
        }

        /// <summary>
        /// Triggers a trigger parameter
        /// </summary>
        public static void SetTrigger(Animator animator, string name)
        {
            if (animator == null) return;
            animator.SetTrigger(name);
        }

        /// <summary>
        /// Resets a trigger parameter
        /// </summary>
        public static void ResetTrigger(Animator animator, string name)
        {
            if (animator == null) return;
            animator.ResetTrigger(name);
        }

        /// <summary>
        /// Gets the current state information on a layer
        /// </summary>
        public static AnimatorStateInfo GetCurrentState(Animator animator, int layerIndex = 0)
        {
            if (animator == null)
                return new AnimatorStateInfo();
                
            return animator.GetCurrentAnimatorStateInfo(layerIndex);
        }

        /// <summary>
        /// Gets the normalized time of the current state on a layer
        /// </summary>
        public static float GetNormalizedTime(Animator animator, int layerIndex = 0)
        {
            if (animator == null)
                return 0f;
                
            return animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;
        }

        /// </// <summary>
        /// Checks if a parameter exists in the animator
        /// </summary>
        public static bool HasParameter(Animator animator, string name)
        {
            if (animator == null)
                return false;
                
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the layer index by name
        /// </summary>
        public static int GetLayerIndex(Animator animator, string layerName)
        {
            if (animator == null || string.IsNullOrEmpty(layerName))
                return -1;
                
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerName(i) == layerName)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Sets the layer weight
        /// </summary>
        public static void SetLayerWeight(Animator animator, int layerIndex, float weight)
        {
            if (animator == null) return;
            if (layerIndex >= 0 && layerIndex < animator.layerCount)
                animator.SetLayerWeight(layerIndex, weight);
        }

        /// <summary>
        /// Gets the layer weight
        /// </summary>
        public static float GetLayerWeight(Animator animator, int layerIndex)
        {
            if (animator == null) return 0f;
            if (layerIndex >= 0 && layerIndex < animator.layerCount)
                return animator.GetLayerWeight(layerIndex);
            return 0f;
        }

        /// <summary>
        /// Matches the current position and rotation to a target
        /// </summary>
        public static void MatchTarget(Animator animator, Vector3 position, Quaternion rotation, AvatarTarget target, 
                                     float startNormalizedTime, float targetNormalizedTime, 
                                     MatchingTargetWeightMask positionWeightMask, MatchingTargetWeightMask rotationWeightMask)
        {
            if (animator == null) return;
            animator.MatchTarget(position, rotation, target, startNormalizedTime, targetNormalizedTime, positionWeightMask, rotationWeightMask);
        }

        /// <summary>
        /// Gets the avatar
        /// </summary>
        public static Avatar GetAvatar(Animator animator)
        {
            if (animator == null) return null;
            return animator.avatar;
        }

        /// <summary>
        /// Sets the avatar
        /// </summary>
        public static void SetAvatar(Animator animator, Avatar avatar)
        {
            if (animator == null) return;
            animator.avatar = avatar;
        }
    }
}