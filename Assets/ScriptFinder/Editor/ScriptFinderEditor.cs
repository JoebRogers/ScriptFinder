using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptFinder
{
    public sealed class ScriptFinderEditor : EditorWindow
    {
        #region Fields
        #region Const / Static
        #region Labels
        private const string LabelFindComponentsHeader = "Matches:";
        private const string LabelFindComponentsFail = "No matches found.";
        private const string LabelShouldRecurse = "Recurse Dependencies (Warning: Very Slow)";

        private const string LabelButtonFindComponents = "Find Components";
        private const string LabelButtonSelect = "Select";

        private const string LabelMiscWindowPath = "Utilities/ScriptFinder";
        #endregion
        #region Misc
        private static readonly Vector2 EditorWindowSize = new Vector2
        {
            x = 800.0f,
            y = 400.0f
        };

        private static readonly GUILayoutOption[] LayoutOptionsButtonSelect = new GUILayoutOption[]
        {
            GUILayout.Width(75.0f)
        };
        #endregion
        #endregion

        #region Private
        #region Styles
        private GUISkin skin;
        private GUIStyle styleHeader;
        private GUIStyle styleObjectFieldLabel;
        private GUIStyle styleRecurseLabel;
        private GUIStyle styleButtonLabel;
        private GUIStyle styleResultLabel;
        private GUIStyle styleResultListLabel;
        private GUIStyle styleScrollView;
        #endregion

        private MonoScript targetComponent;
        private List<string> results;

        private Vector2 scrollPosition = Vector2.zero;
        private bool shouldRecurse = false;
        #endregion
        #endregion

        [MenuItem(LabelMiscWindowPath)]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (ScriptFinderEditor)GetWindow(typeof(ScriptFinderEditor));
            window.minSize = EditorWindowSize;
            window.maxSize = EditorWindowSize;
            window.Show();
        }

        void OnEnable()
        {
            skin = (GUISkin)Resources.Load("ScriptFinderSkin");
            styleHeader = skin.GetStyle("Header");
            styleObjectFieldLabel = skin.GetStyle("ObjectFieldLabel");
            styleRecurseLabel = skin.GetStyle("RecurseLabel");
            styleButtonLabel = skin.GetStyle("ButtonLabel");
            styleResultLabel = skin.GetStyle("ResultLabel");
            styleResultListLabel = skin.GetStyle("ResultListLabel");
            styleScrollView = skin.GetStyle("ScrollView");
        }

        void OnGUI()
        {
            //Header Background
            EditorGUI.DrawRect(new Rect(0, 0, 800, 30), new Color32(60, 60, 60, 255));
            //Body Background
            EditorGUI.DrawRect(new Rect(0, 30, 800, 370), new Color32(90, 90, 90, 255));
            EditorGUI.DrawRect(new Rect(10, 40, 780, 350), new Color32(60, 60, 60, 255));
            EditorGUI.DrawRect(new Rect(12, 42, 776, 346), new Color32(70, 70, 70, 255));
            //Results Background
            EditorGUI.DrawRect(new Rect(20, 225, 760, 155), new Color32(60, 60, 60, 255));
            EditorGUI.DrawRect(new Rect(22, 227, 756, 151), new Color32(90, 90, 90, 255));

            EditorGUILayout.LabelField("ScriptFinder", styleHeader);

            EditorGUILayout.LabelField("Select Target Script", styleObjectFieldLabel);
            targetComponent = (MonoScript)EditorGUI.ObjectField(new Rect(20.0f, 80.0f, 762.0f, 17.0f), targetComponent, typeof(MonoScript), false);

            EditorGUILayout.LabelField(LabelShouldRecurse, styleRecurseLabel);
            shouldRecurse = EditorGUI.Toggle(new Rect(18, 135, 20, 20), shouldRecurse);

            if (GUI.Button(new Rect(20.0f, 160.0f, 760.0f, 20.0f), ""))
            {
                ActionSearchForComponent();
            }

            EditorGUILayout.LabelField(LabelButtonFindComponents, styleButtonLabel);

            if (results != null)
            {
                if (results.Count == 0)
                {
                    EditorGUILayout.LabelField(LabelFindComponentsFail, styleResultLabel);
                }
                else
                {
                    EditorGUILayout.LabelField(LabelFindComponentsHeader, styleResultLabel);
                    GUILayout.BeginArea(new Rect(22.0f, 227.0f, 756.0f, 151.0f));
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, styleScrollView);
                    foreach (string s in results)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(s, styleResultListLabel);
                        if (GUILayout.Button(LabelButtonSelect))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(s);
                        }
                        EditorGUILayout.EndHorizontal();
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
    }
}