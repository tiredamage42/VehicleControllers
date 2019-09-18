
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomInputManager.Internal;

namespace CustomInputManager.Editor {
		
	public class ControlSchemesTab 
	{

	
		public enum MoveDirection { Up, Down }
		public enum CollectionAction { None, Remove, Add, MoveUp, MoveDown }
		public enum KeyType { Positive = 0, Negative }
		
		#region [Fields]
		public const float INPUT_ACTION_SPACING = 20.0f;
		public const float INPUT_BINDING_SPACING = 10.0f;
		public const float JOYSTICK_WARNING_SPACING = 10.0f;
		public const float JOYSTICK_WARNING_HEIGHT = 40.0f;
		public const float INPUT_FIELD_HEIGHT = 16.0f;
		public const float FIELD_SPACING = 2.0f;
		public const float BUTTON_HEIGHT = 24.0f;
		public const float SCROLL_BAR_WIDTH = 15.0f;
		public const float MIN_MAIN_PANEL_WIDTH = 300.0f;
		
		List<ControlScheme> loadedElements;

		void InitializeLoadedElements () {
			if (!Application.isPlaying) loadedElements = DefaultProjectInputs.LoadDefaultSchemes();
		}

		List<ControlScheme> schemes {
			get {
				if (Application.isPlaying) 
					return CustomInput.ControlSchemes;
				if (loadedElements == null) 
					InitializeLoadedElements();
				
				return loadedElements;
			}
		}

		bool selectionEmpty { get { return !IsActionSelected && !IsControlSchemeSelected; } }
		private GUIContent m_gravityInfo;
		private GUIContent m_sensitivityInfo;
		private GUIContent m_snapInfo;
		private GUIContent m_deadZoneInfo;
		private GUIContent m_plusButtonContent;
		private GUIContent m_minusButtonContent;
		private GUIContent m_upButtonContent;
		private GUIContent m_downButtonContent;
		private InputAction m_copySource;
		private KeyCodeField[] m_keyFields;
		private GUIStyle m_whiteFoldout;
		private GUIStyle m_warningLabel;
		#endregion

		#region [Startup]

		public static Texture2D GetUnityIcon(string name)
		{
			return EditorGUIUtility.Load(name + ".png") as Texture2D;
		}
		public static Texture2D GetCustomIcon(string name)
		{
			return Resources.Load<Texture2D>(InputManager.resourcesFolder + name) as Texture2D;
		
		}
		

		public void OnEnable()
		{
			m_gravityInfo = new GUIContent("Gravity When Axis Query", "The speed(in units/sec) at which a digital axis falls towards neutral.");
			m_sensitivityInfo = new GUIContent("Sensitivity When Axis Query", "The speed(in units/sec) at which an axis moves towards the target value.");
			m_snapInfo = new GUIContent("Snap When Axis Query", "If input switches direction, do we snap to neutral and continue from there?");// For digital axes only.");
			m_deadZoneInfo = new GUIContent("Dead Zone", "Size of analog dead zone. Values within this range map to neutral.");
			m_plusButtonContent = new GUIContent (GetUnityIcon("ol plus"));
			m_minusButtonContent = new GUIContent(GetUnityIcon("ol minus"));
			m_upButtonContent = new GUIContent   (GetCustomIcon("input_editor_arrow_up"));
			m_downButtonContent = new GUIContent (GetCustomIcon("input_editor_arrow_down"));

			CreateKeyFields();

			InitializeLoadedElements();
	
		}

		void SaveDefaultProjectInputsXML() {
			DefaultProjectInputs.SaveSchemesAsDefault("Saving", schemes);
			guiChanged = false;
		}
		

		void DisplaySaveDialogue () {
			if (!guiChanged)
				return;

			if (EditorUtility.DisplayDialog("Input Manager", "Would you like to save changes made to the input schemes?", "Yes", "No")) {
			
				SaveDefaultProjectInputsXML();
			}
		}

		public void Dispose(bool repeat)
		{
			if (!repeat) {
				DisplaySaveDialogue();
				m_copySource = null;
			}
		}
			
		#endregion

		#region [Menus]
		


		void CreateFileMenu(Rect position)
		{
			GenericMenu fileMenu = new GenericMenu();
			fileMenu.AddItem(new GUIContent("Overwrite Project Settings"), false, HandleFileMenuOption, 0);// FileMenuOptions.OverwriteProjectSettings);
			
			fileMenu.AddSeparator("");
			fileMenu.AddItem(new GUIContent("Save Project Inputs"), false, HandleFileMenuOption, 1);//FileMenuOptions.SaveProjectInputs);
			
			fileMenu.AddSeparator("");
			

			fileMenu.AddItem(new GUIContent("New Control Scheme"), false, HandleFileMenuOption, 2);//EditMenuOptions.NewControlScheme);
			if(IsControlSchemeSelected)
				fileMenu.AddItem(new GUIContent("New Action"), false, HandleFileMenuOption, 3);// EditMenuOptions.NewInputAction);
			else
				fileMenu.AddDisabledItem(new GUIContent("New Action"));
			fileMenu.AddSeparator("");

			if (!selectionEmpty)
				fileMenu.AddItem(new GUIContent("Duplicate"), false, HandleFileMenuOption, 4);// EditMenuOptions.Duplicate);
			else
				fileMenu.AddDisabledItem(new GUIContent("Duplicate"));

			if (!selectionEmpty)
				fileMenu.AddItem(new GUIContent("Delete"), false, HandleFileMenuOption, 5);//EditMenuOptions.Delete);
			else
				fileMenu.AddDisabledItem(new GUIContent("Delete"));

			if(IsActionSelected)
				fileMenu.AddItem(new GUIContent("Copy"), false, HandleFileMenuOption, 6);//EditMenuOptions.Copy);
			else
				fileMenu.AddDisabledItem(new GUIContent("Copy"));

			if(m_copySource != null && IsActionSelected)
				fileMenu.AddItem(new GUIContent("Paste"), false, HandleFileMenuOption, 7);//EditMenuOptions.Paste);
			else
				fileMenu.AddDisabledItem(new GUIContent("Paste"));

			fileMenu.DropDown(position);
		}

		
		void HandleFileMenuOption(object arg)
		{
			int option = (int)arg;
			
			switch(option)
			{
				case 0: ConvertUnityInputManager.OverwriteProjectSettings(); break;
				case 1: SaveDefaultProjectInputsXML(); break;
				case 2: CreateNewControlScheme(); break;
				case 3: CreateNewInputAction(); break; case 4: Duplicate(); break;
				case 5: Delete(); break;
				case 6: CopyInputAction(); break;
				case 7: PasteInputAction(); break;
			}
		}

		 void CreateControlSchemeContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("New Action"), false, HandleControlSchemeContextMenuOption, 0);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleControlSchemeContextMenuOption, 1);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleControlSchemeContextMenuOption, 2);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleControlSchemeContextMenuOption, 3);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleControlSchemeContextMenuOption, 4);

			contextMenu.DropDown(position);
		}

		void HandleControlSchemeContextMenuOption(object arg)
		{
			int option = (int)arg;
			switch(option)
			{
				case 0: CreateNewInputAction(); break;
				case 1: Duplicate(); break;
				case 2: Delete(); break;
				case 3: ReorderControlScheme(MoveDirection.Up); break;
				case 4: ReorderControlScheme(MoveDirection.Down); break;
			}
		}

		void CreateInputActionContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleInputActionContextMenuOption, 0);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleInputActionContextMenuOption, 1);
			contextMenu.AddItem(new GUIContent("Copy"), false, HandleInputActionContextMenuOption, 2);
			contextMenu.AddItem(new GUIContent("Paste"), false, HandleInputActionContextMenuOption, 3);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleInputActionContextMenuOption, 4);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleInputActionContextMenuOption, 5);

			contextMenu.DropDown(position);
		}

		 void HandleInputActionContextMenuOption(object arg)
		{
			int option = (int)arg;
			switch(option)
			{
				case 0: Duplicate(); break;
				case 1: Delete(); break;
				case 2: CopyInputAction(); break;
				case 3: PasteInputAction(); break;
				case 4: ReorderInputAction(MoveDirection.Up); break;
				case 5: ReorderInputAction(MoveDirection.Down); break;
			}
		}

		 void CreateNewControlScheme()
		{
			schemes.Add(new ControlScheme());

			InputManagerWindow.ResetSelections();
			selectedControlSchemeIndex = schemes.Count - 1;
			InputManagerWindow.instance.Repaint();
		}

		bool IsControlSchemeSelected { get { return selectedControlSchemeIndex >= 0; } }
		bool IsActionSelected { get { return selectedActionIndex >= 0; } }
		
		int selectedControlSchemeIndex { 
			get { return InputManagerWindow.GetSelection(0); } 
			set { InputManagerWindow.SetSelection(0, value); } 
		}
		int selectedActionIndex { 
			get { return InputManagerWindow.GetSelection(1); } 
			set { InputManagerWindow.SetSelection(1, value); } 
		}


		 void CreateNewInputAction()
		{
			if(IsControlSchemeSelected)
			{
				ControlScheme scheme = schemes[selectedControlSchemeIndex];
				scheme.CreateNewAction("New Action", "New Action Display Name");
				selectedActionIndex = scheme.Actions.Count - 1;
				ResetKeyFields();
				InputManagerWindow.instance.Repaint();
			}
		}




		void Duplicate()
		{
			if(IsActionSelected)
				DuplicateInputAction();
			else if(IsControlSchemeSelected)
				DuplicateControlScheme();
			
		}
		InputAction DuplicateInputAction(InputAction source)
		{
			return DuplicateInputAction(source.Name, source);
		}


		InputAction DuplicateInputAction(string name, InputAction source)
		{
			InputAction a = new InputAction("_");
			CopyInputAction(a, source);
			a.Name = name;
			return a;
		}
		void DuplicateInputAction()
		{
			ControlScheme scheme = schemes[selectedControlSchemeIndex];
			InputAction source = scheme.Actions[selectedActionIndex];


			InputAction action = DuplicateInputAction(source.Name + " Copy", source);
			scheme.Actions.Insert(selectedActionIndex + 1, action);
			
			selectedActionIndex++;

			InputManagerWindow.instance.Repaint();
		}

		void DuplicateControlScheme()
		{
			ControlScheme source = schemes[selectedControlSchemeIndex];

			ControlScheme duplicate = new ControlScheme();
			duplicate.Name = source.Name + " Copy";
			duplicate.Actions = new List<InputAction>();
			foreach(var action in source.Actions)
				duplicate.Actions.Add(DuplicateInputAction(action));
			
			schemes.Insert(selectedControlSchemeIndex + 1, duplicate);
			selectedControlSchemeIndex++;

			InputManagerWindow.instance.Repaint();
		}

		void Delete()
		{
			if(IsActionSelected)
			{
				ControlScheme scheme = schemes[selectedControlSchemeIndex];
				
				if(selectedActionIndex >= 0 && selectedActionIndex < scheme.Actions.Count)
					scheme.Actions.RemoveAt(selectedActionIndex);
				
				
			}
			else if(IsControlSchemeSelected)
			{
				schemes.RemoveAt(selectedControlSchemeIndex);
			}

			InputManagerWindow.ResetSelections();
			InputManagerWindow.instance.Repaint();
		}

		void CopyInputAction()
		{
			m_copySource = DuplicateInputAction(schemes[selectedControlSchemeIndex].Actions[selectedActionIndex]);
		}
		void PasteInputAction()
		{
			CopyInputAction(schemes[selectedControlSchemeIndex].Actions[selectedActionIndex], m_copySource);
		}
			

		 void ReorderControlScheme(MoveDirection dir)
		{
			if(IsControlSchemeSelected)
			{
				var index = selectedControlSchemeIndex;

				if(dir == MoveDirection.Up && index > 0)
				{
					var temp = schemes[index];
					schemes[index] = schemes[index - 1];
					schemes[index - 1] = temp;
					InputManagerWindow.ResetSelections();
					selectedControlSchemeIndex = index - 1;
				}
				else if(dir == MoveDirection.Down && index < schemes.Count - 1)
				{
					var temp = schemes[index];
					schemes[index] = schemes[index + 1];
					schemes[index + 1] = temp;
					InputManagerWindow.ResetSelections();
					selectedControlSchemeIndex = index + 1;
				}
			}
		}

		 void SwapActions(ControlScheme scheme, int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < scheme.Actions.Count && toIndex >= 0 && toIndex < scheme.Actions.Count)
			{
				var temp = scheme.Actions[toIndex];
				scheme.Actions[toIndex] = scheme.Actions[fromIndex];
				scheme.Actions[fromIndex] = temp;
			}
		}

		 void ReorderInputAction(MoveDirection dir)
		{
			if(IsActionSelected)
			{
				var scheme = schemes[selectedControlSchemeIndex];
				var schemeIndex = selectedControlSchemeIndex;
				var actionIndex = selectedActionIndex;

				if(dir == MoveDirection.Up && actionIndex > 0)
				{
					SwapActions(scheme, actionIndex, actionIndex - 1);
					InputManagerWindow.ResetSelections();
					selectedControlSchemeIndex = schemeIndex;
					selectedActionIndex = actionIndex - 1;
				}
				else if(dir == MoveDirection.Down && actionIndex < scheme.Actions.Count - 1)
				{
					SwapActions(scheme, actionIndex, actionIndex + 1);
					InputManagerWindow.ResetSelections();
					selectedControlSchemeIndex = schemeIndex;
					selectedActionIndex = actionIndex + 1;
				}
			}
		}
		#endregion

		public static void CopyInputAction(InputAction a, InputAction source)
		{
			a.Name = source.Name;
			a.displayName = source.displayName;
			a.bindings.Clear();
			foreach(var binding in source.bindings)
			{
				InputBinding duplicate = new InputBinding();
				duplicate.Copy(binding);
				a.bindings.Add(binding);
			}
		}

		

		#region [OnGUI]

		static bool guiChanged;
		public void OnGUI()
		{

			EditorGUI.BeginChangeCheck();
			
			EnsureGUIStyles();

			if ((schemes.Count <= 0) || (IsControlSchemeSelected && selectedControlSchemeIndex >= schemes.Count) || (IsActionSelected && selectedActionIndex >= schemes[selectedControlSchemeIndex].Actions.Count))
				InputManagerWindow.ResetSelections();					

            
			// HierarchyGUI.UpdateHierarchyPanelWidth(InputManagerWindow.tabsOffYOffset);

			// HierarchyGUI.DrawMainPanel(InputManagerWindow.tabsOffYOffset, DrawSelected);

			// if (HierarchyGUI.DrawHierarchyPanel(InputManagerWindow.tabsOffYOffset, true, BuildHierarchyElementsList())) {
			// 	ResetKeyFields();
			// }

			if (HierarchyGUI.Draw (InputManagerWindow.tabsOffYOffset, true, BuildHierarchyElementsList(), DrawSelected, CreateFileMenu)) {
				ResetKeyFields();
			}

			// HierarchyGUI.DrawMainToolbar(CreateFileMenu, InputManagerWindow.tabsOffYOffset);
			

			



			if (EditorGUI.EndChangeCheck()) {
				guiChanged = true;
			}
			
		}

		List<HieararchyGUIElement> BuildSubElements (ControlScheme scheme) {
			List<HieararchyGUIElement> r = new List<HieararchyGUIElement>();
			for (int i = 0; i < scheme.Actions.Count; i++) {
				r.Add(new HieararchyGUIElement(scheme.Actions[i].Name, null, CreateInputActionContextMenu));
			}
			return r;
		}
		List<HieararchyGUIElement> BuildHierarchyElementsList () {
			List<HieararchyGUIElement> r = new List<HieararchyGUIElement>();
			for (int i = 0; i < schemes.Count; i++) {
				r.Add(new HieararchyGUIElement(schemes[i].Name, BuildSubElements(schemes[i]), CreateControlSchemeContextMenu));
			}
			return r;
		}

		void DrawSelected (Rect position) {
			if (IsControlSchemeSelected) {

				if(IsActionSelected)
					DrawInputActionFields(position, schemes[selectedControlSchemeIndex].Actions[selectedActionIndex]);
				else
					DrawControlSchemeFields(position, schemes[selectedControlSchemeIndex]);
			}
		}


		 void DrawControlSchemeFields(Rect position, ControlScheme controlScheme)
		{
			position.x += 5;
			position.y += 5;
			position.width -= 10;

			GUILayout.BeginArea(position);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", GUILayout.Width(50));
			controlScheme.Name = EditorGUILayout.TextField(controlScheme.Name);
			EditorGUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		 void DrawInputActionFields(Rect position, InputAction action)
		{
			bool collectionChanged = false;
			float viewRectHeight = CalculateInputActionViewRectHeight(action);
			viewRectHeight += InputManagerWindow.tabsOffYOffset;
			float itemPosY = 0.0f;
			float contentWidth = position.width - 10.0f;
			Rect viewRect = new Rect(-5.0f, -5.0f, position.width - 10.0f, viewRectHeight - 10.0f);
			
			if(viewRect.width < MIN_MAIN_PANEL_WIDTH)
			{
				viewRect.width = MIN_MAIN_PANEL_WIDTH;
				contentWidth = viewRect.width - 10.0f;
			}

			if(viewRectHeight - 10.0f > position.height)
			{
				viewRect.width -= SCROLL_BAR_WIDTH;
				contentWidth -= SCROLL_BAR_WIDTH;
			}

			InputManagerWindow.m_mainPanelScrollPos = GUI.BeginScrollView(position, InputManagerWindow.m_mainPanelScrollPos, viewRect);
			Rect nameRect = new Rect(0.0f, ValuePP(ref itemPosY, INPUT_FIELD_HEIGHT + FIELD_SPACING), contentWidth, INPUT_FIELD_HEIGHT);
			Rect displayNameRect = new Rect(0.0f, ValuePP(ref itemPosY, INPUT_FIELD_HEIGHT + FIELD_SPACING), contentWidth, INPUT_FIELD_HEIGHT);

			string name = EditorGUI.TextField(nameRect, "Name", action.Name);
			if(name != action.Name) action.Name = name;		//	This prevents the warning at runtime

			action.displayName = EditorGUI.TextField(displayNameRect, "Display Name", action.displayName);

			if(action.bindings.Count > 0)
			{
				itemPosY += INPUT_ACTION_SPACING;

				for(int i = 0; i < action.bindings.Count; i++)
				{
					float bindingRectHeight = CalculateInputBindingViewRectHeight(action.bindings[i]);
					Rect bindingRect = new Rect(-4.0f, ValuePP(ref itemPosY, bindingRectHeight + INPUT_BINDING_SPACING), contentWidth + 8.0f, bindingRectHeight);

					var res = DrawInputBindingFields(bindingRect, "Binding " + (i + 1).ToString("D2"), action, i);
					if(res == CollectionAction.Add)
					{
						InsertNewBinding(action, i+1);
						collectionChanged = true;
					}
					else if(res == CollectionAction.Remove)
					{

						 int index = i--;

						 if(index >= 0 && index < action.bindings.Count) {

							action.bindings.RemoveAt(index);
						 }

						
						collectionChanged = true;
					}
					else if(res == CollectionAction.MoveUp)
					{
						SwapBindings(action, i, i-1);
						collectionChanged = true;
					}
					else if(res == CollectionAction.MoveDown)
					{
						
						SwapBindings(action, i, i+1);
						collectionChanged = true;
					}
				}
			}
			else
			{
				Rect buttonRect = new Rect(contentWidth / 2 - 125.0f, itemPosY + INPUT_ACTION_SPACING, 250.0f, BUTTON_HEIGHT);
				if(GUI.Button(buttonRect, "Add New Binding"))
				{
					action.CreateNewBinding();
					collectionChanged = true;
				}
			}

			GUI.EndScrollView();

			if(collectionChanged)
				InputManagerWindow.instance.Repaint();
			
		}

		InputBinding InsertNewBinding(InputAction action, int index)
		{
			if(action.bindings.Count < InputAction.MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				action.bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		 void SwapBindings(InputAction action, int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < action.bindings.Count && toIndex >= 0 && toIndex < action.bindings.Count)
			{
				var temp = action.bindings[toIndex];
				action.bindings[toIndex] = action.bindings[fromIndex];
				action.bindings[fromIndex] = temp;
			}
		}

		 CollectionAction DrawInputBindingFields(Rect position, string label, InputAction action, int bindingIndex)
		{
			Rect headerRect = new Rect(position.x + 5.0f, position.y, position.width, INPUT_FIELD_HEIGHT);
			Rect removeButtonRect = new Rect(position.width - 25.0f, position.y + 2, 20.0f, 20.0f);
			Rect addButtonRect = new Rect(removeButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect downButtonRect = new Rect(addButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect upButtonRect = new Rect(downButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect layoutArea = new Rect(position.x + 10.0f, position.y + INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f, position.width - 12.5f, position.height - (INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f));
			InputBinding binding = action.bindings[bindingIndex];
			KeyCode positive = binding.Positive, negative = binding.Negative;
			CollectionAction result = CollectionAction.None;

			EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
			
			GUILayout.BeginArea(layoutArea);
			binding.Type = (InputType)EditorGUILayout.EnumPopup("Type", binding.Type);

			if(binding.Type == InputType.KeyButton || binding.Type == InputType.DigitalAxis) {
				DrawKeyCodeField(action, bindingIndex, KeyType.Positive);
			}

			if(binding.Type == InputType.DigitalAxis) {
				DrawKeyCodeField(action, bindingIndex, KeyType.Negative);
			}

			if(binding.Type == InputType.MouseAxis) {
				binding.MouseAxis = EditorGUILayout.Popup("Axis", binding.MouseAxis, InputBinding.mouseAxisNames);// m_axisOptions);
			}

			if(binding.Type == InputType.GamepadButton) {
				binding.GamepadButton = (GamepadButton)EditorGUILayout.EnumPopup("Button", binding.GamepadButton);
			}

			if(binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.GamepadAxis) {
				binding.GamepadAxis = (GamepadAxis)EditorGUILayout.EnumPopup("Axis", binding.GamepadAxis);
			}

			if (
				binding.Type == InputType.DigitalAxis ||
				binding.Type == InputType.KeyButton ||
				binding.Type == InputType.GamepadButton ||
				binding.Type == InputType.GamepadAnalogButton
			) {
				binding.Gravity = EditorGUILayout.FloatField(m_gravityInfo, binding.Gravity);
			}

			if (
				binding.Type == InputType.DigitalAxis ||
				binding.Type == InputType.KeyButton ||
				binding.Type == InputType.GamepadButton ||
				binding.Type == InputType.GamepadAnalogButton ||
				binding.Type == InputType.MouseAxis
			) {
				binding.Sensitivity = EditorGUILayout.FloatField(m_sensitivityInfo, binding.Sensitivity);
			}

			if(binding.Type == InputType.GamepadAxis || binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.MouseAxis) {
				binding.DeadZone = EditorGUILayout.FloatField(m_deadZoneInfo, binding.DeadZone);
			}

			binding.SnapWhenReadAsAxis = EditorGUILayout.Toggle(m_snapInfo, binding.SnapWhenReadAsAxis);

			binding.InvertWhenReadAsAxis = EditorGUILayout.Toggle("Invert When Axis Query", binding.InvertWhenReadAsAxis);

			if( binding.Type == InputType.DigitalAxis || binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.GamepadAxis || binding.Type == InputType.MouseAxis) {
				binding.useNegativeAxisForButton = EditorGUILayout.Toggle("Use Negative Axis For Button Query", binding.useNegativeAxisForButton);
			}

			binding.rebindable = EditorGUILayout.Toggle("Rebindable", binding.rebindable);
			binding.sensitivityEditable = EditorGUILayout.Toggle("Sensitivity Editable", binding.sensitivityEditable);
			binding.invertEditable = EditorGUILayout.Toggle("Invert Editable", binding.invertEditable);


			GUILayout.EndArea();

			if(action.bindings.Count < InputAction.MAX_BINDINGS)
			{
				if(GUI.Button(addButtonRect, m_plusButtonContent, EditorStyles.label))
					result = CollectionAction.Add;
			}
			if(GUI.Button(removeButtonRect, m_minusButtonContent, EditorStyles.label))
				result = CollectionAction.Remove;
			if(GUI.Button(upButtonRect, m_upButtonContent, EditorStyles.label))
				result = CollectionAction.MoveUp;
			if(GUI.Button(downButtonRect, m_downButtonContent, EditorStyles.label))
				result = CollectionAction.MoveDown;
			return result;
		}

		void DrawKeyCodeField(InputAction action, int bindingIndex, KeyType keyType)
		{
			InputBinding binding = action.bindings[bindingIndex];
			int kfIndex = bindingIndex * 2;

			if(keyType == KeyType.Positive)
				binding.Positive = m_keyFields[kfIndex].OnGUI("Positive", binding.Positive);
			else
				binding.Negative = m_keyFields[kfIndex + 1].OnGUI("Negative", binding.Negative);
			
		}
		#endregion

		#region [Utility]
		void CreateKeyFields() {
			m_keyFields = new KeyCodeField[InputAction.MAX_BINDINGS * 2];
			for(int i = 0; i < m_keyFields.Length; i++)
				m_keyFields[i] = new KeyCodeField();
		}

		void ResetKeyFields() {
			for(int i = 0; i < m_keyFields.Length; i++) m_keyFields[i].Reset();	
		}

		void EnsureGUIStyles()
		{
			if(m_whiteFoldout == null) {
				m_whiteFoldout = new GUIStyle(EditorStyles.foldout);
				m_whiteFoldout.normal.textColor = Color.white;
				m_whiteFoldout.onNormal.textColor = Color.white;
				m_whiteFoldout.active.textColor = Color.white;
				m_whiteFoldout.onActive.textColor = Color.white;
				m_whiteFoldout.focused.textColor = Color.white;
				m_whiteFoldout.onFocused.textColor = Color.white;
			}
			if(m_warningLabel == null) {
				m_warningLabel = new GUIStyle(EditorStyles.largeLabel) {
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Bold,
					fontSize = 14
				};
			}
		}
		
		public void OnPlayStateChanged (PlayModeStateChange state) {
			if (state == PlayModeStateChange.ExitingPlayMode)
				DisplaySaveDialogue();	
			if (state == PlayModeStateChange.EnteredEditMode)
				InitializeLoadedElements();
		}


		 float CalculateInputActionViewRectHeight(InputAction action)
		{
			float height = INPUT_FIELD_HEIGHT * 2 + FIELD_SPACING * 2 + INPUT_ACTION_SPACING;
			if(action.bindings.Count > 0)
			{
				foreach(var binding in action.bindings)
					height += CalculateInputBindingViewRectHeight(binding) + INPUT_BINDING_SPACING;

				height += 15.0f;
			}
			else
				height += BUTTON_HEIGHT;
			return height;
		}

		 float CalculateInputBindingViewRectHeight(InputBinding binding)
		{
			int numberOfFields = 12;
			switch(binding.Type)
			{
			case InputType.KeyButton: numberOfFields = 5; break;
			case InputType.MouseAxis: numberOfFields = 6; break;
			case InputType.DigitalAxis: numberOfFields = 7; break;
			case InputType.GamepadButton: numberOfFields = 5; break;
			case InputType.GamepadAnalogButton: numberOfFields = 7; break;
			case InputType.GamepadAxis: numberOfFields = 5; break;
			}

			numberOfFields += 2;    //	Header and type
			numberOfFields += 3; //public bool rebindable, sensitivityEditable, invertEditable;

			float height = INPUT_FIELD_HEIGHT * numberOfFields + FIELD_SPACING * numberOfFields + 10.0f;
			if(binding.Type == InputType.KeyButton && (Event.current == null || Event.current.type != EventType.KeyUp))
			{
				if(IsGenericJoystickButton(binding.Positive))
					height += JOYSTICK_WARNING_SPACING + JOYSTICK_WARNING_HEIGHT;
			}

			return height;
		}

		float ValuePP(ref float height, float amount)
		{
			float value = height;
			height += amount;
			return value;
		}

		bool IsGenericJoystickButton(KeyCode keyCode)
		{
			return (int)keyCode >= (int)KeyCode.JoystickButton0 && (int)keyCode <= (int)KeyCode.JoystickButton19;
		}

		#endregion

		
	}
}
