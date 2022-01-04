using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Map.Sample
{
    [CustomEditor(typeof(MapCreater))]
    public class MapCreaterEditor : Editor
    {
        private MapCreater _mapCreater;
        private void OnEnable()
        {
            _mapCreater = target as MapCreater;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create Map") || Event.current.keyCode == KeyCode.Return)
                _mapCreater.Update();
        }
    }
}