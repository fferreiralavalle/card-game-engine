using UnityEngine;
using RuntimeNodeEditor;

namespace RuntimeCardEngine

{
    public class ApplicationStartup : MonoBehaviour
    {
        public RectTransform editorHolder;
        public CardCreatorEditor editor;
        public UIGraph graph;

        private void Start()
        {
            Application.targetFrameRate = 60;
            // var graph = editor.CreateGraph<UIGraph>(editorHolder);
            editor.StartEditor(graph);
        }
    }
}