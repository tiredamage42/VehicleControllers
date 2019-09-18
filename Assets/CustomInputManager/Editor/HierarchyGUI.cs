using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEditor;

namespace CustomInputManager.Editor {


    public class HieararchyGUIElement {
        public string name;
        public List<HieararchyGUIElement> subElements;
        public Action<Rect> createContextMenu;
        public HieararchyGUIElement (string name, List<HieararchyGUIElement> subElements, Action<Rect> createContextMenu) {
            this.name = name;
            this.subElements = subElements;
            this.createContextMenu = createContextMenu;
        }
    }
    public class HierarchyGUI : MonoBehaviour
    {

        public static bool Draw (float offset, bool expandable, List<HieararchyGUIElement> elements, Action<Rect> drawSelected, Action<Rect> drawFileMenu) {
            UpdateHierarchyPanelWidth(offset);
			DrawMainPanel(offset, drawSelected);
			bool clicked = DrawHierarchyPanel(offset, expandable, elements);
            DrawMainToolbar(drawFileMenu, offset);
            return clicked;
        }
			

        const float HIERARCHY_INDENT_SIZE = 30.0f;
        const float HIERARCHY_ITEM_HEIGHT = 18.0f;
		

        static float CalculateHeight(List<HieararchyGUIElement> elements, bool expandable) {
            float h = 0;
            for (int i = 0; i < elements.Count; i++) {
                h += HIERARCHY_ITEM_HEIGHT;
                if (expandable && Expanded(i)) {
                    h += elements[i].subElements.Count * HIERARCHY_ITEM_HEIGHT;
                }
            }
            return h;
        }
		
        static Vector2 m_hierarchyScrollPos = Vector2.zero;
		
        public static bool DrawHierarchyPanel(float yOffset, bool expandable, List<HieararchyGUIElement> elements)
		{

            CreateHighlightTexture();
            CreateWhiteLabelGUI();

            bool clicked  = false;
            float windowHeight = InputManagerWindow.pos.height - yOffset;

            float viewHeight = CalculateHeight(elements, expandable);
		
			Rect screenRect = new Rect(0.0f, TOOLBAR_HEIGHT - 5.0f, m_hierarchyPanelWidth, windowHeight - TOOLBAR_HEIGHT + 10.0f);
			
            screenRect.y += yOffset;
			
			Rect scrollView = new Rect(screenRect.x, screenRect.y + 5.0f, screenRect.width, windowHeight - screenRect.y);
			Rect viewRect = new Rect(0.0f, 0.0f, scrollView.width, viewHeight);
			float itemPosY = 0.0f;
			
			GUI.Box(screenRect, "");
			m_hierarchyScrollPos = GUI.BeginScrollView(scrollView, m_hierarchyScrollPos, viewRect);
			
			for(int i = 0; i < elements.Count; i++)
			{
				Rect csRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
				if (DrawBaseHiearchyItem(csRect, i, expandable, elements[i])) {
                    clicked = true;
                }
				itemPosY += HIERARCHY_ITEM_HEIGHT;

                if (expandable && Expanded(i))
				{
					for(int j = 0; j < elements[i].subElements.Count; j++)
					{
						Rect iaRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
						if (DrawSubElement(iaRect, i, j, elements[i].subElements[j])) {
                            clicked = true;
                        }
						itemPosY += HIERARCHY_ITEM_HEIGHT;
					}
				}
			}
			GUI.EndScrollView();

            return clicked;
		}


        static bool[] schemesExpanded;
		static bool Expanded (int i) {
			if (schemesExpanded == null) {
				schemesExpanded = new bool[0];
			}
			if (schemesExpanded.Length <= i) {
				System.Array.Resize(ref schemesExpanded, i+1);
			}
			return !schemesExpanded[i]; // backwards so it starts out open
		}

        const float MENU_WIDTH = 100.0f;
		
        static bool DrawBaseHiearchyItem(Rect position, int index, bool expandable, HieararchyGUIElement element)
		{
            bool clicked = false;
			
            Rect foldoutRect = new Rect(5.0f, 1.0f, 10, position.height - 1.0f);
			Rect nameRect = new Rect(expandable ? foldoutRect.xMax + 5.0f : 5.0f, 1.0f, position.width - (expandable ? (foldoutRect.xMax + 5.0f) : 0), position.height - 1.0f);

			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(position.Contains(Event.current.mousePosition))
				{
					InputManagerWindow.ResetSelections();
                    InputManagerWindow.SetSelection(0, index);
					
                    clicked = true;

					GUI.FocusControl(null);
					InputManagerWindow.instance.Repaint();

					if(Event.current.button == 1)
					{
						element.createContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
					}
				}
			}

			GUI.BeginGroup(position);

            if (InputManagerWindow.GetSelection(0) >= 0 && InputManagerWindow.GetSelection(1) < 0 && InputManagerWindow.GetSelection(0) == index)
			{
				GUI.DrawTexture(new Rect(0, 0, position.width, position.height), m_highlightTexture, ScaleMode.StretchToFill);
				
                if (expandable) schemesExpanded[index] = !EditorGUI.Foldout(foldoutRect, Expanded(index), GUIContent.none);
                EditorGUI.LabelField(nameRect, element.name, m_whiteLabel);
			}
			else
			{
				if (expandable) schemesExpanded[index] = !EditorGUI.Foldout(foldoutRect, Expanded(index), GUIContent.none);
                EditorGUI.LabelField(nameRect, element.name);
			}
			GUI.EndGroup();
            return clicked;
		}


        static bool DrawSubElement(Rect position, int i, int j, HieararchyGUIElement subElement)
		{
            bool clicked = false;
			Rect nameRect = new Rect(HIERARCHY_INDENT_SIZE, 1.0f, position.width - HIERARCHY_INDENT_SIZE, position.height - 1.0f);

			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(position.Contains(Event.current.mousePosition))
				{
					InputManagerWindow.ResetSelections();

                    InputManagerWindow.SetSelection(0, i);
					InputManagerWindow.SetSelection(1, j);
					
                    clicked = true;
					Event.current.Use();
					GUI.FocusControl(null);
					InputManagerWindow.instance.Repaint();

					if(Event.current.button == 1)
                    {
                        subElement.createContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
					}
				}
			}

			GUI.BeginGroup(position);
			if(InputManagerWindow.GetSelection(1) >= 0 && InputManagerWindow.GetSelection(0) == i && InputManagerWindow.GetSelection(1) == j)
			{
				GUI.DrawTexture(new Rect(0, 0, position.width, position.height), m_highlightTexture, ScaleMode.StretchToFill);
				EditorGUI.LabelField(nameRect, subElement.name, m_whiteLabel);
			}
			else
			{
				EditorGUI.LabelField(nameRect, subElement.name);
			}
			GUI.EndGroup();

            return clicked;
		}

        const float TOOLBAR_HEIGHT = 18.0f;
		


        public static void DrawMainToolbar(Action<Rect> drawFileMenu, float offset)
		{
			Rect screenRect = new Rect(0.0f, 0.0f, InputManagerWindow.width, TOOLBAR_HEIGHT);
			screenRect.y += offset;
			Rect fileMenuRect = new Rect(0.0f, 0.0f, MENU_WIDTH, screenRect.height);			
			Rect paddingLabelRect = new Rect(fileMenuRect.xMax, 0.0f, screenRect.width - MENU_WIDTH, screenRect.height);
			
			GUI.BeginGroup(screenRect);
			DrawEditMenu(fileMenuRect, drawFileMenu);
			EditorGUI.LabelField(paddingLabelRect, "", EditorStyles.toolbarButton);
			
			GUI.EndGroup();
		}

        static void DrawEditMenu(Rect screenRect, Action<Rect> drawFileMenu)
		{
			EditorGUI.LabelField(screenRect, "Options", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && screenRect.Contains(Event.current.mousePosition))
			{
				drawFileMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
		}

        public static void DrawMainPanel(float yOffset, Action<Rect> drawSelected)
		{
			Rect position = new Rect(m_hierarchyPanelWidth, TOOLBAR_HEIGHT, InputManagerWindow.width - m_hierarchyPanelWidth, (InputManagerWindow.pos.height) - TOOLBAR_HEIGHT);
			
            position.y += yOffset;
            position.height -= yOffset;

            drawSelected(position);
			
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(position.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					GUI.FocusControl(null);
					InputManagerWindow.instance.Repaint();
				}
			}
		}


        static float m_hierarchyPanelWidth = MIN_HIERARCHY_PANEL_WIDTH;
        const float MIN_HIERARCHY_PANEL_WIDTH = 150.0f;
        static bool m_isResizingHierarchy = false;
        const float MAX_CURSOR_RECT_WIDTH = 50.0f;
        const float MIN_CURSOR_RECT_WIDTH = 10.0f;
        public static readonly Color32 HIGHLIGHT_COLOR = new Color32(62, 125, 231, 200);
        

        static Texture2D m_highlightTexture;
        static void CreateHighlightTexture()
		{
            if (m_highlightTexture == null) {
                m_highlightTexture = new Texture2D(1, 1);
                m_highlightTexture.SetPixel(0, 0, HIGHLIGHT_COLOR);
                m_highlightTexture.Apply();
            }
		}
        static void DisposeHighlightTexture () {
            if (m_highlightTexture != null) {
                Texture2D.DestroyImmediate(m_highlightTexture);
                m_highlightTexture = null;	
            }
        }

        public static void Dispose () {
            DisposeHighlightTexture();
        }
        

        static GUIStyle m_whiteLabel;
        static void CreateWhiteLabelGUI () {
            if(m_whiteLabel == null) {
				m_whiteLabel = new GUIStyle(EditorStyles.label);
				m_whiteLabel.normal.textColor = Color.white;
			}
        }

		
            
        public static void UpdateHierarchyPanelWidth(float offset)
        {
            float cursorRectWidth = m_isResizingHierarchy ? MAX_CURSOR_RECT_WIDTH : MIN_CURSOR_RECT_WIDTH;
            Rect cursorRect = new Rect(m_hierarchyPanelWidth - cursorRectWidth / 2, TOOLBAR_HEIGHT, cursorRectWidth, InputManagerWindow.pos.height - TOOLBAR_HEIGHT);
            cursorRect.y += offset;
            
            Rect resizeRect = new Rect(m_hierarchyPanelWidth - MIN_CURSOR_RECT_WIDTH / 2, 0.0f, MIN_CURSOR_RECT_WIDTH, InputManagerWindow.pos.height);
            resizeRect.y += offset;

            EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.ResizeHorizontal);
            switch(Event.current.type)
            {
            case EventType.MouseDown:
                if(Event.current.button == 0 && resizeRect.Contains(Event.current.mousePosition))
                {
                    m_isResizingHierarchy = true;
                    Event.current.Use();
                }
                break;
            case EventType.MouseUp:
                if(Event.current.button == 0 && m_isResizingHierarchy)
                {
                    m_isResizingHierarchy = false;
                    Event.current.Use();
                }
                break;
            case EventType.MouseDrag:
                if(m_isResizingHierarchy)
                {   
                    m_hierarchyPanelWidth = Mathf.Clamp(m_hierarchyPanelWidth + Event.current.delta.x, MIN_HIERARCHY_PANEL_WIDTH, InputManagerWindow.width / 2);
                    Event.current.Use();
                    InputManagerWindow.instance.Repaint();
                }
                break;
            default:
                break;
            }
        }
        
    }
}


