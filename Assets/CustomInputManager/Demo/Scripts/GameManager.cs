
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Syd.UI;
using CustomInputManager;

public class GameManager : MonoBehaviour 
{
	public UIMenu pauseMenu;
	static GameManager m_instance;
	[HideInInspector] public bool isPaused;
	public static bool IsPaused { get { return m_instance.isPaused; } }
	
	public static void Pause()
	{
		if (!IsPaused) {

			Time.timeScale = 0.0f;
			m_instance.isPaused = true;
			m_instance.StartCoroutine(m_instance._Pause());
		}
			
	}
	IEnumerator _Pause () {
		yield return null;
		pauseMenu.Open();	
	}
	IEnumerator _Unpause () {
		yield return null;
		Time.timeScale = 1.0f;	
		pauseMenu.Close();
	}

	
	
	public static void UnPause()
	{
		if (IsPaused) {
			m_instance.isPaused = false;
			m_instance.StartCoroutine(m_instance._Unpause());
		}
	}

		int _PauseKey;
		void InitializeInputNameKeys () {
			_PauseKey = CustomInput.Name2Key("Pause");
		}

		void OnEnable () {
			InitializeInputNameKeys();
		}
	
		void Awake()
		{
			if(m_instance != null)
			{
				Destroy(this);
			}
			else
			{
				m_instance = this;
				SceneManager.sceneLoaded += HandleLevelWasLoaded;
				DontDestroyOnLoad(gameObject);
				pauseMenu.onClose += UnPause;
			}
		}

		void Update () {
			if (!isPaused) {
				if(CustomInput.GetButtonDown(_PauseKey))
				{
					Pause();
					pauseMenu.Open();
				}
			}
		}

		public void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}


	void HandleLevelWasLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		UnPause();
	}
	void OnDestroy()
	{
		SceneManager.sceneLoaded -= HandleLevelWasLoaded;
	}
	
}
