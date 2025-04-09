using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PostEffects.Scripts
{
    public class PostEffectChainEditor : EditorWindow
    {
        private readonly List<BaseEffect> _effectChain = new();
        private Vector2 _scrollPos;
        private string _fileName = "nier.png";
        private string _inputPath = "Assets/PostEffects/Test/Input/";
        private string _outputPath = "Assets/PostEffects/Test/Output/";
        private string _outputFilePrefix = "";

        [MenuItem("Tools/Post Effect Chain")]
        public static void ShowWindow()
        {
            GetWindow<PostEffectChainEditor>("Post Effect Chain");
        }

        void OnGUI()
        {
            GUILayout.Label("Input/Output Settings", EditorStyles.boldLabel);
            _fileName = EditorGUILayout.TextField("File Name", _fileName);
            _inputPath = EditorGUILayout.TextField("Input Path", _inputPath);
            _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);
            _outputFilePrefix = EditorGUILayout.TextField("Output File Prefix", _outputFilePrefix);

            EditorGUILayout.Space();
            GUILayout.Label("Effect Chain", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            int remove = -1;
            
            for (int i = 0; i < _effectChain.Count; i++) {
                EditorGUILayout.BeginVertical("box");

                _effectChain[i] = EditorGUILayout.ObjectField(
                    $"Effect {i + 1}",
                    _effectChain[i],
                    typeof(BaseEffect),
                    false
                ) as BaseEffect;
                
                if (_effectChain[i] is not null) {
                    Editor editor = Editor.CreateEditor(_effectChain[i]);
                    editor.OnInspectorGUI();
                }
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("▲", GUILayout.Width(30)) && i > 0) {
                    (_effectChain[i], _effectChain[i - 1]) = (_effectChain[i - 1], _effectChain[i]);
                }

                if (GUILayout.Button("▼", GUILayout.Width(30)) && i < _effectChain.Count - 1) {
                    (_effectChain[i], _effectChain[i + 1]) = (_effectChain[i + 1], _effectChain[i]);
                }

                if (GUILayout.Button("Remove")) {
                    remove = i;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (remove >= 0) {
                _effectChain.RemoveAt(remove);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Effect Slot"))
            {
                _effectChain.Add(null);
            }

            if (GUILayout.Button("Clear Chain"))
            {
                if (EditorUtility.DisplayDialog("Clear Effect Chain", "Are you sure you want to clear the chain?", "Yes", "No"))
                {
                    _effectChain.Clear();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Run Chain"))
            { 
                EffectProcessor.RunChain(_inputPath, _outputPath, _fileName, _effectChain, "postfx_" + _outputFilePrefix + "_");
            }
        }
    }
}
