using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class UIEntitiyAttack : MonoBehaviour
{
    Vector3 originalPosition;

    public async Task TryAttackTask(Transform targetTransform)
    {
        originalPosition = transform.position;

        // Calculate a point pulled back away from the target
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        Vector3 windupPosition = transform.position - (directionToTarget * 0.5f);

        // Pull back over 0.25 seconds
        await transform.DOMove(windupPosition, 0.25f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
    }

    /// <summary>
    /// Phase 2: Dash to target, trigger impact callback, return home
    /// </summary>
    public async Task PerformAttackTask(Transform targetTransform, Action onImpact)
    {
        // 1. Dash to Defender
        await transform.DOMove(targetTransform.position, 0.15f)
            .SetEase(Ease.InExpo)
            .AsyncWaitForCompletion();

        // 2. Trigger Impact (deal damage, screen shake, particles)
        onImpact?.Invoke();

        // 3. Return to Original Board Position
        await transform.DOMove(originalPosition, 0.3f)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion();
    }
}
