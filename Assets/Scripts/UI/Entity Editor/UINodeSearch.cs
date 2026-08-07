using NUnit.Framework;
using RuntimeCardEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINodeSearch : MonoBehaviour
{
    public UINodePreviewItem nodePreviewItemPrefab;
    public GameObject nodesView;
    public Transform nodeList;

    public Dictionary<string, NodeTemplate> nodeTemplates;

    public UnityEvent<NodeTemplate> OnNodeSelected;

    private void Start()
    {
        Init(NodeTemplateManager.Instance.GetNodeTemplates());
    }

    public void Init(Dictionary<string, NodeTemplate> nodeTemplates)
    {
        this.nodeTemplates = nodeTemplates;
        foreach (var nodeTemplate in nodeTemplates.Values)
        {
            UINodePreviewItem item = Instantiate(nodePreviewItemPrefab, nodeList);
            item.Init(nodeTemplate);
            item.GetComponent<Button>().onClick.AddListener(() => HandleSelect(nodeTemplate));
        }
    }

    public void HandleSelect(NodeTemplate nodeTemplate)
    {
        OnNodeSelected?.Invoke(nodeTemplate);
    }

    public void UpdateFilter(string filter)
    {
        foreach (Transform child in nodeList)
        {
            UINodePreviewItem item = child.GetComponent<UINodePreviewItem>();
            if (item != null)
            {
                bool shouldShow = string.IsNullOrEmpty(filter) || item.nodeTemplate.nodeName.ToLower().Contains(filter.ToLower());
                item.gameObject.SetActive(shouldShow);
            }
        }
    }

    public void ShowList()
    {
        nodesView.SetActive(true);
    }
    public void HideList()
    {
        nodesView.SetActive(false);
    }
}
