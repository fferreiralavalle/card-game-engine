using RuntimeNodeEditor;
using System.Linq;
using UnityEngine;

public class UIGraph : NodeGraph
{
    public Node CreateWithInstance(string prefabPath)
    {
        var mousePosition = Utility.GetMousePosition();
        var pos = Utility.GetLocalPointIn(nodeContainer, mousePosition);

        Create(prefabPath, pos);

        var lastNode = nodes.Last();

        return lastNode;
    }

    public Node CreateWithInstance(string prefabPath, Vector2 position)
    {
        var pos = Utility.GetLocalPointIn(nodeContainer, position);

        Create(prefabPath, pos);

        var lastNode = nodes.Last();

        return lastNode;
    }
}
