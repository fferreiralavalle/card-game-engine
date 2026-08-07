using UnityEngine;

public class UIPlayCost : MonoBehaviour
{
    public UIResource resourcePrefab;
    public RectTransform costContiner;

    public PlayCost playCost;

    public UIPlayCost Initiate(PlayCost playCost)
    {
        this.playCost = playCost;

        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        foreach(string resourceId in playCost.costs.Keys)
        {
            Resource r = playCost.costs[resourceId];
            if (r.GetAmount() > 0)
                Instantiate(resourcePrefab, costContiner).Initiate(r);
        }

        return this;
    }
}
