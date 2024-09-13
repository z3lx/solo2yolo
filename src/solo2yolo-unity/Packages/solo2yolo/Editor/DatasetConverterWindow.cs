using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace z3lx.solo2yolo
{
    public class DatasetConverterWindow : EditorWindow
    {
        private static readonly string _windowTitle = "Converter";
        private Texture2D _folderIcon;

        private Vector2 _margins = new Vector2(4, 4);
        private float _lineHeight = 24;
        private float _buttonHeight = 20;
        private float _labelWidth = 144;
        private float _buttonWidth = 96;
        private float _iconWidth = 24;

        public enum ComputerVisionTask
        {
            Classify,
            Detect,
            Segment,
            Pose
        }

        private string _soloPath;
        private string _yoloPath;
        private ComputerVisionTask _task;

        [MenuItem("Tools/solo2yolo Converter")]
        private static void ShowWindow()
        {
            DatasetConverterWindow window = GetWindow<DatasetConverterWindow>();
            window.Initialize();
        }

        private void Initialize()
        {
            titleContent = new GUIContent(_windowTitle);
            minSize = new Vector2((2 * _margins.y) + _labelWidth + _iconWidth + 432 + _buttonWidth, (2 * _margins.y) + (3 * _lineHeight) + _buttonHeight);

            string defaultPath = Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName, "LocalLow", PlayerSettings.companyName, PlayerSettings.productName);
            if (Directory.Exists(defaultPath))
            {
                _soloPath = Path.Combine(defaultPath, "solo");
                _yoloPath = defaultPath;
            }
            _task = ComputerVisionTask.Detect;
            _folderIcon = EditorGUIUtility.FindTexture("d_FolderOpened Icon");
        }

        private void OnGUI()
        {
            Rect soloLabelRect = new Rect(_margins.x, _margins.y, _labelWidth, _lineHeight);
            Rect soloIconRect = new Rect(_margins.x + _labelWidth, _margins.y, _iconWidth, _lineHeight);
            Rect soloPathRect = new Rect(_margins.x + _labelWidth + _iconWidth, _margins.y, position.width - (2 * _margins.x) - _labelWidth - _iconWidth - _buttonWidth, _lineHeight);
            Rect soloButtonRect = new Rect(position.width - _margins.y - _buttonWidth, _margins.y + + ((_lineHeight - _buttonHeight) / 2), _buttonWidth, _buttonHeight);

            Rect yoloLabelRect = new Rect(_margins.x, _margins.y + _lineHeight, _labelWidth, _lineHeight);
            Rect yoloIconRect = new Rect(_margins.x + _labelWidth, _margins.y + _lineHeight, _iconWidth, _lineHeight);
            Rect yoloPathRect = new Rect(_margins.x + _labelWidth + _iconWidth, _margins.y + _lineHeight, position.width - (2 * _margins.x) - _labelWidth - _iconWidth - _buttonWidth, _lineHeight);
            Rect yoloButtonRect = new Rect(position.width - _margins.y - _buttonWidth, _margins.y + _lineHeight + ((_lineHeight - _buttonHeight) / 2), _buttonWidth, _buttonHeight);

            Rect yoloTaskLabelRect = new Rect(_margins.x, _margins.y + (2 * _lineHeight), _labelWidth, _lineHeight);
            Rect yoloTaskEnumRect = new Rect(_margins.x + _labelWidth, _margins.y + (2 * _lineHeight) + ((_lineHeight - _buttonHeight) / 2), position.width - (2 * _margins.x) - _labelWidth, _buttonHeight);
            Rect confirmButtonRect = new Rect(position.width - _margins.y - _buttonWidth, position.height - _margins.x - _buttonHeight, _buttonWidth, _buttonHeight);

            GUI.Label(soloLabelRect, "SOLO directory");
            GUI.Label(soloIconRect, _folderIcon);
            GUI.Label(soloPathRect, _soloPath);
            if (GUI.Button(soloButtonRect, "Choose Folder"))
            {
                string input = EditorUtility.OpenFolderPanel("Select SOLO directory", "", "");
                _soloPath = string.IsNullOrEmpty(input) ? _soloPath : input.Replace('/', Path.DirectorySeparatorChar);
            }

            GUI.Label(yoloLabelRect, "YOLO directory");
            GUI.Label(yoloIconRect, _folderIcon);
            GUI.Label(yoloPathRect, _yoloPath);
            if (GUI.Button(yoloButtonRect, "Choose Folder"))
            {
                string input = EditorUtility.OpenFolderPanel("Select YOLO directory", "", "");
                _yoloPath = string.IsNullOrEmpty(input) ? _yoloPath : input.Replace('/', Path.DirectorySeparatorChar);
            }

            GUI.Label(yoloTaskLabelRect, "YOLO task");
            EditorGUI.BeginDisabledGroup(true);
            _task = (ComputerVisionTask)EditorGUI.EnumPopup(yoloTaskEnumRect, _task);
            EditorGUI.EndDisabledGroup();
            if (GUI.Button(confirmButtonRect, "Confirm"))
            {
                string path = Path.GetFullPath(Path.Combine("Packages", "com.z3lx.solo2yolo", "Runtime"));
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        path = Path.Combine(path, "solo2yolo-win-x64.exe");
                        break;
                    
                    case RuntimePlatform.OSXEditor:
                        path = Path.Combine(path, "solo2yolo-osx-x64");
                        break;

                    case RuntimePlatform.LinuxEditor:
                        path = Path.Combine(path, "solo2yolo-linux-x64");
                        break;

                    default:
                        throw new PlatformNotSupportedException("Unsupported platform");
                }
                System.Diagnostics.Process.Start(path, $"-i {_soloPath} -o {_yoloPath} -t {_task}");
            }

        }
    }
}