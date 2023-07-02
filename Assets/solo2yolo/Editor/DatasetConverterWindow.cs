using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace z3lx.solo2yolo
{
    public class DatasetConverterWindow : EditorWindow
    {
        private string _soloPath;
        private string _yoloPath;
        private ComputerVisionTask _task = ComputerVisionTask.Detect;

        [MenuItem("Tools/solo2yolo Converter")]
        private static void ShowWindow()
        {
            GetWindow(typeof(DatasetConverterWindow));
        }

        private void OnGUI()
        {
            _soloPath = EditorGUILayout.TextField("SOLO directory", _soloPath);
            _yoloPath = EditorGUILayout.TextField("Output directory", _yoloPath);
            if (GUILayout.Button("Convert"))
                DatasetConverter.Convert(_soloPath, _yoloPath, _task);
        }
    }
}