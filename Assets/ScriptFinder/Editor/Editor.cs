using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ScriptFinder.Utilities;

namespace ScriptFinder
{
    public sealed class Editor : EditorWindow
    {
        private sealed class Data
        {
            public struct Colours
            {
                public static readonly Color BackgroundHeader        = new Color32(60, 60, 60, 255);
                public static readonly Color BackgroundBodyOuter     = new Color32(90, 90, 90, 255);
                public static readonly Color BackgroundBodyBorder    = new Color32(60, 60, 60, 255);
                public static readonly Color BackgroundBodyInner     = new Color32(70, 70, 70, 255);
                public static readonly Color BackgroundResultsBorder = new Color32(60, 60, 60, 255);
                public static readonly Color BackgroundResultsInner  = new Color32(90, 90, 90, 255);

                public static readonly Color ButtonMatchNormalBackground = new Color32(40, 90, 90, 255);
                public static readonly Color ButtonMatchNormalBorder = new Color32(0, 0, 0, 255);
                public static readonly Color ButtonMatchNormalText = new Color32(255, 255, 255, 255);
                public static readonly Color ButtonMatchHoverBackground = new Color32(90, 90, 90, 255);
                public static readonly Color ButtonMatchHoverBorder = new Color32(90, 90, 90, 255);
                public static readonly Color ButtonMatchHoverText = new Color32(255, 255, 255, 255);
            }
            public struct Labels
            {
                public const string HeaderFindComponentsSuccess = "Matches:";
                public const string HeaderFindComponentsFail = "No matches found.";

                public const string FieldShouldRecurse = "Recurse Dependencies (Warning: Very Slow)";

                public const string ButtonFindMatches = "Find Components";
                public const string ButtonSelect = "Select";

                public const string MiscWindowPath = "Utilities/ScriptFinder";
            }
            public struct LayoutOptions
            {
                public static readonly GUILayoutOption[] ButtonMatch = new GUILayoutOption[]
                {
                    GUILayout.Width(Rects.ButtonMatch.width),
                    GUILayout.Height(Rects.ButtonMatch.height)
                };
            }
            public struct Rects
            {
                public static readonly Rect BackgroundHeader        = new Rect(0.0f, 0.0f, 800.0f, 30.0f);
                public static readonly Rect BackgroundBodyOuter     = new Rect(0.0f, 30.0f, 800.0f, 370.0f);
                public static readonly Rect BackgroundBodyBorder    = new Rect(10.0f, 40.0f, 780.0f, 350.0f);
                public static readonly Rect BackgroundBodyInner     = new Rect(12.0f, 42.0f, 776.0f, 346.0f);
                public static readonly Rect BackgroundResultsBorder = new Rect(20.0f, 225.0f, 760.0f, 155.0f);
                public static readonly Rect BackgroundResultsInner  = new Rect(22.0f, 227.0f, 756.0f, 151.0f);

                public static readonly Rect FieldTargetObject  = new Rect(20.0f, 80.0f, 762.0f, 17.0f);
                public static readonly Rect FieldRecurseToggle = new Rect(18.0f, 135.0f, 20.0f, 20.0f);

                public static readonly Rect ButtonFindMatches = new Rect(20.0f, 160.0f, 760.0f, 20.0f);
                public static readonly Rect ButtonMatch       = new Rect(0.0f, 0.0f, 740.0f, 30.0f);

                public static readonly Rect AreaResults = new Rect(22.0f, 227.0f, 756.0f, 151.0f);
            }
            
            public struct StyleStates
            {
                private const int ButtonMatchBorderThickness = 4;

                public static readonly GUIStyleState ButtonMatchNormal = new GUIStyleState
                {
                    background = Utility.GenerateColouredBackgroundWithBottomBorder
                    (
                        (int)Rects.ButtonMatch.width, 
                        (int)Rects.ButtonMatch.height, 
                        Colours.ButtonMatchNormalBackground, 
                        Colours.ButtonMatchNormalBorder, 
                        ButtonMatchBorderThickness
                    ),                    
                    textColor = Colours.ButtonMatchNormalText
                };

                public static readonly GUIStyleState ButtonMatchHover = new GUIStyleState
                {
                    background = Utility.GenerateColouredBackgroundWithBottomBorder
                    (
                        (int)Rects.ButtonMatch.width,
                        (int)Rects.ButtonMatch.height,
                        Colours.ButtonMatchHoverBackground,
                        Colours.ButtonMatchHoverBorder,
                        ButtonMatchBorderThickness
                    ),
                    textColor = Colours.ButtonMatchHoverText
                };
            }
            public struct Styles
            {
                public static readonly GUIStyle ButtonMatch = new GUIStyle
                {
                    normal   = StyleStates.ButtonMatchNormal,
                    onNormal = StyleStates.ButtonMatchNormal,
                    hover    = StyleStates.ButtonMatchHover,
                    onHover  = StyleStates.ButtonMatchHover
                };
            }

            public static readonly Vector2 WindowSize = new Vector2
            {
                x = 800.0f,
                y = 400.0f
            };
        }

        #region Fields
        #region Styles
        private GUISkin skin;
        private GUIStyle styleHeader;
        private GUIStyle styleObjectFieldLabel;
        private GUIStyle styleRecurseLabel;
        private GUIStyle styleButtonLabel;
        private GUIStyle styleResultLabel;
        private GUIStyle styleResultListLabel;
        #endregion

        private MonoScript targetComponent;
        private List<string> results;

        private Vector2 scrollPosition = Vector2.zero;
        private bool shouldRecurse = false;
        #endregion

        [MenuItem(Data.Labels.MiscWindowPath)]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (Editor)GetWindow(typeof(Editor));
            window.minSize = Data.WindowSize;
            window.maxSize = Data.WindowSize;
            window.Show();
        }

        void OnEnable()
        {
            wantsMouseMove = true;

            skin = (GUISkin)Resources.Load("ScriptFinderSkin");
            styleHeader = skin.GetStyle("Header");
            styleObjectFieldLabel = skin.GetStyle("ObjectFieldLabel");
            styleRecurseLabel = skin.GetStyle("RecurseLabel");
            styleButtonLabel = skin.GetStyle("ButtonLabel");
            styleResultLabel = skin.GetStyle("ResultLabel");
            styleResultListLabel = skin.GetStyle("ResultListLabel");
        }

        void OnGUI()
        {
            HandleEvents();

            //Header Background
            EditorGUI.DrawRect(Data.Rects.BackgroundHeader, Data.Colours.BackgroundHeader);
            //Body Background
            EditorGUI.DrawRect(Data.Rects.BackgroundBodyOuter, Data.Colours.BackgroundBodyOuter);
            EditorGUI.DrawRect(Data.Rects.BackgroundBodyBorder, Data.Colours.BackgroundBodyBorder);
            EditorGUI.DrawRect(Data.Rects.BackgroundBodyInner, Data.Colours.BackgroundBodyInner);
            //Results Background
            EditorGUI.DrawRect(Data.Rects.BackgroundResultsBorder, Data.Colours.BackgroundResultsBorder);
            EditorGUI.DrawRect(Data.Rects.BackgroundResultsInner, Data.Colours.BackgroundResultsInner);

            EditorGUILayout.LabelField("ScriptFinder", styleHeader);

            EditorGUILayout.LabelField("Select Target Script", styleObjectFieldLabel);
            targetComponent = (MonoScript)EditorGUI.ObjectField(Data.Rects.FieldTargetObject, targetComponent, typeof(MonoScript), false);

            EditorGUILayout.LabelField(Data.Labels.FieldShouldRecurse, styleRecurseLabel);
            shouldRecurse = EditorGUI.Toggle(Data.Rects.FieldRecurseToggle, shouldRecurse);

            if (GUI.Button(Data.Rects.ButtonFindMatches, ""))
            {
                ActionSearchForComponent();
            }

            EditorGUILayout.LabelField(Data.Labels.ButtonFindMatches, styleButtonLabel);

            if (results != null)
            {
                if (results.Count == 0)
                {
                    EditorGUILayout.LabelField(Data.Labels.HeaderFindComponentsFail, styleResultLabel);
                }
                else
                {
                    EditorGUILayout.LabelField(Data.Labels.HeaderFindComponentsSuccess, styleResultLabel);
                    GUILayout.BeginArea(Data.Rects.AreaResults);
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    foreach (string s in results)
                    {
                        if (GUILayout.Button(s, Data.Styles.ButtonMatch, Data.LayoutOptions.ButtonMatch))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(s);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndArea();
                }
            }
        }

        public static string[] GetAllPrefabs()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (s.Contains(".prefab"))
                {
                    result.Add(s);
                }
            }
            return result.ToArray();
        }

        private void ActionSearchForComponent()
        {
            string targetPath = AssetDatabase.GetAssetPath(targetComponent);
            string[] allPrefabs = GetAllPrefabs();
            results = new List<string>();

            foreach (string prefab in allPrefabs)
            {
                string[] single = new string[] { prefab };
                string[] dependencies = AssetDatabase.GetDependencies(single, shouldRecurse);
                foreach (string dependedAsset in dependencies)
                {
                    if (dependedAsset == targetPath)
                    {
                        results.Add(prefab);
                    }
                }
            }
        }

        #region Input Events
        private void HandleEvents()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.MouseMove:
                    OnMouseMove();
                    break;
            }
        }

        #region Mouse Events
        private void OnMouseMove()
        {
            Repaint();
        }
        #endregion
        #endregion
    }
}