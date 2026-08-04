using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RuntimeCardEngine
{
    public class VisualSequencer : MonoBehaviour
    {
        private Queue<Func<Task>> animationQueue = new Queue<Func<Task>>();
        private bool isPlayingSequence = false;

        public static VisualSequencer Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void EnqueueAnimation(Func<Task> animationTask)
        {
            animationQueue.Enqueue(animationTask);

            if (!isPlayingSequence)
            {
                _ = ProcessQueueAsync();
            }
        }

        private async Task ProcessQueueAsync()
        {
            isPlayingSequence = true;

            while (animationQueue.Count > 0)
            {
                var nextAnimation = animationQueue.Dequeue();

                try
                {
                    // Wait for the visual animation to complete before moving to the next!
                    await nextAnimation.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[VisualSequencer Error]: {ex.Message}");
                }
            }

            isPlayingSequence = false;
        }
    }
}