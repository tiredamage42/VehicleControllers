// MouseLook rotates the transform based on the mouse delta.
// Minimum and Maximum values can be used to constrain the possible rotation
// To make an FPS style character:
// - Create a capsule.
// - Add the MouseLook script to the capsule.
//   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
// - Add FPSInputController script to the capsule
//   -> A CharacterMotor and a CharacterController component will be automatically added.
// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
// - Add a MouseLook script to the camera.
//   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
using UnityEngine;
using System.Collections;

namespace CustomInputManager.Examples
{
	public class MouseLook : MonoBehaviour 
	{
		public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
		public RotationAxes axes = RotationAxes.MouseXAndY;
		public float sensitivityX = 15F;
		public float sensitivityY = 15F;

		public float minimumY = -60F;
		public float maximumY = 60F;

		private float rotationY = 0F;

		private void Start()
		{
			// Make the rigid body not change rotation
			if (GetComponent<Rigidbody>())
				GetComponent<Rigidbody>().freezeRotation = true;
		}

		int _MouseX, _MouseY;
        void InitializeInputNameKeys () {
            _MouseX = CustomInput.Name2Key("MouseX");
            _MouseY = CustomInput.Name2Key("MouseY");
            
        }


		void OnEnable () {
			InitializeInputNameKeys();
		}

		private void Update()
		{
			if(GameManager.IsPaused)
				return;

			if (axes == RotationAxes.MouseXAndY)
			{
				float rotationX = transform.localEulerAngles.y + CustomInput.GetAxis(_MouseX) * sensitivityX;
				
				rotationY += CustomInput.GetAxis(_MouseY) * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
			}
			else if (axes == RotationAxes.MouseX)
			{
				transform.Rotate(0, CustomInput.GetAxis(_MouseX) * sensitivityX, 0);
			}
			else
			{
				rotationY += CustomInput.GetAxis(_MouseY) * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
			}
		}
	}
}