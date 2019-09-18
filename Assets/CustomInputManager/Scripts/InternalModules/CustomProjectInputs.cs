using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInputManager.Internal {
    /*
        handle user defined custom inputs 
        (for control remapping)
    */
    public class CustomProjectInputs
    {
        static string customControlBindingsPath { get { return Application.persistentDataPath + "/InputManagerOverride.xml"; } }
        

        public static void SaveCustomControls (List<ControlScheme> schemes) {
			new InputSaverXML(customControlBindingsPath).Save(schemes);
        }

		/// <summary>
		/// Loads the control schemes from an XML file, from Application.persistentDataPath.
		/// </summary>
		public static List<ControlScheme> LoadCustomControls()
		{
			List<ControlScheme> schemes = null;
#if UNITY_WINRT && !UNITY_EDITOR
			if(UnityEngine.Windows.File.Exists(customControlBindingsPath))
#else
			if(System.IO.File.Exists(customControlBindingsPath))
#endif
			{
				schemes = new InputLoaderXML(customControlBindingsPath).Load();
				Debug.Log("Loaded custom control bindings from :: " + customControlBindingsPath);
			}
			return schemes;
		}
    
        public static bool ScanInputForControlRebind (InputBinding inputBinding, bool changingPositiveDigitalAxis, System.Action onStopScan) {
			if (ScanService.isScanning) {
                return false;
            }
            CustomInput.RunCoroutine(StartInputScanDelayed(inputBinding, changingPositiveDigitalAxis, onStopScan));
            return true;
		}


		static bool changingPositive;
        static InputBinding inputBinding;

        static IEnumerator StartInputScanDelayed(InputBinding inputBinding, bool changingPositiveDigitalAxis, System.Action onStopScan)
		{
			
			yield return null; // delay before scanning
            if (inputBinding.Type == InputType.MouseAxis) {
                Debug.LogError("Error, cant rebind Mouse Axes...");
            }
            CustomProjectInputs.inputBinding = inputBinding;
            CustomProjectInputs.changingPositive = true;

            if (inputBinding.Type == InputType.DigitalAxis) {
                CustomProjectInputs.changingPositive = changingPositiveDigitalAxis;
                ScanService.Start(ScanFlags.Key, HandleKeyScan, onStopScan);	
            }
            if (inputBinding.Type == InputType.KeyButton) {
                ScanService.Start(ScanFlags.Key, HandleKeyScan, onStopScan);
            }
            if (inputBinding.Type == InputType.GamepadAxis) {
                ScanService.Start(ScanFlags.JoystickAxis, HandleJoystickAxisScan, onStopScan);
            }
            if (inputBinding.Type == InputType.GamepadButton || inputBinding.Type == InputType.GamepadAnalogButton) {
                ScanFlags flags = ScanFlags.JoystickButton;
				flags |= ScanFlags.JoystickAxis;
				ScanService.Start(flags, HandleJoystickButtonScan, onStopScan);	
            }
		}

		//	When you return false you tell the InputManager that it should keep scaning for other keys
		static bool HandleKeyScan(ScanResult result)
		{
			//	If the key is KeyCode.Backspace clear the current binding
			KeyCode Key = (result.keyCode == KeyCode.Backspace) ? KeyCode.None : result.keyCode;
			if(changingPositive)
				inputBinding.Positive = Key;
			else
				inputBinding.Negative = Key;
			return true;
		
		}
				
		//	When you return false you tell the InputManager that it should keep scaning for other keys
		static bool HandleJoystickButtonScan(ScanResult result)
		{
			
			if(result.ScanFlags == ScanFlags.JoystickButton)
			{
                // pressed a button
				if (result.gamepadButton != GamepadButton.None)
				{
					inputBinding.Type = InputType.GamepadButton;
					inputBinding.GamepadButton = result.gamepadButton;
					return true;
				}
			}
			else
			{
                // detected an axis movement
				if(result.gamepadAxis != GamepadAxis.None)
				{
					inputBinding.Type = InputType.GamepadAnalogButton;
					inputBinding.useNegativeAxisForButton = result.axisValue < 0.0f;
					inputBinding.GamepadAxis = result.gamepadAxis;
					return true;
				}
			}
			return false;
		}
		//	When you return false you tell the InputManager that it should keep scaning for other keys
		static bool HandleJoystickAxisScan(ScanResult result)
		{
			if (result.gamepadAxis != GamepadAxis.None) {
				inputBinding.GamepadAxis = result.gamepadAxis;
				return true;
			}
			return false;
		}
        
        
    }
}
