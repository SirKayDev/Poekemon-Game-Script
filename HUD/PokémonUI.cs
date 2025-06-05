using UnityEngine;
using UnityEngine.UI;

public class PokemonUI : MonoBehaviour
{
    public GameObject pokemonStatusUI; // Pokémon Status UI
    public Button pokemonButton; // Button to open Pokémon Status
    public Button cancelButton; // Button to close Pokémon Status

    private bool isGameFrozen = false; // Track game freeze state

    void Start()
    {
        // Ensure Pokémon Status UI is hidden at start
        if (pokemonStatusUI != null)
            pokemonStatusUI.SetActive(false);

        // Assign button listeners
        if (pokemonButton != null)
            pokemonButton.onClick.AddListener(TogglePokemonStatus);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(ClosePokemonStatus);
    }

    void TogglePokemonStatus()
    {
        bool isVisible = pokemonStatusUI.activeSelf;
        pokemonStatusUI.SetActive(!isVisible);
        if (!isVisible) FreezeGame();
        else UnfreezeGame();
    }

    void ClosePokemonStatus()
    {
        pokemonStatusUI.SetActive(false);
        UnfreezeGame();
    }

    void FreezeGame()
    {
        if (!isGameFrozen)
        {
            Time.timeScale = 0f;
            isGameFrozen = true;
        }
    }

    void UnfreezeGame()
    {
        if (isGameFrozen)
        {
            Time.timeScale = 1f;
            isGameFrozen = false;
        }
    }
}




