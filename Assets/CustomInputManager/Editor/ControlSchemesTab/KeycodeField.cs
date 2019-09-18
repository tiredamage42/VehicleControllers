using System;
using UnityEngine;
using UnityEditor;
using UnityInputConverter;

namespace CustomInputManager.Editor {

    public class KeyCodeField
	{
		private string m_controlName, m_keyString;
		private bool m_isEditing;

		public KeyCodeField()
		{
			m_controlName = Guid.NewGuid().ToString("N");
			m_keyString = "";
			m_isEditing = false;
		}

		string Key2String (KeyCode key) {
			return key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key);
		}

		public KeyCode OnGUI(string label, KeyCode key)
		{
			GUI.SetNextControlName(m_controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == m_controlName);
			if(!m_isEditing && hasFocus)
				m_keyString = Key2String(key);
			
			m_isEditing = hasFocus;
			
            if(m_isEditing)
				m_keyString = EditorGUILayout.TextField(label, m_keyString);
			else
				EditorGUILayout.TextField(label, Key2String(key));
			
			if(m_isEditing && Event.current.type == EventType.KeyUp)
			{
				key = KeyCodeConverter.StringToKey(m_keyString);

				m_keyString = Key2String(key);
				m_isEditing = false;
			}

			return key;
		}

		public void Reset()
		{
			m_keyString = "";
			m_isEditing = false;
		}
	}
}
