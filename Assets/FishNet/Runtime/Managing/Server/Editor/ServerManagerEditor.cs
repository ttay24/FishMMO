﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FishNet.Managing.Server.Editing
{


    [CustomEditor(typeof(ServerManager), true)]
    [CanEditMultipleObjects]
    public class ServerManagerEditor : Editor
    {
        private SerializedProperty _authenticator;
        private SerializedProperty _syncTypeRate;
        private SerializedProperty SpawnPacking;
        private SerializedProperty _changeFrameRate;
        private SerializedProperty _frameRate;
        private SerializedProperty _shareIds;
        private SerializedProperty _startOnHeadless;
        private SerializedProperty _limitClientMTU;

        protected virtual void OnEnable()
        {
            _authenticator = serializedObject.FindProperty(nameof(_authenticator));
            _syncTypeRate = serializedObject.FindProperty(nameof(_syncTypeRate));
            SpawnPacking = serializedObject.FindProperty(nameof(SpawnPacking));
            _changeFrameRate = serializedObject.FindProperty(nameof(_changeFrameRate));
            _frameRate = serializedObject.FindProperty(nameof(_frameRate));
            _shareIds = serializedObject.FindProperty(nameof(_shareIds));
            _startOnHeadless = serializedObject.FindProperty(nameof(_startOnHeadless));
            _limitClientMTU = serializedObject.FindProperty(nameof(_limitClientMTU));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ServerManager)target), typeof(ServerManager), false);
            GUI.enabled = true;


            //EditorGUILayout.PropertyField(_authenticator);
            EditorGUILayout.PropertyField(_syncTypeRate);
            EditorGUILayout.PropertyField(SpawnPacking);
            EditorGUILayout.PropertyField(_changeFrameRate);
            if (_changeFrameRate.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_frameRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(_shareIds);
            EditorGUILayout.PropertyField(_startOnHeadless);
            EditorGUILayout.PropertyField(_limitClientMTU);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif