﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs
{
    [CustomEditor(typeof(MultiMaterialData))]
    public class MultiMaterialDataEditor : Editor
    {
        MaterialEditor[] m_MaterialEditors;
        SerializedProperty m_MultiArray;
        SerializedProperty m_Materials;
        
        void OnEnable()
        {
            m_MultiArray = serializedObject.FindProperty(MultiMaterialData.materialArrayPub);
            m_Materials = m_MultiArray.FindPropertyRelative(MaterialArray.materialsPub);
            m_MaterialEditors = new MaterialEditor[] {};
            MaterialArrayDrawers.UpdateShaderNames();
        }

        public override void OnInspectorGUI()
        {
            var targetData = target as MultiMaterialData;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            var materialPropList = new List<SerializedProperty>();
            m_Materials.arraySize = EditorGUILayout.DelayedIntField("Size", m_Materials.arraySize);
            serializedObject.ApplyModifiedProperties();
            for (var i = 0; i < m_Materials.arraySize; ++i)
            {
                materialPropList.Add(m_Materials.GetArrayElementAtIndex(i));
            }
            var materialProperties = materialPropList.ToArray();
            serializedObject.ApplyModifiedProperties();
            var changed = EditorGUI.EndChangeCheck();

            MaterialArrayDrawers.OnInspectorGUI(serializedObject,
                targetData.materialArrayData, ref m_MaterialEditors, changed, materialProperties);

            var targetArray = targetData.materialArrayData;

            if (GUILayout.Button("Add Selected"))
            {
                serializedObject.Update();
                var matHash = new HashSet<Material>();
                // Use serialized property array size since targetArray could be null when object if first created.
                if (m_Materials.arraySize > 0)
                {
                    foreach (var mat in targetArray.materials)
                    {
                        matHash.Add(mat);
                    }
                }
                foreach (var obj in Selection.objects)
                {
                    var mat = obj as Material;
                    if (mat != null)
                    {
                        matHash.Add(mat);
                    }
                }

                // When first created the material array will be null.
                // Changing the size will create the material array.
                if (m_Materials.arraySize < 1)
                {
                    m_Materials.arraySize = matHash.Count;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    targetArray = targetData.materialArrayData;
                }
                targetArray.materials = matHash.ToArray();

                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Select Materials"))
            {
                if (targetArray != null && targetArray.materials != null && targetArray.materials.Length > 0)
                    Selection.objects = targetArray.materials;
            }
        }

        public override bool HasPreviewGUI()
        {
            if (m_MaterialEditors != null && m_MaterialEditors.Length > 0 && m_MaterialEditors[0] != null)
            {
                return true;
            }
            return base.HasPreviewGUI();
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (m_MaterialEditors != null && m_MaterialEditors.Length > 0 && m_MaterialEditors[0] != null)
            {
                HasPreviewGUI();
                m_MaterialEditors[0].OnInteractivePreviewGUI(r, background);
            }
            else
            {
                base.OnInteractivePreviewGUI(r, background);
            }
        }

        void OnDestroy()
        {
            if (m_MaterialEditors != null)
            {
                foreach (var materialEditor in m_MaterialEditors)
                {
                    DestroyImmediate(materialEditor);
                }
                m_MaterialEditors = null;
            }
        }
    }
}
