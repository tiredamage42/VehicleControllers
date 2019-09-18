using UnityEngine;

namespace CustomInputManager.Internal {

    [CreateAssetMenu(menuName="Custom Input Manager/Replacement Input Manager", fileName="InputManager")]
    public class InputManager : ScriptableObject
    {
        public int _maxPlayers = 2;
        
        [Space] [Header("Gamepads")]
        public float _dpadGravity = 3.0f;
        public float _dpadSensitivity = 3.0f;
        public bool _dpadSnap = true;
        [Header("Interval (in seconds) to check for joystick connections")]
        [Range(.01f, 5.0f)] public float _joystickCheckFrequency = 1.0f;

        public static int maxPlayers {
            get {
                if (instance == null) return 0;
                return instance._maxPlayers;
            }
        }
        public static float joystickCheckFrequency {
            get {
                if (instance == null) return 1;
                return instance._joystickCheckFrequency;
            }
        }
        public static float dpadGravity {
            get {
                if (instance == null) return 3;
                return instance._dpadGravity;
            }
        }
        public static float dpadSensitivity {
            get {
                if (instance == null) return 3;
                return instance._dpadSensitivity;
            }
        }
        public static bool dpadSnap {
            get {
                if (instance == null) return true;
                return instance._dpadSnap;
            }
        }
        
        public const string resourcesFolder = "CustomInputManager/";
        const string resourcesPath = resourcesFolder + "InputManager";
        
        static InputManager _instance;
        public static InputManager instance {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<InputManager>(resourcesPath);
                }
                if (_instance == null) {
                    Debug.LogError("Input Manager object not found! Create a new one at path: 'Resources/" + resourcesPath + ".asset.\nProject View -> Right Click -> Custom Input Manager -> Replacement Input Manager");
                }
                return _instance;       
            }
        }


        public static int EncodeActionName (string actionName) {
			return Shader.PropertyToID(actionName);
		}

        #if UNITY_EDITOR
        public static string fullResourcesDirectory {
            get {
                if (instance == null)
                    return string.Empty;

                string directory = System.IO.Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(instance));
                if (!directory.EndsWith("/"))
                    directory += "/";
                
                return directory;
            }
        }
        #endif
    }
 }

