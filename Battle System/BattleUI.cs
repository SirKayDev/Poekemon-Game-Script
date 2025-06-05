using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleUI : MonoBehaviour
{
    public GameObject battleUI;
    public GameObject fightOptionsUI;
    public GameObject bagUI;
    public GameObject pokemonUI;
    public GameObject textBoxUI;

    public Image enemyImage;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyLevelText;
    public Slider enemyHPSlider;
    public TextMeshProUGUI enemyHPText;

    public Image allyImage;
    public TextMeshProUGUI allyNameText;
    public TextMeshProUGUI allyLevelText;
    public Slider allyHPSlider;
    public Slider allyExpSlider;
    public TextMeshProUGUI allyHPText;

    public Button fightButton;
    public Button bagButton;
    public Button pokemonButton;
    public Button runButton;

    public Button move1Button;
    public Button move2Button;
    public Button move3Button;
    public Button move4Button;

    public Button Enemymove1Button;
    public Button Enemymove2Button;
    public Button Enemymove3Button;
    public Button Enemymove4Button;

    public TextMeshProUGUI textBoxText;

    public GameObject bagHUDButton;
    public GameObject pokemonHUDButton;
    public GameObject settingsHUDButton;

    private bool isFightActive = false;

    public GameObject player;
    private bool playerFrozen = false;
    public bool IsBattleActive { get; private set; } = false;

    public bool isEnemyTrainerBattle = false; 

    public TextMeshProUGUI pokemonCheckText;
    public TextMeshProUGUI CheckPokemonDefeated;

    public GameObject BattleUiScene;
    public TextMeshProUGUI AllyAttack;
    private string lastAllyAttackText = "";
    private string lastAllyHPText = "";
    public TextMeshProUGUI EnemyAttack;


    public void SetPokemonCheck(string source)
    {
        if (pokemonCheckText != null)
            pokemonCheckText.text = $"Pokemon Check: {source}";
    }

    void Start()
    {
        battleUI.SetActive(false);
        fightOptionsUI.SetActive(false);
        move1Button.gameObject.SetActive(false);
        move2Button.gameObject.SetActive(false);
        move3Button.gameObject.SetActive(false);
        move4Button.gameObject.SetActive(false);
        textBoxUI.SetActive(false);
        enemyHPText.gameObject.SetActive(false);

        fightButton.onClick.AddListener(OnFightButtonClick);
        bagButton.onClick.AddListener(OnBagButtonClick);
        pokemonButton.onClick.AddListener(OnPokemonButtonClick);
        runButton.onClick.AddListener(OnRunButtonClick);

        move1Button.onClick.AddListener(() => OnMoveButtonClick(1));
        move2Button.onClick.AddListener(() => OnMoveButtonClick(2));
        move3Button.onClick.AddListener(() => OnMoveButtonClick(3));
        move4Button.onClick.AddListener(() => OnMoveButtonClick(4));

        ToggleHUDButtons(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleBattleUI();
            ToggleFightOptionsUI();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            DamageEnemyByTen();
        }

        // Apply damage when AllyAttack text is updated
        if (AllyAttack != null && !string.IsNullOrEmpty(AllyAttack.text))
        {
            // If AllyAttack text is different from the last time, apply damage
            if (AllyAttack.text != lastAllyAttackText)
            {
                DamageEnemyFromAllyAttackText();  // Apply damage
                lastAllyAttackText = AllyAttack.text;  // Update last known text
                StartCoroutine(ClearAllyAttackTextAfterDelay(0.5f));  // Clear after 0.5s
            }
        }

        // Check if Ally HP text changed
        if (allyHPText != null && allyHPText.text != lastAllyHPText)
        {
            UpdateAllyHPFromText();
            lastAllyHPText = allyHPText.text;
        }


    }

    public void DamageEnemyFromAllyAttackText()
    {
        if (!string.IsNullOrEmpty(AllyAttack.text))
        {
            if (int.TryParse(AllyAttack.text.Trim(), out int damageAmount))
            {
                string[] hpParts = enemyHPText.text.Split('/');
                if (hpParts.Length == 2 &&
                    float.TryParse(hpParts[0].Trim(), out float currentHP) &&
                    float.TryParse(hpParts[1].Trim(), out float maxHP))
                {
                    currentHP = Mathf.Max(currentHP - damageAmount, 0f);
                    UpdateEnemyHP(currentHP, maxHP);
                }
                else
                {
                    Debug.LogWarning("Could not parse enemy HP.");
                }
            }
            else
            {
                Debug.LogWarning("Could not parse damage amount from AllyAttack text.");
            }
        }
    }


    public IEnumerator ClearAllyAttackTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AllyAttack.text = "";  // Clear the attack text after delay

        // We want to allow the next update to work, even if it's the same number again
        lastAllyAttackText = "";  // Reset the last attack text so the next one can trigger
    }



    private void DamageEnemyByTen()
    {
        // Parse the current and max HP from the UI text
        string[] hpParts = enemyHPText.text.Split('/');
        if (hpParts.Length == 2 &&
            float.TryParse(hpParts[0].Trim(), out float currentHP) &&
            float.TryParse(hpParts[1].Trim(), out float maxHP))
        {
            // Subtract 10 HP but clamp it at 0
            currentHP = Mathf.Max(currentHP - 10f, 0f);
            UpdateEnemyHP(currentHP, maxHP);
        }
        else
        {
            Debug.LogWarning("Could not parse enemy HP from UI.");
        }
    }

    public void UpdateAllyHPFromText()
    {
        if (allyHPText != null)
        {
            string[] hpParts = allyHPText.text.Split('/');
            if (hpParts.Length == 2 &&
                float.TryParse(hpParts[0].Trim(), out float currentHP) &&
                float.TryParse(hpParts[1].Trim(), out float maxHP))
            {
                allyHPSlider.maxValue = maxHP;
                allyHPSlider.value = currentHP;
            }
            else
            {
                Debug.LogWarning("Could not parse ally HP text.");
            }
        }
    }


    private void ToggleBattleUI()
    {
        battleUI.SetActive(!battleUI.activeSelf);
        ToggleHUDButtons(battleUI.activeSelf);

        if (battleUI.activeSelf)
            FreezePlayer();
        else
            UnfreezePlayer();
    }

    private void FreezePlayer()
    {
        playerFrozen = true;
        if (player.TryGetComponent<CharacterController>(out var cc))
            cc.enabled = false;
        else if (player.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
    }

    public void UnfreezePlayer()
    {
        playerFrozen = false;
        if (player.TryGetComponent<CharacterController>(out var cc))
            cc.enabled = true;
        else if (player.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;
    }

    private void ToggleFightOptionsUI()
    {
        if (battleUI.activeSelf)
        {
            fightOptionsUI.SetActive(true);
            move1Button.gameObject.SetActive(true);
            move2Button.gameObject.SetActive(true);
            move3Button.gameObject.SetActive(true);
            move4Button.gameObject.SetActive(true);
            textBoxUI.SetActive(true);
        }
    }

    public void ToggleHUDButtons(bool isBattleActive)
    {
        bagHUDButton.SetActive(!isBattleActive);
        pokemonHUDButton.SetActive(!isBattleActive);
        settingsHUDButton.SetActive(!isBattleActive);

        runButton.gameObject.SetActive(isBattleActive);
        bagButton.gameObject.SetActive(isBattleActive);
    }

    public void SetupEnemy(string name, Sprite sprite, int level, int baseHP, float currentHP)
    {
        // Calculate HP using the formula
        float maxHP = ((2 * baseHP * level) / 100f) + level + 10f;
        currentHP = Mathf.Min(currentHP, maxHP);

        // Update enemy details
        enemyNameText.text = name;
        enemyLevelText.text = "Lv. " + level;
        enemyImage.sprite = sprite;

        // Calculate the HP percentage
        float hpPercentage = (currentHP / maxHP) * 100f;

        // Update the enemy HP slider
        enemyHPSlider.maxValue = 100f;  // Maximum value in the slider represents 100%
        enemyHPSlider.value = hpPercentage;

        enemyHPText.text = $"{(int)currentHP} / {(int)maxHP}";

    }

    public static float CalculateMaxHP(int baseHP, int level)
    {
        return ((2 * baseHP * level) / 100f) + level + 10f;
    }


    private IEnumerator ShowPokemonUIForSeconds(float duration)
    {
        if (pokemonUI != null && BattleUiScene != null)
        {
            // Save original parent and index
            Transform originalParent = pokemonUI.transform.parent;
            int originalIndex = pokemonUI.transform.GetSiblingIndex();

            // Re-parent to ensure it's under BattleUiScene (if not already)
            pokemonUI.transform.SetParent(BattleUiScene.transform, worldPositionStays: false);

            // Send behind other UI temporarily
            pokemonUI.transform.SetSiblingIndex(0);
            pokemonUI.SetActive(true);

            yield return new WaitForSeconds(duration);

            // Restore original parent and index
            pokemonUI.transform.SetParent(originalParent, worldPositionStays: false);
            pokemonUI.transform.SetSiblingIndex(originalIndex);

            pokemonUI.SetActive(false);
        }
    }




    public void UpdateEnemyHP(float currentHP, float maxHP)
    {
        float hpPercentage = (currentHP / maxHP) * 100f;
        Debug.Log($"Updating enemy HP slider: {currentHP} / {maxHP} = {hpPercentage}%");
        enemyHPSlider.value = hpPercentage;

        enemyHPText.text = $"{(int)currentHP} / {(int)maxHP}";

        if (currentHP <= 0)
        {
            Debug.Log("Enemy HP is 0 or less.");
            StartCoroutine(ShowPokemonUIForSeconds(0.05f));

            // Check if it's a wild Pokémon
            if (pokemonCheckText != null && pokemonCheckText.text.Contains("Wild Pokémon"))
            {
                Debug.Log("Wild Pokémon defeated! Waiting to end battle...");
                StartCoroutine(EndBattleAfterDelay(1f)); // Wait 2 seconds before ending battle
            }
            else
            {
                Debug.Log("Enemy is not a wild Pokémon. Battle continues (Trainer battle).");
            }

            // Update the TextMeshProUGUI to show the enemy has been defeated
            if (CheckPokemonDefeated != null)
            {
                CheckPokemonDefeated.text = $"{enemyNameText.text}"; // Show defeated message
            }
        }

    }





    public void SetupAlly(string name, Sprite sprite, int level, float maxHP, float currentHP, float maxExp, float currentExp)
    {
        allyNameText.text = name; 
        allyImage.sprite = sprite;
        allyHPSlider.maxValue = maxHP;
        allyHPSlider.value = currentHP;
        allyExpSlider.maxValue = maxExp;
        allyExpSlider.value = currentExp;
        allyHPText.text = currentHP + " / " + maxHP;
        allyLevelText.text = "Lv." + level;

        UpdateTextBoxWithAllyName();

    }



    private void OnFightButtonClick()
    {
        if (isFightActive)
        {
            fightOptionsUI.SetActive(false);
            textBoxUI.SetActive(true);
            isFightActive = false;
        }
        else
        {
            if (textBoxUI.activeSelf)
            {
                fightOptionsUI.SetActive(true);
                move1Button.gameObject.SetActive(true);
                move2Button.gameObject.SetActive(true);
                move3Button.gameObject.SetActive(true);
                move4Button.gameObject.SetActive(true);
                textBoxUI.SetActive(false);
            }
            else
            {
                textBoxUI.SetActive(true);
            }
            isFightActive = !isFightActive;
        }
    }

    private void OnBagButtonClick()
    {
        bagUI.SetActive(true);
        pokemonUI.SetActive(false);
    }

    private void OnPokemonButtonClick()
    {
        bagUI.SetActive(false);
        pokemonUI.SetActive(true);
    }

    private void OnRunButtonClick()
    {
        // Prevent running away in enemy trainer battles
        if (isEnemyTrainerBattle)
        {
            // Optionally show a message or feedback here that running away isn't allowed
            Debug.Log("You can't run from a Trainer Battle!");
            return;
        }

        // Reset enemy information
        enemyNameText.text = "";
        enemyLevelText.text = "";

        // Hide Battle UI and reset any battle-related UI
        battleUI.SetActive(false);
        ToggleHUDButtons(false);
        UnfreezePlayer();
        IsBattleActive = false;

        // Reset Pokemon Check text
        if (pokemonCheckText != null)
        {
            pokemonCheckText.text = $"Pokemon Check: "; // Reset or clear the text
        }
    }




    private void OnMoveButtonClick(int moveNumber)
    {
        Debug.Log("Move " + moveNumber + " selected!");
        fightOptionsUI.SetActive(false);
        isFightActive = false;
    }

    public void SetTextBox(string text)
    {
        textBoxText.text = text;
    }

    public void ActivateBattle(bool againstEnemyTrainer = false)
    {
        battleUI.SetActive(true);
        ToggleFightOptionsUI();
        FreezePlayer();  // Freeze player movement
        ToggleHUDButtons(true);  // Ensure battle-related UI is hidden here
        IsBattleActive = true;

        isEnemyTrainerBattle = againstEnemyTrainer;  // Set the flag based on battle type

        if (isEnemyTrainerBattle)
        {
            runButton.interactable = false; // Disable running away during a trainer battle
        }
        else
        {
            runButton.interactable = true;
        }
    }


    public void EndBattle()
{   
    IsBattleActive = false;
    battleUI.SetActive(false);
    ToggleHUDButtons(false); // Hide battle UI-related buttons
    UnfreezePlayer();

        // Reset Pokemon Check text
        if (pokemonCheckText != null)
        {
            pokemonCheckText.text = $"Pokemon Check: "; // Reset or clear the text
        }

    }



    public void UpdateTextBoxWithAllyName()
    {
        string allyName = allyNameText.text; // Get the ally's name from the allyNameText component
        textBoxText.text = "What will " + allyName + " do?"; // Update the text box
    }

    public void ShowTemporaryMessage(string message, float delaySeconds = 2.5f)
    {
        StartCoroutine(ShowMessageAndReset(message, delaySeconds));
    }

    private IEnumerator ShowMessageAndReset(string message, float delay)
    {
        SetTextBox(message); // Show custom message
        yield return new WaitForSeconds(delay);
        UpdateTextBoxWithAllyName(); // Revert to default message
    }

    private IEnumerator EndBattleAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        EndBattle();
    }


}

