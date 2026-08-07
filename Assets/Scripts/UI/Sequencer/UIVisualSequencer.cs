using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace RuntimeCardEngine
{
    public class VisualSequencer : MonoBehaviour
    {
        private List<AnimationEvent> animationQueue = new List<AnimationEvent>();
        private bool isPlayingSequence = false;

        public static VisualSequencer Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void EnqueueAnimation(Func<Task> animationTask, float priority = 0)
        {
            int i = 0;
            while (i < animationQueue.Count)
            {
                float animPriority = animationQueue[i].priority;
                if (priority > animPriority)
                    break;
                i++;
            }
            animationQueue.Insert(i, new AnimationEvent(animationTask, priority));

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
                var nextAnimation = animationQueue[0];
                animationQueue.RemoveAt(0);
                try
                {
                    // Wait for the visual animation to complete before moving to the next!
                    await nextAnimation.Play();
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