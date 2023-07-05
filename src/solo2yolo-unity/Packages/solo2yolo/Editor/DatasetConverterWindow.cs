using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace z3lx.solo2yolo
{
    internal class DatasetConverterWindow : EditorWindow
    {
        private static readonly string _windowTitle = "Converter";
        private Texture2D _folderIcon;

        private readonly Vector2 _margins = new Vector2(4, 4);
        private readonly float _lineHeight = 24;
        private readonly float _buttonHeight = 20;
        private readonly float _labelWidth = 144;
        private readonly float _buttonWidth = 96;
        private readonly float _iconWidth = 24;

        private enum ComputerVisionTask
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
            minSize = new Vector2((2 * _margins.x) + _labelWidth + _iconWidth + 384 + _buttonWidth,
                (2 * _margins.y) + (3 * _lineHeight) + _buttonHeight);
            _folderIcon = EditorGUIUtility.FindTexture("d_FolderOpened Icon");

            string defaultPath = Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName,
                "LocalLow", PlayerSettings.companyName, PlayerSettings.productName);

            _soloPath = Path.Combine(defaultPath, "solo");
            _yoloPath = defaultPath;
            _task = ComputerVisionTask.Detect;
        }

        private void OnGUI()
        {
            Rect contentRect = new Rect(_margins.x, _margins.y,
                position.width - (2 * _margins.x), position.height - (2 * _margins.y));
            GUILayout.BeginArea(contentRect);

            RectLayout layout = new RectLayout()
            {
                lineHeight = _lineHeight,
                contentRect = contentRect
            };

            DrawPathInput(layout, "SOLO directory", ref _soloPath);
            DrawPathInput(layout, "YOLO directory", ref _yoloPath);
            DrawTaskInput(layout, "YOLO task");
            DrawConfirmButton(layout);

            GUILayout.EndArea();
        }

        private void DrawPathInput(RectLayout layout, string label, ref string path)
        {
            GUI.Label(layout.GetNextRect(_labelWidth), label);
            GUI.Label(layout.GetNextRect(_iconWidth), _folderIcon);
            GUI.Label(layout.GetNextRect(layout.contentRect.width - _labelWidth - _iconWidth - _buttonWidth), path);
            if (!GUI.Button(layout.GetNextRect(_buttonWidth, _buttonHeight), "Choose Folder"))
                return;

            string input = EditorUtility.OpenFolderPanel($"Select {label}", path, "");
            path = string.IsNullOrEmpty(input) ? path : input.Replace('/', Path.DirectorySeparatorChar);
            layout.NewLine();
        }

        private void DrawTaskInput(RectLayout layout, string label)
        {
            GUI.Label(layout.GetNextRect(_labelWidth), label);
            EditorGUI.BeginDisabledGroup(true);
            Rect taskRect = layout.GetNextRect(layout.contentRect.width - _labelWidth, EditorStyles.popup.fixedHeight);
            _task = (ComputerVisionTask)EditorGUI.EnumPopup(taskRect, _task);
            EditorGUI.EndDisabledGroup();
            layout.NewLine();
        }

        private void DrawConfirmButton(RectLayout layout)
        {
            Rect confirmButtonRect = new Rect(layout.contentRect.width - _buttonWidth,
                layout.contentRect.height - _buttonHeight, _buttonWidth, _buttonHeight);
            if (!GUI.Button(confirmButtonRect, "Confirm"))
                return;

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
                    throw new PlatformNotSupportedException("Unsupported platform.");
            }
            System.Diagnostics.Process.Start(path, $"-i {_soloPath} -o {_yoloPath} -t {_task}");
        }
    }
}