using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField]
	private string sceneName = "";

	private void Awake()
	{
        // Load the scene if it wasn't already loaded.
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
	}
}