
using UnityEngine;
using Syd.UI;
using System.Collections.Generic;
namespace CustomInputManager.Examples
{
	public class ControlSchemeEditPage : MonoBehaviour 
	{
		public UIButtonProfile normalProfile, scanningProfile;
		public RectTransform rebindParent;
		public GameObject rebindElement;
		[SerializeField] private string m_controlSchemeName = null;
		[SerializeField] private int bindingIndex = 0;

		List<RebindInputButton> rebinders = new List<RebindInputButton>();

		void BuildRebindElements () {
			ControlScheme controlScheme = CustomInput.GetControlScheme(m_controlSchemeName);
			
			for (int i = 0; i < controlScheme.Actions.Count; i++) {

				InputBinding binding = controlScheme.Actions[i].GetBinding(bindingIndex);

				if (binding.rebindable || binding.invertEditable || binding.sensitivityEditable) {

					GameObject newRebindElement = Instantiate(rebindElement);
					newRebindElement.transform.SetParent(rebindParent);

					RebindInputButton rebinder = newRebindElement.GetComponent<RebindInputButton>();
					rebinder.Initialize(binding, controlScheme.Actions[i], normalProfile, scanningProfile);
					rebinders.Add(rebinder);
				}
			}
		}

		void Start () {
			BuildRebindElements();
		}
		
		public void ResetScheme()
		{
			if (CustomInput.RevertSchemeToDefaults(m_controlSchemeName, bindingIndex)) {
				foreach (var t in rebinders) t.RefreshText();
			}
		}
	}
}