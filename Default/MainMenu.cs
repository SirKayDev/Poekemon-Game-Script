using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Function to load a new scene (teleport to the scene)
    public void PlayGame(string sceneName)
    {
        // Load the scene by its name
        SceneManager.LoadScene("Main Game");
    }

    // Function to quit the game
    public void QuitGame()
    {
#if UNITY_EDITOR
        // If running in the editor, stop the play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // If running as a built game, quit the application
            Application.Quit();
#endif
    }
}

