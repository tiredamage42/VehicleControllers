using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using CustomInputManager.Internal;
namespace CustomInputManager
{
    public enum GamepadAxis
	{
		LeftThumbstickX = 0, LeftThumbstickY = 1,
		RightThumbstickX = 2, RightThumbstickY = 3,
		DPadX = 4, DPadY = 5,
		LeftTrigger = 6, RightTrigger = 7,
		None = 8
	}

    public enum GamepadButton
	{
		LeftStick = 0, RightStick = 1,
		LeftBumper = 2, RightBumper = 3,
		DPadUp = 4, DPadDown = 5, DPadLeft = 6, DPadRight = 7,
		Back = 8, Start = 9,
		ActionBottom = 10, ActionRight = 11, ActionLeft = 12, ActionTop = 13,
		None = 14
	}

    public enum GamepadDPadType { Axis = 0, Button }

    public enum GamePadPossiblePlatform { Linux, OSX, Windows, PS4, XboxOne };
    
    // [System.Serializable] public class GamepadHandler
    
    public static class GamepadHandler
    {   
        static bool CurrentPlatformInGamepadPlatforms (List<GamePadPossiblePlatform> platforms) {
            RuntimePlatform platform = Application.platform;
            if (platform == RuntimePlatform.LinuxEditor || platform == RuntimePlatform.LinuxPlayer) 
                return platforms.Contains(GamePadPossiblePlatform.Linux);
            else if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer) 
                return platforms.Contains(GamePadPossiblePlatform.OSX);
            else if (platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer) 
                return platforms.Contains(GamePadPossiblePlatform.Windows);
            else if (platform == RuntimePlatform.PS4) 
                return platforms.Contains(GamePadPossiblePlatform.PS4);
            else if (platform == RuntimePlatform.XboxOne) 
                return platforms.Contains(GamePadPossiblePlatform.XboxOne);

            Debug.LogError("Platform: " + platform + " not supported by gamepads via Custom Input Manager");
            return false;
        }

        static List<T> ToList<T>(T[] a) {
            List<T> r = new List<T>();
            for (int i = 0; i < a.Length; i++) {
                r.Add(a[i]);
            }
            return r;
        }

        static GenericGamepadProfile GetProfileForJoystickName(string joystickName, GenericGamepadProfile[] allProfiles) {
            for (int i = 0; i < allProfiles.Length; i++) {
                GenericGamepadProfile p = allProfiles[i];
                if (CurrentPlatformInGamepadPlatforms(ToList(p.platforms))) {
                    for (int n = 0; n < p.unityJoystickNames.Length; n++) {
                        if (joystickName.Contains( p.unityJoystickNames[n] ) ) {
                            return p;
                        }
                    }
                }
            }
            Debug.LogError("Cant Find Profile for joystick: " + joystickName);
            return null;
        }


        struct DPadState
        {
            public Vector2 axes;
            public ButtonState Up, Down, Left, Right;
            public static DPadState Empty { get { return new DPadState() { Up = ButtonState.Released, Down = ButtonState.Released, Right = ButtonState.Released, Left = ButtonState.Released }; } }

            public void UpdateButtonStates (bool upPressed, bool downPressed, bool leftPressed, bool rightPressed) {
                Up = GetNewDPadButtonState(upPressed, Up);
                Down = GetNewDPadButtonState(downPressed, Down);
                Left = GetNewDPadButtonState(leftPressed, Left);
                Right = GetNewDPadButtonState(rightPressed, Right);
            }

            static ButtonState GetNewDPadButtonState(bool isPressed, ButtonState oldState)
            {
                ButtonState newState = isPressed ? ButtonState.Pressed : ButtonState.Released;
                
                if (oldState == ButtonState.Pressed || oldState == ButtonState.JustPressed)
                    newState = isPressed ? ButtonState.Pressed : ButtonState.JustReleased;

                else if (oldState == ButtonState.Released || oldState == ButtonState.JustReleased)
                    newState = isPressed ? ButtonState.JustPressed : ButtonState.Released;
                
                return newState;
            }
        }

        static GenericGamepadProfile[] gamepadProfilesPerGamepad, allGamepadProfiles;
        public const string gamepadProfilesResourcesDirectory = "GamepadProfiles";
        const string gamepadProfilesResourcesPath = InputManager.resourcesFolder + gamepadProfilesResourcesDirectory;

        public static GenericGamepadProfile[] LoadAllGamepadProfiles () {
            return Resources.LoadAll(gamepadProfilesResourcesPath, typeof(GenericGamepadProfile)).Cast<GenericGamepadProfile>().ToArray();
        }
        
        static float dpadGravity { get { return InputManager.dpadGravity; } }
        static float dpadSensitivity { get { return InputManager.dpadSensitivity; } }
        static bool dpadSnap { get { return InputManager.dpadSnap; } }
        static int maxJoysticks { get { return InputManager.maxPlayers; } }
        
        static DPadState[] m_dpadState;
        static string[] m_axisNameLookupTable;

        public static void OnAwake()//, int maxJoysticks, float joystickCheckFrequency, float dpadGravity, float dpadSensitivity, bool dpadSnap)
        {
            // this.maxJoysticks = maxJoysticks;
            // this.dpadGravity = dpadGravity;
            // this.dpadSensitivity = dpadSensitivity;
            // this.dpadSnap = dpadSnap;

            allGamepadProfiles = LoadAllGamepadProfiles();

            gamepadProfilesPerGamepad = new GenericGamepadProfile[maxJoysticks];
            gamepadNames = new string[maxJoysticks];
            gamepadConnectionStates = new bool[maxJoysticks];

            m_dpadState = new DPadState[maxJoysticks];
            for (int i = 0; i < maxJoysticks; i++) {
                m_dpadState[i] = DPadState.Empty;
            }

            GenerateAxisNameLookupTable();
            // CustomInput.RunCoroutine(CheckForGamepads());
        }

        public static void OnStart () {
            CustomInput.RunCoroutine(CheckForGamepads());
        }

        static string[] gamepadNames;
        static bool[] gamepadConnectionStates;

        static bool IndexInRange (int i) {
            if (i < 0 || i >= maxJoysticks) {
                Debug.LogError("Gamepad Index (" + i.ToString() + ") out of range, current range: " + maxJoysticks);
                return false;
            }
            return true;
        }
        
        static public string GamepadName (int gamepad) {
            if (!IndexInRange(gamepad)) return null;
            return gamepadNames[gamepad];
        }
        static public bool GamepadConnected (int gamepad) {
            if (!IndexInRange(gamepad)) return false;
            return gamepadConnectionStates[gamepad];
        }

        static  IEnumerator CheckForGamepads()
        {
            while(true)
            {
                string[] joystickNames = CustomInput.GetJoystickNames();

                int ln = joystickNames.Length;
                for(int i = 0; i < maxJoysticks; i++) {

                    bool wasConnected = gamepadConnectionStates[i];

                    bool connected = ln > i && !string.IsNullOrEmpty(joystickNames[i]);

                    gamepadConnectionStates[i] = connected;
                    gamepadNames[i] = connected ? joystickNames[i] : "Not Connected";

                    if (connected) {
                        if (!wasConnected){
                            gamepadProfilesPerGamepad[i] = GetProfileForJoystickName(joystickNames[i], allGamepadProfiles);
                            if (gamepadProfilesPerGamepad[i] == null) {
                                gamepadNames[i] = joystickNames[i] + " [ No Profile ]";
                            }
                            else {
                                Debug.Log("Assigned profile " + gamepadProfilesPerGamepad[i].name + " for joystick: " + joystickNames[i]);
                            }
                        }
                    }
                    else {
                        gamepadProfilesPerGamepad[i] = null;
                    }
                }
                
                yield return new WaitForSecondsRealtime(InputManager.joystickCheckFrequency);
            }
        }

        static void GenerateAxisNameLookupTable() {
            string template = "joy_{0}_axis_{1}";
            m_axisNameLookupTable = new string[maxJoysticks * InputBinding.MAX_JOYSTICK_AXES];
            for(int j = 0; j < maxJoysticks; j++) {
                for(int a = 0; a < InputBinding.MAX_JOYSTICK_AXES; a++) {
                    m_axisNameLookupTable[j * InputBinding.MAX_JOYSTICK_AXES + a] = string.Format(template, j, a);
                }
            }
        }

        public static bool GamepadAvailable (int gamepad, out GenericGamepadProfile profile) {
            profile = null;
            if (!GamepadConnected(gamepad)) return false;
            if (!CheckForGamepadProfile(gamepad, out profile)) return false;
            return true;
        }

        public static void OnUpdate(float deltaTime)
        {
            for(int i = 0; i < maxJoysticks; i++)
            {
                GenericGamepadProfile profile;
                if (!GamepadAvailable(i, out profile)) continue;
                
                if(profile.DPadType == GamepadDPadType.Button)
                {
                    // mimic axis values
                    UpdateDPadAxis(i, deltaTime, 0, profile.DPadRightButton, profile.DPadLeftButton);
                    UpdateDPadAxis(i, deltaTime, 1, profile.DPadUpButton, profile.DPadDownButton);
                }
                else
                {
                    // mimic button values
                    UpdateDPadButton(i, profile);
                }   
            }
        }

        static  void UpdateDPadAxis(int gamepad, float deltaTime, int axis, int posButton, int negButton)
        {
            bool posPressed = GetButton(posButton, gamepad);
            bool negPressed = GetButton(negButton, gamepad);

            float ax = m_dpadState[gamepad].axes[axis];

            if(posPressed)
            {
                if(ax < InputBinding.AXIS_NEUTRAL && dpadSnap) ax = InputBinding.AXIS_NEUTRAL;
                ax += dpadSensitivity * deltaTime;
                if(ax > InputBinding.AXIS_POSITIVE) ax = InputBinding.AXIS_POSITIVE;
            }
            else if(negPressed)
            {
                if(ax > InputBinding.AXIS_NEUTRAL && dpadSnap) ax = InputBinding.AXIS_NEUTRAL;
                ax -= dpadSensitivity * deltaTime;
                if(ax < InputBinding.AXIS_NEGATIVE) ax = InputBinding.AXIS_NEGATIVE;
            }
            else
            {
                if(ax < InputBinding.AXIS_NEUTRAL)
                {
                    ax += dpadGravity * deltaTime;
                    if(ax > InputBinding.AXIS_NEUTRAL) ax = InputBinding.AXIS_NEUTRAL;
                }
                else if(ax > InputBinding.AXIS_NEUTRAL)
                {
                    ax -= dpadGravity * deltaTime;
                    if(ax < InputBinding.AXIS_NEUTRAL) ax = InputBinding.AXIS_NEUTRAL;
                }
            }

            m_dpadState[gamepad].axes[axis] = ax;    
        }

        
        static readonly float dpadThreshold = 0.9f;
        static readonly float negDpadThreshold = -dpadThreshold;
        static void UpdateDPadButton(int gamepad, GenericGamepadProfile profile)
        {
            int jOffset = gamepad * InputBinding.MAX_JOYSTICK_AXES;
            float x = Input.GetAxis(m_axisNameLookupTable[jOffset + profile.DPadXAxis]);
            float y = Input.GetAxis(m_axisNameLookupTable[jOffset + profile.DPadYAxis]);
            m_dpadState[gamepad].UpdateButtonStates (y >= dpadThreshold, y <= negDpadThreshold, x <= negDpadThreshold, x >= dpadThreshold);
        }

    
        static HashSet<int> checkedGamepadLTriggersForInitialization = new HashSet<int>();
        static HashSet<int> checkedGamepadRTriggersForInitialization = new HashSet<int>();
        
        // xbox controller triggers on OSX initialize at 0 but have range -1, 1
        // that means when at rest initially, they return 0.5 instead of 0 when normalizing to 0..1 range
        
        // TODO: actaully check which othre platforms need this
        static float AdjustOSXAxis (float rawAxis, int gamepad, ref HashSet<int> checkSet) {

            float adjustedAxis = 0.0f;
            bool checkedTrigger = checkSet.Contains(gamepad);
            if (!checkedTrigger) {
                // the way to tell if we've used the axis and initialized the -1,1 range
                // is if the axis is negative
                if (rawAxis > -0.9f && rawAxis < -0.0001f){
                    checkSet.Add(gamepad);
                    checkedTrigger = true;
                }
            }
            if(checkedTrigger) {
                adjustedAxis = (rawAxis + 1.0f) * 0.5f;
            }
            return adjustedAxis;
        }


        public static float GetAxis(GamepadAxis axis, int gamepad)
        {

            GenericGamepadProfile profile;
            if (!GamepadAvailable(gamepad, out profile)) return 0.0f;
            
            int axisID = -1;

            switch(axis)
            {
                case GamepadAxis.LeftThumbstickX:  axisID = profile.LeftStickXAxis; break;
                case GamepadAxis.LeftThumbstickY:  axisID = profile.LeftStickYAxis; break;
                case GamepadAxis.RightThumbstickX: axisID = profile.RightStickXAxis; break;
                case GamepadAxis.RightThumbstickY: axisID = profile.RightStickYAxis; break;
                
                case GamepadAxis.DPadX: 
                    if (profile.DPadType == GamepadDPadType.Button) return m_dpadState[gamepad].axes[0];
                    axisID = profile.DPadXAxis; 
                    break;
                case GamepadAxis.DPadY: 
                    if (profile.DPadType == GamepadDPadType.Button) return m_dpadState[gamepad].axes[1];
                    axisID = profile.DPadYAxis; 
                    break;
                
                case GamepadAxis.LeftTrigger:  return AdjustOSXAxis (Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + profile.LeftTriggerAxis]), gamepad, ref checkedGamepadLTriggersForInitialization);
                case GamepadAxis.RightTrigger: return AdjustOSXAxis (Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + profile.RightTriggerAxis]), gamepad, ref checkedGamepadRTriggersForInitialization);
            }

            return axisID >= 0 ? Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + axisID]) : 0.0f;
        }

        public static float GetAxisRaw(GamepadAxis axis, int gamepad, float deadZone)
        {
            float value = GetAxis(axis, gamepad);
            if ((value < 0 && value > -deadZone) || (value > 0 && value < deadZone) || value == 0) return 0;
            return value > 0 ? 1 : -1;
        }

        static bool CheckForGamepadProfile (int gamepad, out GenericGamepadProfile profile) {
            profile = null;
            if (!IndexInRange(gamepad)) return false;
            profile = gamepadProfilesPerGamepad[gamepad];
            if (profile == null) {
                Debug.LogError("No Gamepad Profile supplied For Input Manager...");
                return false;
            }
            return true;
        }

        static bool ButtonQuery (GamepadButton button, int gamepad, System.Func<int, int, bool> callback, ButtonState dpadButtonStateCheck) {
            GenericGamepadProfile profile;
            if (!GamepadAvailable(gamepad, out profile)) return false;
            
            switch(button)
            {
                case GamepadButton.LeftStick:       return callback(profile.LeftStickButton, gamepad);
                case GamepadButton.RightStick:      return callback(profile.RightStickButton, gamepad);
                case GamepadButton.LeftBumper:      return callback(profile.LeftBumperButton, gamepad);
                case GamepadButton.RightBumper:     return callback(profile.RightBumperButton, gamepad);
                
                case GamepadButton.Back:            return callback(profile.BackButton, gamepad);
                case GamepadButton.Start:           return callback(profile.StartButton, gamepad);
                case GamepadButton.ActionBottom:    return callback(profile.ActionBottomButton, gamepad);
                case GamepadButton.ActionRight:     return callback(profile.ActionRightButton, gamepad);
                case GamepadButton.ActionLeft:      return callback(profile.ActionLeftButton, gamepad);
                case GamepadButton.ActionTop:       return callback(profile.ActionTopButton, gamepad);
                
                case GamepadButton.DPadUp:          return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadUpButton, gamepad) : m_dpadState[gamepad].Up == dpadButtonStateCheck;
                case GamepadButton.DPadDown:        return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadDownButton, gamepad) : m_dpadState[gamepad].Down == dpadButtonStateCheck;
                case GamepadButton.DPadLeft:        return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadLeftButton, gamepad) : m_dpadState[gamepad].Left == dpadButtonStateCheck;
                case GamepadButton.DPadRight:       return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadRightButton, gamepad) : m_dpadState[gamepad].Right == dpadButtonStateCheck;
                
                default:
                    return false;
            }
        }

        public static bool GetButton(GamepadButton button, int gamepad) { return ButtonQuery(button, gamepad, GetButton, ButtonState.Pressed); }
        public static bool GetButtonDown(GamepadButton button, int gamepad) { return ButtonQuery(button, gamepad, GetButtonDown, ButtonState.JustPressed); }
        public static bool GetButtonUp(GamepadButton button, int gamepad) { return ButtonQuery(button, gamepad, GetButtonUp, ButtonState.JustReleased); }

        static readonly int firstJoyButton = (int)KeyCode.Joystick1Button0;

        static bool GetButton(int button, int gamepad) { return CustomInput.GetKey((KeyCode)(firstJoyButton + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button)); }
        static bool GetButtonDown(int button, int gamepad) { return CustomInput.GetKeyDown((KeyCode)(firstJoyButton + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button)); }
        static bool GetButtonUp(int button, int gamepad) { return CustomInput.GetKeyUp((KeyCode)(firstJoyButton + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button)); }
    }
}