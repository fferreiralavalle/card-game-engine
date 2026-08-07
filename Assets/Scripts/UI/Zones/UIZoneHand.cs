using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using RuntimeCardEngine;
using System.Linq;

public class UIZoneHand : UIZone
{
    public float arcHeight = 5f; // How much the outer cards drop down to form an arch
    public float maxRotation = 10f; // Maximum Z rotation angle for edge cards (degrees)
    
    public Transform activeHandle;
    public Transform inactiveHandle;

    public override async Task LayoutCardsTask(bool animated = true)
    {
        var overlap = entitySeparation;
        var width = (cards.Count - 1) * overlap;
        var xPos = -width / 2f;
        var duration = animated ? 0.4f : 0;

        List<Task> tweenTasks = new List<Task>();

        for (int i = 0; i < cards.Count; ++i)
        {
            var canvas = cards[i].GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = baseSortOrder + i;
            }

            // Calculate normalized position from center (-1 for leftmost, 0 for center, 1 for rightmost)
            float normalizedIndex = cards.Count > 1
                ? (2f * i / (cards.Count - 1)) - 1f
                : 0f;

            // 1. Calculate Z-Rotation (-maxRotation on far right, +maxRotation on far left)
            float zRotation = -normalizedIndex * maxRotation;

            // 2. Calculate Y-Offset for the Arch (Parabolic curve: highest at center, drops at edges)
            float yOffset = -Mathf.Pow(Mathf.Abs(normalizedIndex), 2) * arcHeight;

            // Combine base handle position with calculated offsets
            Vector3 targetPosition = originPoint.position + new Vector3(xPos, yOffset, 0);
            Vector3 targetRotation = originPoint.rotation.eulerAngles + new Vector3(0, 0, zRotation);

            // Apply Tweens
            cards[i].DORotate(targetRotation, duration);
            Tweener moveTweener = cards[i].DOMove(targetPosition, duration);

            if (moveTweener != null && animated)
            {
                tweenTasks.Add(moveTweener.AsyncWaitForCompletion());
            }

            xPos += overlap;
        }

        if (tweenTasks.Count > 0)
        {
            await Task.WhenAll(tweenTasks);
        }
    }

    public override async Task HandleCardsAdd(MoveToZoneEvent moveToZoneEvent)
    {
        foreach (Entity entity in moveToZoneEvent.movedEntities)
        {
            SetCardEntityOriginalPos(entity, moveToZoneEvent);
            await HandleAddCard(entity, true, false);
        }
        foreach (Entity entity in moveToZoneEvent.overflownEntities)
        {
            SetCardEntityOriginalPos(entity, moveToZoneEvent);
            await HandleAddCard(entity, true, true);
        }
    }
}