using UnityEngine;

using UnityEditor;
using UnityEditor.SceneManagement;
using CustomInputManager.Internal;

namespace CustomInputManager.Editor {

    [System.Serializable] public class GamepadTestSceneEditor
    {
        public string lastScene;
        const string testSceneName = "CustomInputManagerGamepadTestScene";
        public void StartTestScene () {

            Object asset = Resources.Load(InputManager.resourcesFolder + testSceneName);
            if (asset != null)
            {
                string scene = AssetDatabase.GetAssetPath(asset);
                lastScene = EditorSceneManager.GetActiveScene().path;
                if (lastScene == scene) {
					lastScene = null;
                }
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(scene);
                EditorApplication.isPlaying = true;
            }   
        }
        public void OnPlayStateChanged (PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredEditMode) {
				if (!string.IsNullOrEmpty(lastScene)) {
				    EditorSceneManager.OpenScene(lastScene);
                    lastScene = null;
                }
            }
        }
    }
}
