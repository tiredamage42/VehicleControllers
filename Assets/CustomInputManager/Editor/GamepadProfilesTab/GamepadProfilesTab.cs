using UnityEngine;
using UnityEditor;

using CustomInputManager.Internal;

using System.Collections.Generic;

namespace CustomInputManager.Editor {

    [System.Serializable] public class GamepadProfilesTab
    {
		
		
		GamepadProfilesEditor profilesEditor = new GamepadProfilesEditor();
		[SerializeField] GamepadTestSceneEditor testSceneEditor = new GamepadTestSceneEditor();
        
		GenericGamepadProfile[] gamepadProfiles;
        SerializedObject profileSO;
		
		bool hasSelection { get { return selectedProfileIndex >= 0; } }
		int selectedProfileIndex { 
			get { return InputManagerWindow.GetSelection(0); } 
			set { InputManagerWindow.SetSelection(0, value); } 
		}

        


        void ReloadProfilesAndRepaint () {
			gamepadProfiles = GamepadHandler.LoadAllGamepadProfiles();
			InputManagerWindow.ResetSelections();
			InputManagerWindow.instance.Repaint();
		}

		
		void CreateEditMenu(Rect position)
		{
			GenericMenu editMenu = new GenericMenu();
			editMenu.AddItem(new GUIContent("New Gamepad Profile"), false, HandleEditMenuOption, 0);
			
            editMenu.AddSeparator("");

			if (hasSelection)
				editMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, 1);
			else
				editMenu.AddDisabledItem(new GUIContent("Duplicate"));

			if (hasSelection)			
				editMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, 2);
			else
				editMenu.AddDisabledItem(new GUIContent("Delete"));

			editMenu.DropDown(position);
		}

		void HandleEditMenuOption(object arg)
		{
			switch((int)arg)
			{
				case 0: profilesEditor.CreateNewGamepadProfile("New_Gamepad_Profile"); ReloadProfilesAndRepaint(); break;
				case 1: profilesEditor.DuplicateProfile(gamepadProfiles[selectedProfileIndex]); ReloadProfilesAndRepaint(); break;
				case 2: profilesEditor.DeleteProfile(gamepadProfiles[selectedProfileIndex]); ReloadProfilesAndRepaint(); break;
			}
		}

		void CreateControlSchemeContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, 1);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, 2);
			contextMenu.DropDown(position);
		}


		List<HieararchyGUIElement> BuildHierarchyElementsList () {
			List<HieararchyGUIElement> r = new List<HieararchyGUIElement>();
			for (int i = 0; i < gamepadProfiles.Length; i++) {
				r.Add(new HieararchyGUIElement(gamepadProfiles[i].name, null, CreateControlSchemeContextMenu));
			}
			return r;
		}

		
		public void OnGUI()
		{
			if (gamepadProfiles == null) gamepadProfiles = GamepadHandler.LoadAllGamepadProfiles();
	
			if (gamepadProfiles.Length <= 0 || (hasSelection && selectedProfileIndex >= gamepadProfiles.Length)) InputManagerWindow.ResetSelections();

			float testButtonHeight = 30; 
			if (Application.isPlaying) {
				testButtonHeight+= 45;
				if (testSceneEditor.lastScene != null) {
					testButtonHeight += 44;
				}
			}

			if (HierarchyGUI.Draw (InputManagerWindow.tabsOffYOffset + testButtonHeight, false, BuildHierarchyElementsList(), DrawSelected, CreateEditMenu)) {	
				OnNewProfileSelection(gamepadProfiles[selectedProfileIndex]);
			}

			Rect testRect = new Rect(0.0f, 0.0f, InputManagerWindow.width, testButtonHeight);
			testRect.y += InputManagerWindow.tabsOffYOffset;
			
			GUILayout.BeginArea(testRect);

			for (int i = 0; i < 1; i++) EditorGUILayout.Space();
            
			if (Application.isPlaying) {
				if (testSceneEditor.lastScene != null) {
                	EditorGUILayout.HelpBox("Do not close the Input Manager window while in the gamepad testing scene.\n\nOr you will not be taken back to the original scene you were working on...", MessageType.Warning);
				}
                EditorGUILayout.HelpBox("[Play Mode]: Any new Profiles will be active the next time you enter play mode.", MessageType.Info);
            }
			else {
            	if (GUILayout.Button("Start Gamepad Inputs Testing Scene")) testSceneEditor.StartTestScene();
			}
            
			GUILayout.EndArea();			
		}

        public void OnPlayStateChanged (PlayModeStateChange state) {
			testSceneEditor.OnPlayStateChanged(state);
        }
        
		void DrawSelected(Rect position)
		{
			if (!hasSelection)
				return;

			position.x += 5;
			position.y += 5;
			position.width -= 10;

			GUILayout.BeginArea(position);
			InputManagerWindow.m_mainPanelScrollPos = EditorGUILayout.BeginScrollView( InputManagerWindow.m_mainPanelScrollPos);
			

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            string oldName = gamepadProfiles[selectedProfileIndex].name;
			string newName = EditorGUILayout.DelayedTextField("", oldName);
			EditorGUILayout.EndHorizontal();

            if (newName != oldName) profilesEditor.RenameProfile(gamepadProfiles[selectedProfileIndex], newName);
            
			profilesEditor.DrawProfile(profileSO);

			for (int i = 0; i < 3; i++) EditorGUILayout.Space();

			EditorGUILayout.EndScrollView();
			
			GUILayout.EndArea();
		}

		public void OnNewProfileSelection(GenericGamepadProfile profile) {
            profileSO = new SerializedObject(profile);
        }
	}
}

