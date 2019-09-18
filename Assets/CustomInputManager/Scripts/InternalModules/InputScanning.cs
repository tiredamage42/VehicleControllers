
using UnityEngine;

namespace CustomInputManager.Internal
{
	public enum ScanFlags { 
		None = 0, 
		Key = 1 << 1, 
		JoystickButton = 1 << 2, 
		JoystickAxis = 1 << 3, 
		MouseAxis = 1 << 4 
	}
    
    public struct ScanResult {
		public ScanFlags ScanFlags;
		public KeyCode keyCode;
		public int mouseAxis;
		public GamepadButton gamepadButton;
		public GamepadAxis gamepadAxis;
		public float axisValue;

		public static ScanResult Empty (ScanFlags scanResult = ScanFlags.None){
			ScanResult result = new ScanResult();
			result.ScanFlags = scanResult;
			result.keyCode = KeyCode.None;
			result.mouseAxis = -1;
			result.gamepadButton = GamepadButton.None;
			result.gamepadAxis = GamepadAxis.None;
			result.axisValue = 0.0f;
			return result;
		}

		public static ScanResult KeyScanResult(KeyCode keyCode) {
			ScanResult result = Empty(ScanFlags.Key);
			result.keyCode = keyCode;
			return result;
		}
		public static ScanResult MouseScanResult(int mouseAxis) {
			ScanResult result = Empty(ScanFlags.MouseAxis);
			result.mouseAxis = mouseAxis;
			return result;
		}
		public static ScanResult GamepadButtonResult(GamepadButton gamepadButton){
			ScanResult result = Empty(ScanFlags.JoystickButton);
			result.gamepadButton = gamepadButton;
			return result;
		}
		public static ScanResult GamepadAxisResult(GamepadAxis gamepadAxis, float axisValue){
			ScanResult result = Empty(ScanFlags.JoystickAxis);
			result.gamepadAxis = gamepadAxis;
			result.axisValue = axisValue;
			return result;
		}
	}

	/// <summary>
	/// Encapsulates a method that takes one parameter(the scan result) and returns 'true' if
	/// the scan result is accepted or 'false' if it isn't.
	/// </summary>
	public delegate bool ScanHandler(ScanResult result);

	public static class ScanService
	{
		static ScanHandler scanHandler;
		static ScanFlags scanFlags;
		static System.Action onScanEnd;
		static float m_scanStartTime;
		static KeyCode[] m_keys;
		static string[] m_rawMouseAxes;
		public static bool isScanning { get; private set; }
		

		public static void OnAwake () {
			m_rawMouseAxes = new string[InputBinding.MAX_MOUSE_AXES];
			for(int i = 0; i < m_rawMouseAxes.Length; i++)
				m_rawMouseAxes[i] = string.Concat("mouse_axis_", i);
			
			m_keys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
			isScanning = false;
		}

		public static void Start(ScanFlags scanFlags, ScanHandler scanHandler, System.Action onScanEnd)
		{
			if(isScanning)
				Stop();

			m_scanStartTime = Time.unscaledTime;
			isScanning = true;
			ScanService.scanFlags = scanFlags;
			ScanService.onScanEnd = onScanEnd;
			ScanService.scanHandler = scanHandler;

		}

		public static void Stop()
		{
			if(isScanning)
			{
				isScanning = false;
				scanHandler(ScanResult.Empty());
				EndScan();
			}
		}

		static bool EndScan() {
			scanHandler = null;

			if (onScanEnd != null)
				onScanEnd();
			
			onScanEnd = null;
			return true;
		}

		public static void Update(float gameTime, KeyCode cancelScanKey, float maxScanTime, int numJoysticks)
		{
			if(!isScanning) {
				return;
			}
			float timeout = gameTime - m_scanStartTime;

			// or on cancel
			if(Input.GetKeyDown(cancelScanKey) || timeout >= maxScanTime) {
				Stop();
				return;
			}

			bool success = false;
			if(!success && HasFlag(ScanFlags.Key))
				success = ScanKey();

			if(!success && HasFlag(ScanFlags.JoystickButton))
				success = ScanJoystickButton(numJoysticks);

			if(!success && HasFlag(ScanFlags.JoystickAxis))
				success = ScanJoystickAxis(numJoysticks);

			if(!success && HasFlag(ScanFlags.MouseAxis))
				success = ScanMouseAxis();
	
			isScanning = !success;
		}

		static bool ScanKey()
		{
			int max = (int)KeyCode.JoystickButton0;

			for(int i = 0; i < m_keys.Length; i++) {
				KeyCode k = m_keys[i];
				
				if((int)k >= max)
					break;

				if(Input.GetKeyDown(k)) {
					if(scanHandler(ScanResult.KeyScanResult(k))) {
						return EndScan();
					}
				}
			}

			return false;
		}

		
		

		static bool ScanJoystickButton(int numJoysticks)
		{
			int gamepadButtons = 14;
			for(int i = 0; i < gamepadButtons; i++) {
				GamepadButton button = (GamepadButton)i;
				// for (int x = 0; x < InputBinding.MAX_JOYSTICKS; x++) {
				for (int x = 0; x < numJoysticks; x++) {
				
					if(GamepadHandler.GetButtonDown(button, x)) {
						if(scanHandler(ScanResult.GamepadButtonResult(button))) {
							return EndScan();
						}
					}
				}
			}
			return false;
		}

		static bool ScanJoystickAxis(int numJoysticks)
		{
			int axes = 8;
			for(int i = 0; i < axes; i++) {
				GamepadAxis axis = (GamepadAxis)i;
				
				// for (int x = 0; x < InputBinding.MAX_JOYSTICKS; x++) {
				for (int x = 0; x < numJoysticks; x++) {
					
					float axisRaw = GamepadHandler.GetAxisRaw(axis, x, .2f);

					if(Mathf.Abs(axisRaw) >= 1.0f) {
						if(scanHandler(ScanResult.GamepadAxisResult(axis, axisRaw))) {
							return EndScan();
						}
					}
				}
			}
			return false;
		}

		static bool ScanMouseAxis() {
			for(int i = 0; i < m_rawMouseAxes.Length; i++) {
				if(Mathf.Abs(Input.GetAxis(m_rawMouseAxes[i])) > 0.0f) {
					if(scanHandler(ScanResult.MouseScanResult(i))) {
						return EndScan();
					}
				}
			}
			return false;
		}

		static bool HasFlag(ScanFlags flag) {
			return ((int)scanFlags & (int)flag) != 0;
		}
	}
}
