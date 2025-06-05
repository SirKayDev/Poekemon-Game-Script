using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    // Reference to the Settings Button
    public Button settingsButton;

    // References to the UI elements (Save, Quit)
    public GameObject saveButton;
    public GameObject quitButton;

    // References to Bag and Pokémon buttons
    public Button bagButton;
    public Button pokemonButton;

    private bool settingsVisible = false; // Track the toggle state

    void Start()
    {
        // Initially hide all UI elements
        saveButton.SetActive(false);
        quitButton.SetActive(false);

        // Disable Bag and Pokémon initially if needed
        UpdateBagAndPokemonButtons();

        // Set up the Settings Button click listener
        settingsButton.onClick.AddListener(ToggleSettings);
    }

    // Method to toggle the visibility of the Settings UI
    private void ToggleSettings()
    {
        settingsVisible = !settingsVisible;

        // Show or hide the elements based on the state
        saveButton.SetActive(settingsVisible);
        quitButton.SetActive(settingsVisible);

        // Freeze or unfreeze the game
        Time.timeScale = settingsVisible ? 0 : 1;

        // Update Bag and Pokémon buttons
        UpdateBagAndPokemonButtons();
    }

    // Method to enable or disable Bag and Pokémon buttons
    private void UpdateBagAndPokemonButtons()
    {
        bool shouldEnable = !settingsVisible && !saveButton.activeSelf && !quitButton.activeSelf;
        bagButton.interactable = shouldEnable;
        pokemonButton.interactable = shouldEnable;
    }

    // Method for Quit button to load Main Menu scene
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked! Attempting to load Main Menu...");

        try
        {
            Time.timeScale = 1; // Ensure time is resumed before loading a new scene
            SceneManager.LoadScene("Main Menu");
            Debug.Log("Scene load successful!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading scene: " + e.Message);
        }
    }
}

