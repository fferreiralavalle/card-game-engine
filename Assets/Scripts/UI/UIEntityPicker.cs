using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class UIEntityPicker
{
    public static UICardEntity GetHoveredCardFromRaycast()
    {
        // 1. Create PointerEventData at current mouse position
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        // 2. Raycast against all UI elements
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 3. Find first UICardEntity hit
        foreach (RaycastResult result in results)
        {
            UICardEntity card = result.gameObject.GetComponentInParent<UICardEntity>();
            if (card != null)
            {
                return card;
            }
        }

        return null;
    }
}
