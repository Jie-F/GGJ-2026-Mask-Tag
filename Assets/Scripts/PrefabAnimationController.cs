using UnityEngine;

public class PrefabAnimatorController : MonoBehaviour
{
    [Header("Assign the Animator of your prefab here")]
    public Animator animator;

    private const string animationStateName = "MaskKill";

    /// <summary>
    /// Call this function to start the MaskKill animation from the beginning.
    /// </summary>
    public void PlayMaskKill()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator not assigned on " + gameObject.name);
            return;
        }

        // Play the MaskKill animation from the start
        animator.Play(animationStateName, 0, 0f);
    }
}
