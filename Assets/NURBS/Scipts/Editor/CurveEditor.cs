using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NurbsUnity
{
    [CustomEditor(typeof(Curve))]
    public class CurveEditor : Editor
    {
        Curve myTarget;

        SerializedProperty controlPointTransforms;
        SerializedProperty controlPointsParent;
        SerializedProperty knots;

        private void OnEnable()
        {
            myTarget = (Curve)target;
            controlPointTransforms = serializedObject.FindProperty("controlPointTransforms");
            controlPointsParent = serializedObject.FindProperty("controlPointsParent");
            knots = serializedObject.FindProperty("knots");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Curve Settings:", EditorStyles.boldLabel);
            myTarget.type = (CurveType)EditorGUILayout.EnumPopup("Curve Type:", myTarget.type);
            switch (myTarget.type)
            {
                case CurveType.NURBS:
                    myTarget.degree = EditorGUILayout.IntField("Degree:", myTarget.degree);
                    break;
                default:
                    break;
            }
            myTarget.resolution = EditorGUILayout.FloatField("Resolution:", myTarget.resolution);
            myTarget.transforms = (Transforms)EditorGUILayout.EnumPopup("Control Points Provider:", myTarget.transforms);
            switch (myTarget.transforms)
            {
                case Transforms.AsList:
                    EditorGUILayout.PropertyField(controlPointTransforms, new GUIContent("Control Point Transform"),true);
                    break;
                case Transforms.FromParent:
                    EditorGUILayout.PropertyField(controlPointsParent, new GUIContent("Control Points Parent"));
                    break;
            }
            myTarget.initializeOnStart = EditorGUILayout.Toggle("Initialize On Start:", myTarget.initializeOnStart);
            myTarget.autoUpdate = EditorGUILayout.Toggle("Auto-Update:", myTarget.autoUpdate);
            if (myTarget.type == CurveType.NURBS)
            {
                EditorGUILayout.PropertyField(knots, new GUIContent("Knots"), true);
                if (GUILayout.Button("Get Knot Vector:"))
                {
                    switch (myTarget.transforms)
                    {
                        case Transforms.AsList:
                            myTarget.knots = NurbsUtils.Knots(myTarget.controlPointTransforms.Length, myTarget.degree);
                            break;
                        case Transforms.FromParent:
                            myTarget.knots = NurbsUtils.Knots(myTarget.controlPointsParent.childCount, myTarget.degree);
                            break;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
