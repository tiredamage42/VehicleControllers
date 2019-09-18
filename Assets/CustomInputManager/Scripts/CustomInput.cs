using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using CustomInputManager.Internal;
namespace CustomInputManager
{
	public partial class CustomInput : MonoBehaviour
	{

		#region SCENE_INSTANCE
		static void InitializeInputManagerInstance () {
			_instance = new GameObject("InputManagerInstance");
			inst = _instance.AddComponent<CustomInput>();
			DontDestroyOnLoad(_instance);

			// try and load custom runtime bindings
			if (!LoadCustomSchemes()) {
				Debug.Log("Couldnt find custom control bindings, loading defaults...");
				LoadDefaultSchemes();
			}
		}

		static void init () { 
			if (_instance == null) {
				InitializeInputManagerInstance(); 
			}
		}

		static GameObject _instance;
		static CustomInput inst;
		void Awake()
		{
			m_schemeLookup = new Dictionary<string, ControlScheme>();
			playerSchemes = new ControlScheme[numPlayers];
			ScanService.OnAwake();
			GamepadHandler.OnAwake();
		}
		void Start () {
			GamepadHandler.OnStart();
		}

		void Update()
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			GamepadHandler.OnUpdate(unscaledDeltaTime);
			for (int i = 0; i < m_controlSchemes.Count; i++) {
				m_controlSchemes[i].Update(unscaledDeltaTime);
			}
			ScanService.Update(Time.unscaledTime, KeyCode.Escape, 5.0f, numPlayers);
		}

		public static void RunCoroutine (IEnumerator coroutine) {
			init(); inst.StartCoroutine(coroutine);
		}
		
		#endregion




		static List<ControlScheme> m_controlSchemes = new List<ControlScheme>();
		static ControlScheme[] playerSchemes;
		static Dictionary<string, ControlScheme> m_schemeLookup;
		public static List<ControlScheme> ControlSchemes { get { init(); return m_controlSchemes; } }
		public static int numPlayers { get { init(); return InputManager.maxPlayers; } }
		
		static void Initialize()
		{
			m_schemeLookup.Clear();

			// playerSchemes = new ControlScheme[numPlayers];
			for (int i = 0; i < numPlayers; i++) 
				playerSchemes[i] = null;
		
			if(m_controlSchemes.Count == 0) {
				Debug.LogWarning("No Control Schemes Loaded...");
				return;
			}

			for (int i = 0; i < numPlayers; i++) {
				playerSchemes[i] = m_controlSchemes[0];
			}

			m_schemeLookup.Clear();
			foreach(ControlScheme scheme in m_controlSchemes) {
				m_schemeLookup[scheme.Name] = scheme;
				scheme.Initialize(numPlayers);
			}
			
			Input.ResetInputAxes();
		}
		
		public static ControlScheme GetControlScheme(int playerID) {
			init(); return playerSchemes[playerID];
		}

		public static void SetControlScheme(string name, int playerID)
		{
			init();
			// int? playerWhoUsesControlScheme = instance.IsControlSchemeInUse(name);

			// this assumes only one player per control scheme, which only works for strictly
			// keyboard only games...

			// if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value != playerID) {
			// 	Debug.LogErrorFormat("The control scheme named \'{0}\' is already being used by player {1}", name, playerWhoUsesControlScheme.Value.ToString());
			// 	return;
			// }

			if (playerSchemes[playerID] != null && playerSchemes[playerID].Name == name) {
				Debug.LogWarning("player " + playerID + " is already using scheme: " + name);
				return;
			}
			
			ControlScheme controlScheme = null;
			if(m_schemeLookup.TryGetValue(name, out controlScheme))
				playerSchemes[playerID] = controlScheme;
			
			else Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", name));
			
		}

		public static ControlScheme GetControlScheme(string name)
		{
			init();
			ControlScheme scheme;
			if(m_schemeLookup.TryGetValue(name, out scheme)) {
				return scheme;
			}
			Debug.LogError("Scheme " + name + " does not exist");
			return null;
		}

		static InputAction GetAction(int playerID, int actionKey)
		{
			if(playerSchemes[playerID] == null) return null;
			return playerSchemes[playerID].GetAction(actionKey);
		}

		public static int Name2Key (string name) {
			init();
			bool nameValid = false;
			for (int i = 0; i < m_controlSchemes.Count; i++) {
				if (m_controlSchemes[i].HasActionName(name)) {
					nameValid = true;
					break;
				}
			}
			if (!nameValid) {
				Debug.LogError(string.Format("An action named \'{0}\' does not exist in the active input configuration", name));
				return -1;
			}
			return InputManager.EncodeActionName(name);
		}
		public static int Name2Key (string name, int playerID) {
			init();
			if (!playerSchemes[playerID].HasActionName(name)) {
				Debug.LogError(string.Format("An action named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
				return -1;
			}
			return InputManager.EncodeActionName(name);
		}

		static void SetSchemes (List<ControlScheme> schemes) {
			m_controlSchemes = schemes;
			Initialize();
		}

		static bool LoadCustomSchemes()
		{
			List<ControlScheme> customSchemes = CustomProjectInputs.LoadCustomControls();
			if (customSchemes == null)
				return false;
			SetSchemes(customSchemes);
			return true;
		}

		static void LoadDefaultSchemes () { SetSchemes(DefaultProjectInputs.LoadDefaultSchemes()); }
		public static bool RevertSchemeToDefaults ( string controlSchemeName, int bindingIndex ) {
			init();
			ControlScheme defaultScheme = DefaultProjectInputs.LoadDefaultScheme(controlSchemeName);
			if(defaultScheme != null) {
				ControlScheme curScheme = GetControlScheme(controlSchemeName);
				int c = defaultScheme.Actions.Count;
				if(c == curScheme.Actions.Count)
				{
					for(int i = 0; i < c; i++) curScheme.Actions[i].GetBinding(bindingIndex).Copy(defaultScheme.Actions[i].GetBinding(bindingIndex));
					
					Initialize();
					// save as override...
					SaveCustomControls();
                    return true;
				}
				else Debug.LogError("Current and default control scheme don't have the same number of actions");
			}
			else Debug.LogErrorFormat("Default input profile doesn't contain a control scheme named '{0}'", controlSchemeName);
			
            return false;
		}


		public static void SaveCustomControls () {
			init(); CustomProjectInputs.SaveCustomControls(m_controlSchemes);
		}
		public static bool ScanInputForControlRebind (InputBinding inputBinding, bool changingPositiveDigitalAxis, Action onStopScan) {
			init(); return CustomProjectInputs.ScanInputForControlRebind(inputBinding, changingPositiveDigitalAxis, onStopScan);
		}


		public static bool AnyInput(string schemeName, int playerID)
		{
			init();
			ControlScheme scheme;
			if(m_schemeLookup.TryGetValue(schemeName, out scheme)) {
				return scheme.AnyInput(playerID);
			}
			return false;
		}
		public static bool AnyInput(int playerID) { init(); return playerSchemes[playerID].AnyInput(playerID); }

	
		public static float GetAxis(int key, int playerID=0) { init(); return AxisQuery(key, playerID, NormalAxisQuery); }
		public static float GetAxisRaw(int key, int playerID=0) { init(); return AxisQuery(key, playerID, RawAxisQuery); }
		public static bool GetButton(int key, int playerID=0) { init(); return ButtonQuery(key, playerID, ButtonHeldQuery); }
		public static bool GetButtonDown(int key, int playerID=0) { init(); return ButtonQuery(key, playerID, ButtonDownQuery); }
		public static bool GetButtonUp(int key, int playerID=0) { init(); return ButtonQuery(key, playerID, ButtonUpQuery); }
		public static float GetAxis(string name, int playerID=0) { init(); return GetAxis(Name2Key(name, playerID), playerID); }
		public static float GetAxisRaw(string name, int playerID=0) { init(); return GetAxisRaw(Name2Key(name, playerID), playerID); }
		public static bool GetButton(string name, int playerID=0) { init(); return GetButton(Name2Key(name, playerID), playerID); }
		public static bool GetButtonDown(string name, int playerID=0) { init(); return GetButtonDown(Name2Key(name, playerID), playerID); }
		public static bool GetButtonUp(string name, int playerID=0) { init(); return GetButtonUp(Name2Key(name, playerID), playerID); }
		public static bool GetKey(KeyCode key) { init(); return Input.GetKey(key); }
		public static bool GetKeyDown(KeyCode key) { init(); return Input.GetKeyDown(key); }
		public static bool GetKeyUp(KeyCode key) { init(); return Input.GetKeyUp(key); }
		public static bool GetMouseButton(int index) { init(); return Input.GetMouseButton(index); }
		public static bool GetMouseButtonDown(int index) { init(); return Input.GetMouseButtonDown(index); }
		public static bool GetMouseButtonUp(int index) { init(); return Input.GetMouseButtonUp(index); }
		public static string[] GetJoystickNames() { init(); return Input.GetJoystickNames(); }
		public static void ResetInputAxes() { init(); Input.ResetInputAxes(); }

		static float RawAxisQuery (InputAction action, int playerID) { return action.GetAxisRaw(playerID); }
		static float NormalAxisQuery (InputAction action, int playerID) { return action.GetAxis(playerID); }
		static bool ButtonHeldQuery (InputAction action, int playerID) { return action.GetButton(playerID); }
		static bool ButtonDownQuery (InputAction action, int playerID) { return action.GetButtonDown(playerID); }
		static bool ButtonUpQuery (InputAction action, int playerID) { return action.GetButtonUp(playerID); }



		static bool ButtonQuery (int key, int playerID, System.Func<InputAction, int, bool> buttonQuery) {
			InputAction action = GetAction(playerID, key);
			if(action != null) return buttonQuery(action, playerID);
			Debug.LogError(string.Format("An button key \'{0}\' does not exist in the active input configuration for player {1}", key, playerID));
			// Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
			return false;
		}

		static float AxisQuery (int key, int playerID, System.Func<InputAction, int, float> axisQuery) {
			InputAction action = GetAction(playerID, key);
			if(action != null) return axisQuery(action, playerID);
			Debug.LogError(string.Format("An axis key \'{0}\' does not exist in the active input configuration for player {1}", key, playerID));
			// Debug.LogError(string.Format("An axis named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
			return 0.0f;
		}



		public static bool anyKey { get { init(); return Input.anyKey; } }
		public static bool anyKeyDown { get { init(); return Input.anyKeyDown; } }
		public static Vector2 mousePosition { get { init(); return Input.mousePosition; } }
		public static bool mousePresent { get { init(); return Input.mousePresent; } }
		public static Vector2 mouseScrollDelta { get { init(); return Input.mouseScrollDelta; } }




		public static Touch GetTouch(int index) { init(); return Input.GetTouch(index); }
		public static Vector3 acceleration { get { init(); return Input.acceleration; } }
		public static int accelerationEventCount { get { init(); return Input.accelerationEventCount; } }
		public static AccelerationEvent[] accelerationEvents { get { init(); return Input.accelerationEvents; } }
		public static Compass compass { get { init(); return Input.compass; } }
		public static string compositionString { get { init(); return Input.compositionString; } }
		public static DeviceOrientation deviceOrientation { get { init(); return Input.deviceOrientation; } }
		public static Gyroscope gyro { get { init(); return Input.gyro; } }
		public static bool imeIsSelected { get { init(); return Input.imeIsSelected; } }
		public static string inputString { get { init(); return Input.inputString; } }
		public static LocationService location { get { init(); return Input.location; } }
		public static bool touchSupported { get { init(); return Input.touchSupported; } }
		public static int touchCount { get { init(); return Input.touchCount; } }
		public static Touch[] touches { get { init(); return Input.touches; } }
		
		public static bool compensateSensors
		{
			get { init(); return Input.compensateSensors; }
			set { init(); Input.compensateSensors = value; }
		}
		
		public static Vector2 compositionCursorPos
		{
			get { init(); return Input.compositionCursorPos; }
			set { init(); Input.compositionCursorPos = value; }
		}
		
		public static IMECompositionMode imeCompositionMode
		{
			get { init(); return Input.imeCompositionMode; }
			set { init(); Input.imeCompositionMode = value; }
		}
		
		public static bool multiTouchEnabled
		{
			get { init(); return Input.multiTouchEnabled; }
			set { init(); Input.multiTouchEnabled = value; }
		}
		
		public static AccelerationEvent GetAccelerationEvent(int index) { init(); return Input.GetAccelerationEvent(index); }

	}
}
