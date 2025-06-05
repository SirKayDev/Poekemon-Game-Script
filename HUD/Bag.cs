using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bag : MonoBehaviour
{
    public static Bag Instance;

    public Button bagButton;
    public Button cancelButton;
    public GameObject bagUI;

    public TextMeshProUGUI pokeBallText;
    public TextMeshProUGUI greatBallText;
    public TextMeshProUGUI potionText;
    public TextMeshProUGUI superPotionText;

    public TextMeshProUGUI playerMoneyText;

    public GameObject boulderBadge;
    public GameObject cascadeBadge;

    public Button pokeBallButton;
    public Button greatBallButton;
    public Button potionButton;
    public Button superPotionButton;


    public GameObject pokemonStatusUI;
    private enum PotionType { None, Potion, SuperPotion }
    private PotionType selectedPotion = PotionType.None;

    public BattleUI battleUI;
    public TextMeshProUGUI catchText;
    public TextMeshProUGUI levelText;
    public PokemonSlotsManager pokemonSlotsManager;

    private int GetItemCount(TextMeshProUGUI itemText)
    {
        if (itemText.text.StartsWith("x") && int.TryParse(itemText.text.Substring(1), out int count))
            return count;
        return 0;
    }



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        bagUI.SetActive(false);
        Time.timeScale = 1;

        boulderBadge.SetActive(false);
        cascadeBadge.SetActive(false);

        bagButton.onClick.AddListener(OpenBag);
        cancelButton.onClick.AddListener(CloseBag);

        potionButton.onClick.AddListener(() => OnPotionButton(PotionType.Potion));
        superPotionButton.onClick.AddListener(() => OnPotionButton(PotionType.SuperPotion));

        pokeBallButton.onClick.AddListener(() => UseBall("Poké Ball")); 
        greatBallButton.onClick.AddListener(() => UseBall("Great Ball"));

        cancelButton.onClick.AddListener(() =>
        {
            CloseBag();
            CancelPotionUse();
        });

        AddItem("Poké Ball", 5); 
        AddItem("Potion", 1);
        //AddItem("Great Ball", 100);
    }

    void UseBall(string ballName)
    {
        int itemCount = (ballName == "Poké Ball") ? GetItemCount(pokeBallText) : GetItemCount(greatBallText);
        if (itemCount <= 0) return;

        if (!battleUI.pokemonCheckText.text.Contains("Wild Pokémon"))
            return;

        if (IsPartyFull())
        {
            catchText.text = "You can't catch more Pokémon!";
            return;
        }

        DecrementItem(ballName == "Poké Ball" ? pokeBallText : greatBallText);
        bagUI.SetActive(false);

        // Extract HP values from the text (e.g. "35 / 100")
        string[] hpParts = battleUI.enemyHPText.text.Split('/');
        if (hpParts.Length != 2) return;

        if (!float.TryParse(hpParts[0].Trim(), out float currentHP)) return;
        if (!float.TryParse(hpParts[1].Trim(), out float maxHP)) return;

        string pokemonName = battleUI.enemyNameText.text;
        int baseRate = GetBaseRate(pokemonName);

        float ballModifier = ballName == "Great Ball" ? 1.5f : 1.0f;

        // Calculate catch chance
        float catchChance = (maxHP / Mathf.Max(currentHP, 1)) * baseRate * ballModifier / 100f;
        catchChance = Mathf.Clamp(catchChance, 0f, 100f);

        // Roll random chance
        float roll = Random.Range(0f, 100f);
        bool isCaught = roll <= catchChance;

        if (isCaught)
        {
            // Parse the level text (e.g., "Lv. 5") and extract the number  
            string levelStr = battleUI.enemyLevelText.text.Replace("Lv.", "").Trim();
            if (!int.TryParse(levelStr, out int level)) level = 1;

            int baseHP = GetBaseHP(pokemonName);

            // Remove HP and Base HP from the catchText and use levelTextB
            catchText.text = $"{pokemonName}";
            levelText.text = $"{level}"; // Display level in a separate TextMeshPro component

            // Convert levelText to int
            int.TryParse(levelText.text, out int caughtLevel);

            // Get sprite from PokemonSlotsManager
            Sprite pokemonSprite = pokemonSlotsManager.GetSpriteByName(pokemonName);
            int baseHPOfCaughtPokemon = Bag.Instance.GetBaseHP(pokemonName);

            // Find the first empty slot
            for (int i = 0; i < 6; i++)
            {
                if (pokemonSlotsManager.IsSlotEmpty(i))
                {
                    pokemonSlotsManager.AddPokemonToSlot(pokemonName, pokemonSprite, baseHP, i, caughtLevel);
                    break;
                }
            }

            // End the battle when the Pokémon is caught
            battleUI.EndBattle(); // End battle when Pokémon is caught
        }
        else
        {
            catchText.text = $"{pokemonName} broke free!";
        }
    }

    private bool IsPartyFull()
    {
        for (int i = 0; i < 6; i++)
        {
            if (pokemonSlotsManager.IsSlotEmpty(i))
                return false;
        }
        return true;
    }



    // Helper method to get the base rate for each Pokémon
    int GetBaseRate(string pokemonName)
    {
        switch (pokemonName)
        {
            case "Pidgey":
            case "Rattata":
            case "Spearow":
            case "Caterpie":
            case "Weedle":
            case "Jigglypuff":
            case "Zubat":
            case "Geodude":
            case "Paras":
            case "Sandshrew":
                return 255;
            case "Metapod":
            case "Kakuna":
                return 120;
            case "Pikachu":
                return 190;
            default:
                return 255; // Default case, if not found
        }
    }

    int GetBaseHP(string pokemonName)
    {
        switch (pokemonName)
        {
            case "Pidgey": return 40;
            case "Rattata": return 30;
            case "Spearow": return 40;
            case "Caterpie": return 45;
            case "Weedle": return 40;
            case "Metapod": return 50;
            case "Kakuna": return 45;
            case "Pikachu": return 35;
            case "Jigglypuff": return 115;
            case "Zubat": return 40;
            case "Geodude": return 40;
            case "Paras": return 35;
            case "Clefairy": return 70;
            case "Ekans": return 35;
            case "Sandshrew": return 50;
            default: return 40; // fallback
        }
    }



    void OnPotionButton(PotionType type)
    {
        int itemCount = (type == PotionType.Potion) ? GetItemCount(potionText) : GetItemCount(superPotionText);
        if (itemCount <= 0) return;

        selectedPotion = type;
        pokemonStatusUI.SetActive(true);
    }

    public void CancelPotionUse()
    {
        selectedPotion = PotionType.None;
        pokemonStatusUI.SetActive(false);
    }

    public bool IsUsingPotion() => selectedPotion != PotionType.None;

    public int GetPotionHealAmount()
    {
        return selectedPotion == PotionType.Potion ? 20 :
               selectedPotion == PotionType.SuperPotion ? 50 : 0;
    }

    public void UsePotionItem()
    {
        if (selectedPotion == PotionType.Potion)
            DecrementItem(potionText);
        else if (selectedPotion == PotionType.SuperPotion)
            DecrementItem(superPotionText);
    }

    void DecrementItem(TextMeshProUGUI itemText)
    {
        int count = GetItemCount(itemText);
        count = Mathf.Max(0, count - 1);
        itemText.text = $"x{count}";
    }


    void OpenBag()
    {
        bagUI.SetActive(true);
        Time.timeScale = 0;
    }

    void CloseBag()
    {
        bagUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void UpdateMoneyUI(int money)
    {
        playerMoneyText.text = $"¥{money}";
    }


    public void AddItem(string itemName, int quantity)
    {
        switch (itemName)
        {
            case "Poké Ball":
                IncrementItem(pokeBallText, quantity);
                break;
            case "Great Ball":
                IncrementItem(greatBallText, quantity);
                break;
            case "Potion":
                IncrementItem(potionText, quantity);
                break;
            case "Super Potion":
                IncrementItem(superPotionText, quantity);
                break;
        }
    }

    void IncrementItem(TextMeshProUGUI itemText, int amount)
    {
        string text = itemText.text;
        int count = 0;

        if (text.StartsWith("x"))
        {
            int.TryParse(text.Substring(1), out count);
        }

        count += amount;
        itemText.text = $"x{count}";
    }

    public void UnlockBoulderBadge()
    {
        boulderBadge.SetActive(true);
    }

    public void UnlockCascadeBadge()
    {
        cascadeBadge.SetActive(true);
    }
}

