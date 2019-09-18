using System;
using UnityEngine;
using UnityEditor;

namespace CustomInputManager.Editor {
    
    public class InputManagerWindow : EditorWindow {

        [MenuItem("GameObject/Input Manager", false, 300)]
		static void OpenWindow () {
			EditorWindow settingsWindow = EditorWindow.GetWindow<InputManagerWindow>("Input Manager", true, Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"));
			settingsWindow.Focus();
		}

        public InputSettingsTab inputSettings = new InputSettingsTab();
        public ControlSchemesTab controlSchemes = new ControlSchemesTab();
        public GamepadProfilesTab gamepadProfilesWindow = new GamepadProfilesTab();
        
        public static InputManagerWindow instance;
        public static Rect pos { get { return instance.position; } }
        public static float width { get { return pos.width; } }
        public const float tabsOffYOffset = 45;
        
        Selection[] selections;
        bool wasDiabled;
        int selectedTab;
        Action[] tabGUIs { get { return new Action[] { inputSettings.OnGUI, controlSchemes.OnGUI, gamepadProfilesWindow.OnGUI }; } }
        public static Vector2 m_mainPanelScrollPos = Vector2.zero;

        public static int GetSelection(int index) {
            return instance.selections[index].selected;
        }
        public static void SetSelection(int index, int value) {
            instance.selections[index].selected = value;
        }
        void DisposeManagerWindow ( bool repeat) {
            instance = null;
            controlSchemes.Dispose( repeat);
            HierarchyGUI.Dispose();
        }
        public static void ResetSelections () {
            for (int i = 0; i < instance.selections.Length; i++) {
                instance.selections[i].Reset();
            }
        }
        public void OnPlayStateChanged (PlayModeStateChange state) {
            controlSchemes.OnPlayStateChanged(state);
            gamepadProfilesWindow.OnPlayStateChanged(state);
        }
        void OnEnable () {
            instance = this;
            selections = new Selection[2];
            for (int i = 0; i < 2; i++) selections[i] = new Selection();
            ResetSelections();
            ConvertUnityInputManager.ShowStartupWarning();
            controlSchemes.OnEnable();
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
        }

        void OnDisable () {
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            wasDiabled = true;
            DisposeManagerWindow(false);
        }
        void OnDestroy () {
            DisposeManagerWindow(wasDiabled);
        }
        void OnGUI() {
            for (int i = 0; i < 3; i++) EditorGUILayout.Space();
            selectedTab = GUILayout.Toolbar (selectedTab, new string[] {"Settings", "Schemes", "Gamepad Profiles"});
            tabGUIs[selectedTab] ();
		}
    }
        
    public class Selection {
        const int NONE = -1;
        public int selected;
        public Selection() { Reset(); }
        public void Reset() { selected = NONE; }
    }
}
