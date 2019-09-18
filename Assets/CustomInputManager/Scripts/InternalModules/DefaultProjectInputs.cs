using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
    Handle the developer specified project inputs
    
    (default game input schemes)
*/
namespace CustomInputManager.Internal {
    public static class DefaultProjectInputs
    {
        const string XMLName = "DefaultProjectInputsXML";
        static TextAsset _xmlAsset;
        static TextAsset xmlAsset {
            get {
                if (_xmlAsset == null) {
                    InitializeXMLAsset();
                }
                return _xmlAsset;
            }
        }

   
        public static ControlScheme LoadDefaultScheme (string controlSchemeName) {
            ControlScheme r = null;
            using(StringReader reader = new StringReader(xmlAsset.text)) {
				r = new InputLoaderXML(reader).Load(controlSchemeName);
			}
            return r;
        }
    
        public static List<ControlScheme> LoadDefaultSchemes () {
            if (xmlAsset == null)
                return null;
            
            List<ControlScheme> r = null;
            using(StringReader reader = new StringReader(xmlAsset.text)) {
                r = new InputLoaderXML(reader).Load();
            }
            return r;
        }

        
        static void InitializeXMLAsset () {
            if (_xmlAsset != null)
                return;
            
            _xmlAsset = LoadXMLAsset();

            #if UNITY_EDITOR
            if (_xmlAsset == null) {
                if (Application.isPlaying) {
                    Debug.LogError("Default Project Inputs XML not found! Open up the Custom Input Manager Window in Editor Mode to initialize it.");
                }
                else {
                    _xmlAsset = CreateNewXML ();
                }
            }
            #endif   
        }
                    
        static TextAsset LoadXMLAsset () {
            return Resources.Load<TextAsset>(InputManager.resourcesFolder + XMLName);
        }


#if UNITY_EDITOR
        static TextAsset CreateNewXML () {
            SaveSchemesAsDefault("Creating", new List<ControlScheme>());
            return LoadXMLAsset();   
        }
        public static void SaveSchemesAsDefault (string msg, List<ControlScheme> controlSchemes) {
            if (InputManager.instance == null)
                return;

            if (controlSchemes == null)
                return;
            
            string path = InputManager.fullResourcesDirectory + XMLName + ".xml";
            
            Debug.Log(msg + " Default Project Inputs XML at path :: " + path);
            new InputSaverXML(path).Save(controlSchemes);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
#endif

    }
}
