using UnityEngine;
using UnityEditor;
using System.IO;
using UnityInputConverter;

namespace CustomInputManager.Editor {
    public static class ConvertUnityInputManager
    {
        public static void ShowStartupWarning()
		{
			string key = PlayerSettings.companyName + "." + PlayerSettings.productName + ".InputManager.StartupWarning";
			if(!EditorPrefs.GetBool(key, false))
			{
				string message = "In order to use the Custom Input Manager plugin you need to overwrite your project's input settings.\n\nDo you want to overwrite the input settings now?\nYou can always do it later from the File menu.";
				if(EditorUtility.DisplayDialog("Warning", message, "Yes", "No"))
				{
					if(OverwriteProjectSettings())
						EditorPrefs.SetBool(key, true);
				}
			}
		}
		
		public static bool OverwriteProjectSettings()
		{
			int length = Application.dataPath.LastIndexOf('/');
			string projectSettingsFolder = Application.dataPath.Substring(0, length) + "/ProjectSettings";

			if(!Directory.Exists(projectSettingsFolder))
			{
				EditorUtility.DisplayDialog("Error", "Unable to get the correct path to the ProjectSetting folder.", "OK");
				return false;
			}

			new InputConverter().GenerateDefaultUnityInputManager(projectSettingsFolder + "/InputManager.asset");
			EditorUtility.DisplayDialog("Success", "The input settings have been successfully replaced.\n\nYou might need to minimize and restore Unity to reimport the new settings.", "OK");
			return true;
		}        
    }
}

