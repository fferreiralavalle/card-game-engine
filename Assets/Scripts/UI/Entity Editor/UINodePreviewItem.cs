using RuntimeCardEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINodePreviewItem : MonoBehaviour
{
    public TextMeshProUGUI nodeName;
    public Image background;
    public NodeTemplate nodeTemplate;

    public UINodePreviewItem Init(NodeTemplate nodeTemplate)
    {
        this.nodeTemplate = nodeTemplate;
        nodeName.text = nodeTemplate.nodeName;
        Color backgroundColor;
        if (ColorUtility.TryParseHtmlString(nodeTemplate.headerColorHex, out backgroundColor))
        {
            background.color = backgroundColor;
        }
        else
        {
            Debug.LogError("Invalid hex color format.");
        }
        return this;
    }
}
