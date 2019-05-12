using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NurbsUnity
{
    [CustomEditor(typeof(Surface))]
    public class SurfaceEditor : Editor
    {
        Surface myTarget;
        SerializedProperty controlPointTransforms;
        SerializedProperty controlPointsParent;
        SerializedProperty knotsU;
        SerializedProperty knotsV;

        private void OnEnable()
        {
            myTarget = (Surface)target;
            controlPointTransforms = serializedObject.FindProperty("controlPointTransforms");
            controlPointsParent = serializedObject.FindProperty("controlPointsParent");
            knotsU = serializedObject.FindProperty("knotsU");
            knotsV = serializedObject.FindProperty("knotsV");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Surface Settings:", EditorStyles.boldLabel);
            myTarget.type = (SurfaceType)EditorGUILayout.EnumPopup("Surface Type:", myTarget.type);
            switch (myTarget.type)
            {
                case SurfaceType.BSPline:
                    myTarget.degreeU = EditorGUILayout.IntField("Degree U:", myTarget.degreeU);
                    myTarget.degreeV = EditorGUILayout.IntField("Degree V:", myTarget.degreeV);
                    break;
                default:
                    break;
            }
            myTarget.resolution = EditorGUILayout.FloatField("Resolution:", myTarget.resolution);
            myTarget.transforms = (Transforms)EditorGUILayout.EnumPopup("Control Points Provider:", myTarget.transforms);
            switch (myTarget.transforms)
            {
                case Transforms.AsList:
                    EditorGUILayout.PropertyField(controlPointTransforms, new GUIContent("Control Point Transform"), true);
                    break;
                case Transforms.FromParent:
                    EditorGUILayout.PropertyField(controlPointsParent, new GUIContent("Control Points Parent"));
                    break;
            }
            myTarget.initializaOnStart = EditorGUILayout.Toggle("Initialize On Start:", myTarget.initializaOnStart);
            myTarget.autoUpdate = EditorGUILayout.Toggle("Auto-Update:", myTarget.autoUpdate);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
