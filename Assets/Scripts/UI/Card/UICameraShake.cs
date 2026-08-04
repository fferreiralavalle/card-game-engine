using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class UICameraShake : MonoBehaviour
{
    public static UICameraShake Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public async Task ShakeTask(float duration = 0.2f, float strength = 20f, int vibrato = 20)
    {
        // Shakes camera local position and automatically restores original pos when complete
        await transform.DOShakePosition(duration, strength, vibrato, randomness: 90, snapping: false, fadeOut: true)
            .AsyncWaitForCompletion();
    }

    public void Shake(float duration = 0.2f, float strength = 20f, int vibrato = 20)
    {
        transform.DOShakePosition(duration, strength, vibrato, randomness: 90, snapping: false, fadeOut: true);
    }
}