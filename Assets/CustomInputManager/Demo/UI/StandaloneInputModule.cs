
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using CustomInputManager;

namespace Syd.UI
{
    // [AddComponentMenu("CustomInputModule/Standalone Input Module")]
//     public class StandaloneInputModule : PointerInputModule
//     {
//         float GetAxisRaw (string axis) {
//             return InputManager.GetAxisRaw(axis);
//         }
//         bool GetButtonDown (string button) {
//             return InputManager.GetButtonDown(button);
//         }
//         bool GetMouseButtonDown (int button) {
//             return InputManager.GetMouseButtonDown(button);
//         }
//         Vector2 mousePosition {
//             get {
//                 return InputManager.mousePosition;
//             }
//         }



//         [HideInInspector] public GameObject currentDefaultSelection;
//         bool overrideInput;
    
//         float m_NextAction;

//         private Vector2 m_LastMousePosition, m_MousePosition;
        
//         [SerializeField] private string m_HorizontalAxis = "Horizontal";
//         [SerializeField] private string m_VerticalAxis = "Vertical";

//         // [SerializeField] private string m_UpButton = "UI_Up";
// 		// [SerializeField] private string m_DownButton = "UI_Down";
// 		// [SerializeField] private string m_LeftButton = "UI_Left";
// 		// [SerializeField] private string m_RightButton = "UI_Right";
    
//         [SerializeField] private string m_SubmitButton = "Submit";
//         [SerializeField] private string m_CancelButton = "Cancel";

//         [SerializeField] private float m_InputActionsPerSecond = 10;



        



        
//     }




    
// }


// using System;

// namespace UnityEngine.EventSystems
// {
    [AddComponentMenu("Event/Standalone Input Module")]
    public class StandaloneInputModule : PointerInputModule
    {
        float GetAxisRaw (int axis) {
            return CustomInput.GetAxisRaw(axis);
        }
        bool GetButtonDown (int button) {
            return CustomInput.GetButtonDown(button);
        }
        bool GetMouseButtonDown (int button) {
            return CustomInput.GetMouseButtonDown(button);
        }

        bool overrideInput;
    
        private float m_NextAction;
        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;

        int _HorizontalAxis, _VerticalAxis, _SubmitButton, _CancelButton;
        void InitializeInputNameKeys () {
            _HorizontalAxis = CustomInput.Name2Key(m_HorizontalAxis);
            _VerticalAxis = CustomInput.Name2Key(m_VerticalAxis);
            _SubmitButton = CustomInput.Name2Key(m_SubmitButton);
            _CancelButton = CustomInput.Name2Key(m_CancelButton);
        }

        protected override void OnEnable() {
            base.OnEnable();
            InitializeInputNameKeys();
        }

        

        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";

        /// <summary>
        /// Name of the vertical axis for movement (if axis events are used).
        /// </summary>
        [SerializeField]
        private string m_VerticalAxis = "Vertical";

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private string m_SubmitButton = "Submit";

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private string m_CancelButton = "Cancel";

        [SerializeField]
        private float m_InputActionsPerSecond = 10;

        
        public override void UpdateModule()
        {
            m_LastMousePosition = m_MousePosition;
            m_MousePosition = CustomInput.mousePosition;
        }

        public override bool IsModuleSupported()
        {
            return true;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            if (overrideInput) {
                return false;
            }
            

            var shouldActivate = GetButtonDown(_SubmitButton);
            shouldActivate |= GetButtonDown(_CancelButton);

            // shouldActivate |= GetButtonDown(_UpButton);
			// shouldActivate |= GetButtonDown(_DownButton);
			// shouldActivate |= GetButtonDown(_LeftButton);
			// shouldActivate |= GetButtonDown(_RightButton);
	
            shouldActivate |= !Mathf.Approximately(GetAxisRaw(_HorizontalAxis), 0.0f);
            shouldActivate |= !Mathf.Approximately(GetAxisRaw(_VerticalAxis), 0.0f);
            shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
            shouldActivate |= GetMouseButtonDown(0);
            return shouldActivate;
        }


        public override void ActivateModule()
        {
            base.ActivateModule();
            m_MousePosition = CustomInput.mousePosition;
            m_LastMousePosition = CustomInput.mousePosition;

            var toSelect = eventSystem.currentSelectedGameObject;
            
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            ProcessMouseEvent();
        }


        public void OverrideInput (bool overrriden) {
            if (overrriden) {
                overrideInput = true;
            }
            else {
                StartCoroutine(UnOverrideInput());
            }
        }

        System.Collections.IEnumerator UnOverrideInput () {
            yield return new WaitForSecondsRealtime(1f/m_InputActionsPerSecond);
            overrideInput = false;
        }

        private bool SendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            if (overrideInput) {
                return false;
            }
            
            var data = GetBaseEventData();
            if (GetButtonDown(_SubmitButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

            if (GetButtonDown(_CancelButton)) {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            }
            return data.used;
        }


        private bool AllowMoveEventProcessing(float time)
        {
            if (overrideInput) {
                return false;
            }
            
            bool allow = GetButtonDown(_HorizontalAxis);
            allow |= GetButtonDown(_VerticalAxis);

            //  allow |= GetButtonDown(_UpButton);
            // 	allow |= GetButtonDown(_DownButton);
            // 	allow |= GetButtonDown(_LeftButton);
            // 	allow |= GetButtonDown(_RightButton);
	
            allow |= (time > m_NextAction);
            return allow;
        }
        [HideInInspector] public GameObject currentDefaultSelection;

        
        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = GetAxisRaw(_HorizontalAxis);
            move.y = GetAxisRaw(_VerticalAxis);

            // if (GetButtonDown(_HorizontalAxis))
            {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            // if (GetButtonDown(_VerticalAxis))
            {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            //     if(GetButtonDown(m_LeftButton)) move.x = -1.0f;
            //     if(GetButtonDown(m_RightButton)) move.x = 1.0f;
            //     if(GetButtonDown(m_UpButton))  move.y = 1.0f;
            //     if(GetButtonDown(m_DownButton)) move.y = -1.0f;
            
            return move;
        }

        /// <summary>
        /// Process keyboard events.
        /// </summary>
        private bool SendMoveEventToSelectedObject()
        {
            if (overrideInput) {
                return false;
            }
            
            float time = Time.unscaledTime;

            if (!AllowMoveEventProcessing(time))
                return false;

            Vector2 movement = GetRawMoveVector();
            // Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
            if (!Mathf.Approximately(axisEventData.moveVector.x, 0f) || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
            {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            }
            m_NextAction = time + 1f / m_InputActionsPerSecond;
            return axisEventData.used;
        }

        public void ActionTime () {
            m_NextAction = Time.unscaledDeltaTime + 1f / m_InputActionsPerSecond;
        }

        /// <summary>
        /// Process all mouse events.
        /// </summary>
        private void ProcessMouseEvent()
        {
            var mouseData = GetMousePointerEventData();

            var pressed = mouseData.AnyPressesThisFrame();
            var released = mouseData.AnyReleasesThisFrame();

            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            if (!UseMouse(pressed, released, leftButtonData.buttonData))
                return;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
        {
            if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
                return true;

            return false;
        }

        private bool SendUpdateEventToSelectedObject()
        {
            if (overrideInput) {
                return false;
            }
            

            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        private void ProcessMousePress(MouseButtonEventData data)
        {
            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (data.PressedThisFrame())
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (data.ReleasedThisFrame())
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over somethign that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }
            }
        }
    }
}