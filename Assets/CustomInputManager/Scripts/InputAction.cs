
using UnityEngine;
using System.Collections.Generic;

namespace CustomInputManager
{
	[System.Serializable] public class InputAction
	{
		public const int MAX_BINDINGS = 16;
		[SerializeField] private string m_name;
		[SerializeField] private List<InputBinding> m_bindings;
		[SerializeField] private string m_displayName;
		public List<InputBinding> bindings { get { return m_bindings; } }
		int bindingsCount;

		public string displayName {
			get { return string.IsNullOrEmpty( m_displayName ) ? Name : m_displayName; }
			set { m_displayName = value; }
		}

		public string Name
		{
			get { return m_name; }
			set {
				if(Application.isPlaying)
					Debug.LogWarning("You should not change the name of an input action at runtime");
				else 
					m_name = value;
			}
		}

		public bool AnyInput(int playerID) {
			for (int i = 0; i < bindingsCount; i++) {	
				if(m_bindings[i].AnyInput(playerID)) return true;
			}
			return false;
		}
		
		public InputAction(string name) : this(name, name) { }
		
		public InputAction(string name, string displayName) {
			m_name = name;
			m_displayName = displayName;
			m_bindings = new List<InputBinding>();
		}
		
		public void Initialize(int maxJoysticks) {
			bindingsCount = bindings.Count;
			for (int i = 0; i < bindingsCount; i++) m_bindings[i].Initialize(maxJoysticks);
		}
		
		public void Update(float deltaTime) {
			for (int i = 0; i < bindingsCount; i++) m_bindings[i].Update(deltaTime);
		}
		
		public float GetAxis(int playerID) {
			float value = InputBinding.AXIS_NEUTRAL;
			for (int i = 0; i < bindingsCount; i++) value += m_bindings[i].GetAxis(playerID);				
			return Mathf.Clamp(value, -1, 1);
		}

		public float GetAxisRaw(int playerID) {
			float value = InputBinding.AXIS_NEUTRAL;
			for (int i = 0; i < bindingsCount; i++) value += m_bindings[i].GetAxisRaw(playerID);
			return Mathf.Clamp(value, -1, 1);
		}
		
		public bool GetButton(int playerID) {
			for (int i = 0; i < bindingsCount; i++) {	
				if (m_bindings[i].GetButton(playerID)) return true;
			}
			return false;
		}
		
		public bool GetButtonDown(int playerID) {
			for (int i = 0; i < bindingsCount; i++) {	
				if (m_bindings[i].GetButtonDown(playerID)) return true;
			}
			return false;
		}
		
		public bool GetButtonUp(int playerID) {
			for (int i = 0; i < bindingsCount; i++) {	
				if (m_bindings[i].GetButtonUp(playerID)) return true;
			}
			return false;
		}

		
		public InputBinding GetBinding(int index) {
			if(index >= 0 && index < bindingsCount)
				return m_bindings[index];

			return null;
		}

		public InputBinding CreateNewBinding() {
			if(m_bindings.Count < MAX_BINDINGS) {
				InputBinding binding = new InputBinding();
				m_bindings.Add(binding);
				return binding;
			}

			return null;
		}
	}
}