using UnityEngine;
using Syd.UI;
using UnityEngine.UI;

namespace CustomInputManager.Examples
{

	/*
	
		change:
			-/+ binding (+ only if digital axis)
			invert (if gamepad axis or mouse axis or digital axis)
			sensitivity (if gamepad axis or mouse axis or digital axis)
	*/
	public class RebindInputButton : MonoBehaviour
	{
		UIButtonProfile normalProfile, scanningProfile;
		UIButton[] uIButtons;

		InputBinding inputBinding;
		InputAction inputAction;

		UIButton negativeButton { get { return uIButtons[0]; } }
		UIButton positiveButton { get { return uIButtons[1]; } }

		void ResetNormalButtonProfiles () {
			for (int i = 0; i < uIButtons.Length; i++) uIButtons[i].profile = normalProfile;
		}

		public void Initialize (InputBinding inputBinding, InputAction inputAction, UIButtonProfile normalProfile, UIButtonProfile scanningProfile) {
			this.inputBinding = inputBinding;
			this.inputAction = inputAction;
			this.normalProfile = normalProfile;
			this.scanningProfile = scanningProfile;

			RefreshText();

			Toggle invertToggle = GetComponentInChildren<Toggle>();

			Text displayNameText = GetComponentInChildren<Text>();
			displayNameText.text = inputAction.displayName;
			displayNameText.color = negativeButton.profile.textColor;

			ResetNormalButtonProfiles();
			
			GameObject axisControlSection = invertToggle.transform.parent.gameObject;
			bool isAxis = inputBinding.Type == InputType.DigitalAxis || inputBinding.Type == InputType.GamepadAxis || inputBinding.Type == InputType.MouseAxis;
			axisControlSection.SetActive(isAxis && (inputBinding.invertEditable || inputBinding.sensitivityEditable));

			
			// cnat rebind mouse axis
			negativeButton.gameObject.SetActive(inputBinding.Type != InputType.MouseAxis && inputBinding.rebindable);
			
			// only digital axes can be rebound with different keys per +/-
			positiveButton.gameObject.SetActive(inputBinding.Type == InputType.DigitalAxis  && inputBinding.rebindable);
			

			if (axisControlSection.activeSelf) {
				if (inputBinding.invertEditable) {
					invertToggle.onValueChanged.AddListener( OnInvertChanged );
					invertToggle.isOn = inputBinding.InvertWhenReadAsAxis;
				}
				else {
					invertToggle.gameObject.SetActive(false);
				}
				
				Slider sensitivitySlider = GetComponentInChildren<Slider>();
				if (inputBinding.sensitivityEditable) {
					sensitivitySlider.onValueChanged.AddListener( OnSensitivityChange );
					sensitivitySlider.value = inputBinding.Sensitivity;
				}
				else {
					sensitivitySlider.gameObject.SetActive(false);
				}

			}

			if (negativeButton.gameObject.activeSelf) negativeButton.onClick += OnClickNegative;
			if (positiveButton.gameObject.activeSelf) positiveButton.onClick += OnClickPositive;
		}
		
		void Awake()
		{
			uIButtons = GetComponentsInChildren<UIButton>();
		}

		string GetButtonPrefix (InputBinding inputBinding, bool positive) {
			return inputBinding.Type != InputType.DigitalAxis ? "" : "( " + (positive ? "+" : "-") + " ) ";
		}

		void OnClick (InputBinding inputBinding, UIButton button, bool changePositive) {
			if (CustomInput.ScanInputForControlRebind(inputBinding, changePositive, OnStopScan)) {
				// override ui input
				UIUtils.OverrideUIInputControl();
				// update button visual
				button.profile = scanningProfile;
				button.text.text = GetButtonPrefix(inputBinding, changePositive) + "...";
			}
		}

		void OnClickNegative() {
			OnClick(inputBinding, negativeButton, false);
		}

		void OnClickPositive() {
			OnClick(inputBinding, positiveButton, true);
		}

		void OnSensitivityChange (float value) {
			inputBinding.Sensitivity = value;
			CustomInput.SaveCustomControls();
		}
		void OnInvertChanged(bool value) {
			inputBinding.InvertWhenReadAsAxis = value;
			CustomInput.SaveCustomControls();
		}

		public void RefreshText () {
			ResetNormalButtonProfiles();
			for (int i = 0; i < 2; i++) 
				if (uIButtons[i].gameObject.activeSelf) 
					uIButtons[i].text.text = GetButtonPrefix(inputBinding, i == 1) + inputBinding.GetAsString(i == 1);
		}

		void OnStopScan() {
			CustomInput.SaveCustomControls();
			RefreshText();
			UIUtils.RestoreUIInputControl();
		}	
	}
}