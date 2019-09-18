using UnityEngine;
using System.Collections;

namespace CustomInputManager.Examples
{
	[RequireComponent(typeof(CharacterMotor))]
	public class FPSInputController : MonoBehaviour
	{
		private CharacterMotor motor;

		private void Awake()
		{
			motor = GetComponent<CharacterMotor>();
		}

		int _HorizontalAxis, _VerticalAxis, _JumpButton;
        void InitializeInputNameKeys () {
            _HorizontalAxis = CustomInput.Name2Key("Horizontal");
            _VerticalAxis = CustomInput.Name2Key("Vertical");
            _JumpButton = CustomInput.Name2Key("Jump");
        }

		void OnEnable () {
			InitializeInputNameKeys();
		}

		private	void Update() 
		{
			// Get the input vector from keyboard or analog stick
			var directionVector = new Vector3(CustomInput.GetAxis(_HorizontalAxis), 0, CustomInput.GetAxis(_VerticalAxis));
			// Debug.Log(directionVector);
			if (directionVector != Vector3.zero) 
			{
				// Get the length of the directon vector and then normalize it
				// Dividing by the length is cheaper than normalizing when we already have the length anyway
				var directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;
				
				// Make sure the length is no bigger than 1
				directionLength = Mathf.Min(1, directionLength);
				
				// Make the input vector more sensitive towards the extremes and less sensitive in the middle
				// This makes it easier to control slow speeds when using analog sticks
				directionLength = directionLength * directionLength;
				
				// Multiply the normalized direction vector by the modified length
				directionVector = directionVector * directionLength;
			}
			
			// Apply the direction to the CharacterMotor
			motor.inputMoveDirection = transform.rotation * directionVector;


			motor.inputJump = CustomInput.GetButtonDown(_JumpButton);
			
		}
	}
}