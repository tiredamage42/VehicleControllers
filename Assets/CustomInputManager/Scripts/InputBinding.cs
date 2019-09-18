
using UnityEngine;

namespace CustomInputManager
{
	public enum ButtonState { Pressed, JustPressed, Released, JustReleased }
	public enum InputType
	{
		KeyButton, MouseAxis, DigitalAxis,
		GamepadButton, GamepadAnalogButton, GamepadAxis
	}

	[System.Serializable] public class InputBinding
	{

		public static readonly string[] mouseAxisNames = new string[] { "Mouse X", "Mouse Y", "Mouse Scroll" };
		
		// cache to not allocate string memory every frame...
		string _rawMouseAxisName { get { return string.Concat("mouse_axis_", mouseAxis); } } 
		string rawMouseAxisName;
		public string GetAsString(bool usePositive) {

			switch (m_type) {

				case InputType.KeyButton:
					return m_positive.ToString();
				case InputType.MouseAxis:
					return mouseAxisNames[mouseAxis];
				case InputType.DigitalAxis:
					return usePositive ? m_positive.ToString() : m_negative.ToString();
				case InputType.GamepadButton:
					return m_gamepadButton.ToString();
				case InputType.GamepadAnalogButton:
					return (useNegativeAxisForButton ? "-" : "+") + GamepadAxis.ToString();
				case InputType.GamepadAxis:
					return GamepadAxis.ToString();
			}
			return "ERROR";
		}


		public const float AXIS_NEUTRAL = 0.0f;
		public const float AXIS_POSITIVE = 1.0f;
		public const float AXIS_NEGATIVE = -1.0f;
		public const int MAX_MOUSE_AXES = 3;
		public const int MAX_JOYSTICK_AXES = 28;
        public const int MAX_JOYSTICK_BUTTONS = 20;
        public const int MAX_UNITY_JOYSTICKS = 11;

		[SerializeField] private KeyCode m_positive;
		[SerializeField] private KeyCode m_negative;
		[SerializeField] private float m_deadZone = .2f;
		[SerializeField] private float m_gravity = 1;
		[SerializeField] private float m_sensitivity = 1;
		[SerializeField] private bool m_snap;
		[SerializeField] private bool m_invert;
		public bool useNegativeAxisForButton;
		public bool rebindable, sensitivityEditable, invertEditable;


		[SerializeField] private InputType m_type;
		[SerializeField] private int mouseAxis;
		[SerializeField] private GamepadAxis m_gamepadAxis;
		[SerializeField] private GamepadButton m_gamepadButton;

		public KeyCode Positive
		{
			get { return m_positive; }
			set { m_positive = value; }
		}
		public KeyCode Negative
		{
			get { return m_negative; }
			set { m_negative = value; }
		}
		public float DeadZone
		{
			get { return m_deadZone; }
			set { m_deadZone = Mathf.Max(value, 0.0f); }
		}
		public float Gravity
		{
			get { return m_gravity; }
			set { m_gravity = Mathf.Max(value, 0.0f); }
		}
		public float Sensitivity
		{
			get { return m_sensitivity; }
			set { m_sensitivity = Mathf.Max(value, 0.0f); }
		}
		public bool SnapWhenReadAsAxis
		{
			get { return m_snap; }
			set { m_snap = value; }
		}
		public bool InvertWhenReadAsAxis
		{
			get { return m_invert; }
			set { m_invert = value; }
		}
		public InputType Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		public int MouseAxis
		{
			get { return mouseAxis; }
			set { mouseAxis = Mathf.Clamp(value, 0, MAX_MOUSE_AXES - 1); }
		}
		public GamepadButton GamepadButton
		{
			get { return m_gamepadButton; }
			set { m_gamepadButton = value; }
		}
		public GamepadAxis GamepadAxis
		{
			get { return m_gamepadAxis; }
			set { m_gamepadAxis = value; }
		}

		public InputBinding()
		{
			m_positive = KeyCode.None;
			m_negative = KeyCode.None;
			m_type = InputType.KeyButton;
			m_gravity = 1.0f;
			m_sensitivity = 1.0f;
		}

		float digitalAxisValue;
		float[] buttonsAsAxisValue;
		ButtonState[] axesAsButtonValues;

		int maxJoysticks;
		void Reset(int maxJoysticks)
		{
			this.maxJoysticks = maxJoysticks;

			if (buttonsAsAxisValue == null) buttonsAsAxisValue = new float[maxJoysticks];
			if (axesAsButtonValues == null) axesAsButtonValues = new ButtonState[maxJoysticks];

			if (m_type == InputType.DigitalAxis) {
				digitalAxisValue = AXIS_NEUTRAL;
			}

			for (int i = 0; i < maxJoysticks; i++) {
				buttonsAsAxisValue[i] = AXIS_NEUTRAL;
				axesAsButtonValues[i] = ButtonState.Released;
			}
			
			rawMouseAxisName = _rawMouseAxisName;

		}
		public void Initialize(int maxJoysticks)
		{
			Reset(maxJoysticks);
		}
		public void Update(float deltaTime)
		{

			if (m_type == InputType.GamepadAnalogButton) {
				for (int i = 0; i < maxJoysticks; i++) {
					if (GamepadHandler.GamepadAvailable(i, out _)) {
						UpdateAxisAsButtons(i, GamepadHandler.GetAxis(m_gamepadAxis, i));
					}
				}
			}
			else if (m_type == InputType.GamepadAxis) {
				for (int i = 0; i < maxJoysticks; i++) {
					if (GamepadHandler.GamepadAvailable(i, out _)) {
						UpdateAxisAsButtons(i, GetAxis(i));
					}
				}
			}
			else if (m_type == InputType.MouseAxis) {
				UpdateAxisAsButtons(0, GetAxis(0));
			}

			if(m_type == InputType.DigitalAxis) {
				UpdateDigitalAxisValue(deltaTime);
			}
			else if (m_type == InputType.KeyButton) {
				UpdateButtonAsAxisValue(deltaTime, 0);
			}
			else if (m_type == InputType.GamepadButton || m_type == InputType.GamepadAnalogButton){
				for (int i = 0; i < maxJoysticks; i++) {
					if (GamepadHandler.GamepadAvailable(i, out _)) {
						UpdateButtonAsAxisValue(deltaTime, i);
					}
				}
			}	
		}

		void UpdateButtonAsAxisValue (float deltaTime, int index) {
			bool buttonDown = GetButton(index);

			float v = buttonsAsAxisValue[index];

			if (buttonDown) {
				if (v < AXIS_NEUTRAL && m_snap)
					v = AXIS_NEUTRAL;

				if (v < AXIS_POSITIVE) {
					v += m_sensitivity * deltaTime;
					if (v > AXIS_POSITIVE) v = AXIS_POSITIVE;
				}
			}
			else
			{
				if (v > AXIS_NEUTRAL) {
					v -= m_gravity * deltaTime;
					if (v < AXIS_NEUTRAL) v = AXIS_NEUTRAL;
				}
			}
			buttonsAsAxisValue[index] = v;
		}

		private void UpdateDigitalAxisValue(float deltaTime)
		{
			bool posDown = Input.GetKey(m_positive);
			bool negDown = Input.GetKey(m_negative);
			
			if (posDown && negDown) return;
			
			if(posDown) {
				if(digitalAxisValue < AXIS_NEUTRAL && m_snap) digitalAxisValue = AXIS_NEUTRAL;
				
				if (digitalAxisValue < AXIS_POSITIVE) {
					digitalAxisValue += m_sensitivity * deltaTime;
					if(digitalAxisValue > AXIS_POSITIVE) digitalAxisValue = AXIS_POSITIVE;
				}
			}
			else if(negDown)
			{
				if(digitalAxisValue > AXIS_NEUTRAL && m_snap) digitalAxisValue = AXIS_NEUTRAL;
				
				if (digitalAxisValue > AXIS_NEGATIVE) {
					digitalAxisValue -= m_sensitivity * deltaTime;
					if(digitalAxisValue < AXIS_NEGATIVE) digitalAxisValue = AXIS_NEGATIVE;
				}	
			}
			else
			{
				if(digitalAxisValue < AXIS_NEUTRAL)
				{
					digitalAxisValue += m_gravity * deltaTime;
					if(digitalAxisValue > AXIS_NEUTRAL) digitalAxisValue = AXIS_NEUTRAL;	
				}
				else if(digitalAxisValue > AXIS_NEUTRAL)
				{
					digitalAxisValue -= m_gravity * deltaTime;
					if(digitalAxisValue < AXIS_NEUTRAL) digitalAxisValue = AXIS_NEUTRAL;
				}
			}
		}
		

		private void UpdateAxisAsButtons(int index, float axis)
		{

			// inverts if the axis to check is negative
			axis = useNegativeAxisForButton ? -axis : axis;

			ButtonState v = axesAsButtonValues[index];
				
			if(axis > m_deadZone)
			{
				if (v == ButtonState.Released || v == ButtonState.JustReleased)
					v = ButtonState.JustPressed;
				else if (v == ButtonState.JustPressed)
					v = ButtonState.Pressed;
			}
			else
			{
				if (v == ButtonState.Pressed || v == ButtonState.JustPressed)
					v = ButtonState.JustReleased;
				else if (v == ButtonState.JustReleased)
					v = ButtonState.Released;
			}
			axesAsButtonValues[index] = v;
			
		}



		#region  GETTERS
		public bool AnyInput (int playerID)
		{
			switch(m_type)
			{
				case InputType.KeyButton:
					return Input.GetKey(m_positive);
				case InputType.GamepadAnalogButton:
					return axesAsButtonValues[playerID] == ButtonState.Pressed || axesAsButtonValues[playerID] == ButtonState.JustPressed;
				case InputType.GamepadButton:
					return GamepadHandler.GetButton(m_gamepadButton, playerID);
				case InputType.GamepadAxis:
					return GamepadHandler.GetAxisRaw(m_gamepadAxis, playerID, m_deadZone) != 0f;
				case InputType.DigitalAxis:
					return digitalAxisValue != 0;
				case InputType.MouseAxis:
					return Input.GetAxisRaw(rawMouseAxisName) != 0;
					
				default:
					return false;
			}
		}
		
		public float GetAxis(int playerID)
		{
			float axis = 0;
		
		
			if(m_type == InputType.DigitalAxis)
			{
				axis = m_invert ? -digitalAxisValue : digitalAxisValue;
			}

			else if(m_type == InputType.MouseAxis)
			{
				axis = Input.GetAxis(rawMouseAxisName) * m_sensitivity;
				axis = m_invert ? -axis : axis;
			}
			else if(m_type == InputType.GamepadAxis)
			{
				axis = GamepadHandler.GetAxis(m_gamepadAxis, playerID);
				if ((axis < 0 && axis > -m_deadZone) || (axis > 0 && axis < m_deadZone))
					axis = AXIS_NEUTRAL;
				
				axis = Mathf.Clamp(axis * m_sensitivity, -1, 1);
				axis = m_invert ? -axis : axis;
			}

			else if (m_type == InputType.KeyButton) {
				axis = buttonsAsAxisValue[0];
				axis = m_invert ? -axis : axis;
			}
			else if (m_type == InputType.GamepadButton) {
				axis = buttonsAsAxisValue[playerID];
				axis = m_invert ? -axis : axis;
			}
			else if (m_type == InputType.GamepadAnalogButton) {
				axis = buttonsAsAxisValue[playerID];
				axis = m_invert ? -axis : axis;
			}

			return axis;
		}

		public float GetAxisRaw(int playerID)
		{
			float axis = 0;

			if(m_type == InputType.DigitalAxis)
			{
				if(Input.GetKey(m_positive))
					axis = m_invert ? -AXIS_POSITIVE : AXIS_POSITIVE;
				else if(Input.GetKey(m_negative))
					axis = m_invert ? -AXIS_NEGATIVE : AXIS_NEGATIVE;
			}
			else if(m_type == InputType.MouseAxis)
			{
				axis = Input.GetAxisRaw(rawMouseAxisName);
				axis = m_invert ? -axis : axis;
			}
			else if(m_type == InputType.GamepadAxis)
			{
				axis = GamepadHandler.GetAxisRaw(m_gamepadAxis, playerID, m_deadZone);
				axis = m_invert ? -axis : axis;
			}

			else if (m_type == InputType.KeyButton) {
				if (GetButton(playerID)) axis = m_invert ? -AXIS_POSITIVE : AXIS_POSITIVE;
			}
			else if (m_type == InputType.GamepadButton) {
				if (GetButton(playerID)) axis = m_invert ? -AXIS_POSITIVE : AXIS_POSITIVE;
			}
			else if (m_type == InputType.GamepadAnalogButton) {
				if (GetButton(playerID)) axis = m_invert ? -AXIS_POSITIVE : AXIS_POSITIVE;
			}


			return axis;
		}

		public bool GetButton(int playerID)
		{
			bool value = false;

			if (m_type == InputType.KeyButton)
				value = Input.GetKey(m_positive);
			else if (m_type == InputType.GamepadButton)
				value = GamepadHandler.GetButton(m_gamepadButton, playerID);
			else if ( m_type == InputType.GamepadAnalogButton)
				value = axesAsButtonValues[playerID] == ButtonState.Pressed || axesAsButtonValues[playerID] == ButtonState.JustPressed;
			else if ( m_type == InputType.DigitalAxis ) 
				value = Input.GetKey(useNegativeAxisForButton ? m_negative : m_positive);
			else if (m_type == InputType.GamepadAxis) 
				value = axesAsButtonValues[playerID] == ButtonState.Pressed || axesAsButtonValues[playerID] == ButtonState.JustPressed;
			else if (m_type == InputType.MouseAxis) 
				value = axesAsButtonValues[playerID] == ButtonState.Pressed || axesAsButtonValues[playerID] == ButtonState.JustPressed;


			return value;
		}

		public bool GetButtonDown(int playerID)
		{
			bool value = false;

			if (m_type == InputType.KeyButton)
				value = Input.GetKeyDown(m_positive);
			else if (m_type == InputType.GamepadButton)
				value = GamepadHandler.GetButtonDown(m_gamepadButton, playerID);
			else if ( m_type == InputType.GamepadAnalogButton)
				value = axesAsButtonValues[playerID] == ButtonState.JustPressed;
			else if ( m_type == InputType.DigitalAxis ) 
				value = Input.GetKeyDown(useNegativeAxisForButton ? m_negative : m_positive);
			else if (m_type == InputType.GamepadAxis) 
				value = axesAsButtonValues[playerID] == ButtonState.JustPressed;
			else if (m_type == InputType.MouseAxis) 
				value = axesAsButtonValues[playerID] == ButtonState.JustPressed;
			
			return value;
		}

		public bool GetButtonUp(int playerID)
		
		{
			bool value = false;

			if(m_type == InputType.KeyButton)
				value = Input.GetKeyUp(m_positive);
			else if(m_type == InputType.GamepadButton)
				value = GamepadHandler.GetButtonUp(m_gamepadButton, playerID);
			else if( m_type == InputType.GamepadAnalogButton)
				value = axesAsButtonValues[playerID] == ButtonState.JustReleased;
			else if ( m_type == InputType.DigitalAxis )
				value = Input.GetKeyUp(useNegativeAxisForButton ? m_negative : m_positive);
			else if (m_type == InputType.GamepadAxis) 
				value = axesAsButtonValues[playerID] == ButtonState.JustReleased;
			else if (m_type == InputType.MouseAxis) 
				value = axesAsButtonValues[playerID] == ButtonState.JustReleased;				

			return value;
		}
		#endregion

		
		public void Copy(InputBinding source)
		{
			m_positive = source.m_positive;
			m_negative = source.m_negative;

			m_deadZone = source.m_deadZone;
			m_gravity = source.m_gravity;
			m_sensitivity = source.m_sensitivity;
			m_snap = source.m_snap;
			m_invert = source.m_invert;
			useNegativeAxisForButton = source.useNegativeAxisForButton;
			m_type = source.m_type;

			rebindable = source.rebindable;
			sensitivityEditable = source.sensitivityEditable;
			invertEditable = source.invertEditable;

			mouseAxis = source.mouseAxis;
			m_gamepadAxis = source.m_gamepadAxis;
			m_gamepadButton = source.m_gamepadButton;
		}
	}
}