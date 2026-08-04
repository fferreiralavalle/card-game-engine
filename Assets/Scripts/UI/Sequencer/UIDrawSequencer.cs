using RuntimeCardEngine;
using UnityEngine;

public class UIDrawSequencer : MonoBehaviour
{
    public VisualSequencer visualSequencer;

    public void OnDrawEventFired(DrawEvent drawEvent)
    {
        // Fetch visual representations of objects
        // UICardEntity cardView = UIVisualManager.Instance.GetCardEntity();
        UIZone handZone = UIVisualManager.Instance.GetZone(CommonZones.HAND, drawEvent.handOwnerId);

        // Queue the visual movement task
        visualSequencer.EnqueueAnimation(async () =>
        {
            // Smoothly animate card from Deck position to Hand position
            // await cardView.AnimateToHandAsync(handContainer);
        });
    }
}
