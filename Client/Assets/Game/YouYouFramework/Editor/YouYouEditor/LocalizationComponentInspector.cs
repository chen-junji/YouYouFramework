using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YouYou
{
    [CustomEditor(typeof(LocalizationManager),true)]
    public class LocalizationComponentInspector : Editor
    {
        private SerializedProperty m_CurrLanguage;

        void OnEnable()
        {
            m_CurrLanguage = serializedObject.FindProperty("m_CurrLanguage");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_CurrLanguage);
            serializedObject.ApplyModifiedProperties();
        }
    }
}