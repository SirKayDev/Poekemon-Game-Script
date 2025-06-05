using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PokemonSlotsManager : MonoBehaviour
{
    public class Pokemon
    {
        public string name;
        public int baseHP;
        public int level;
        public int currentHP;
        public int exp;
        public Sprite image;

        public Pokemon(string name, int baseHP, Sprite image, int level)
        {
            this.name = name;
            this.baseHP = baseHP;
            this.level = level;
            this.currentHP = CalculateHP();
            this.exp = 0;
            this.image = image;
        }


        public int CalculateHP()
        {
            return ((2 * baseHP * level) / 100) + level + 10;
        }

        public int CalculateEXP()
        {
            return ((level * level * level) / 5) + 64;
        }

        // Method to check if the Pokemon should evolve and update its name and image
        public void CheckEvolution(Dictionary<string, (int, string, Sprite)> evolutionData)
        {
            if (evolutionData.ContainsKey(name))
            {
                var evolutionInfo = evolutionData[name];
                if (level >= evolutionInfo.Item1)  // If the level meets the evolution requirement
                {
                    name = evolutionInfo.Item2;    // Update the name to the evolved form
                    image = evolutionInfo.Item3;   // Update the image to the evolved form
                }
            }
        }

    }

    public Button charmaderButton, bulbasaurButton, squirtleButton;
    public Button[] slotButtons = new Button[6];

    public TMP_Text[] nameTexts = new TMP_Text[6];
    public TMP_Text[] hpTexts = new TMP_Text[6];
    public TMP_Text[] levelTexts = new TMP_Text[6];
    public Image[] pokemonImages = new Image[6];
    public Slider[] healthBars = new Slider[6];
    public Slider[] expBars = new Slider[6];
    public TMP_Text[] expText = new TMP_Text[6];


    private Pokemon[] pokemons = new Pokemon[6];
    private int slotIndex = 0; 
    private int selectedSlot = -1;
    private Color defaultColor = Color.white;
    private Color selectedColor = new Color(0, 1, 0);

    public Sprite charmaderSprite, bulbasaurSprite, squirtleSprite, pidgeySprite, rattataSprite, spearowSprite, caterpieSprite, weedleSprite, metapodSprite, kakunaSprite, pikachuSprite, jigglypuffSprite, zubatSprite, geodudeSprite, parasSprite, clefairySprite, ekansSprite, sandshrewSprite;
    public Sprite IvysaurSprite, CharmeleonSprite, WartortleSprite, KakunaSprite, BeedrillSprite, MetapodSprite, ButterfreeSprite, PidgeottoSprite, RaticateSprite, ArbokSprite, FearowSprite, GolbatSprite, SandslashSprite;

    public GameObject starterSelectionUI;  
    public BattleUI battleUI; 
    public TMP_Text checkPokemonDefeatedText;  
    private string previousDefeatedText = "";
    public TMP_Text checkEnemyLevelText;  
    private string previousEnemyLevelText = "";

    public TextMeshProUGUI EnemyAttack;
    public string EnemyAttackTextCheck = "";

    // Add the evolution data dictionary
    private Dictionary<string, (int level, string evolvedName, Sprite evolvedImage)> evolutionData = new Dictionary<string, (int, string, Sprite)>()
    {
        { "Bulbasaur", (16, "Ivysaur", null) },  // Placeholder null for image; it will be set later
        { "Charmander", (16, "Charmeleon", null) },
        { "Squirtle", (16, "Wartortle", null) },
        { "Weedle", (7, "Kakuna", null) },
        { "Kakuna", (10, "Beedrill", null) },
        { "Caterpie", (7, "Metapod", null) },
        { "Metapod", (10, "Butterfree", null) },
        { "Pidgey", (18, "Pidgeotto", null) },
        { "Rattata", (20, "Raticate", null) },
        { "Ekans", (22, "Arbok", null) },
        { "Spearow", (20, "Fearow", null) },
        { "Zubat", (22, "Golbat", null) },
        { "Sandshrew", (22, "Sandslash", null) }
    };


    public Sprite GetSpriteByName(string name)
    {
        switch (name)
        {
            case "Charmander": return charmaderSprite;
            case "Bulbasaur": return bulbasaurSprite;
            case "Squirtle": return squirtleSprite;
            case "Pidgey": return pidgeySprite;
            case "Rattata": return rattataSprite;
            case "Spearow": return spearowSprite;
            case "Caterpie": return caterpieSprite;
            case "Weedle": return weedleSprite;
            case "Metapod": return metapodSprite;
            case "Kakuna": return kakunaSprite;
            case "Pikachu": return pikachuSprite;
            case "Jigglypuff": return jigglypuffSprite;
            case "Zubat": return zubatSprite;
            case "Geodude": return geodudeSprite;
            case "Paras": return parasSprite;
            case "Clefairy": return clefairySprite;
            case "Ekans": return ekansSprite;
            case "Sandshrew": return sandshrewSprite;
            default: return null;
        }
    }


    private Dictionary<string, int> baseHPData = new Dictionary<string, int>()
    {
        { "Pidgey", 40 },
        { "Rattata", 30 },
        { "Spearow", 40 },
        { "Caterpie", 45 },
        { "Weedle", 40 },
        { "Metapod", 50 },
        { "Kakuna", 45 },
        { "Pikachu", 35 },
        { "Jigglypuff", 115 },
        { "Zubat", 40 },
        { "Geodude", 40 },
        { "Paras", 35 },
        { "Clefairy", 70 },
        { "Ekans", 35 },
        { "Sandshrew", 50 },
        { "Charmander", 39 },
        { "Bulbasaur", 45 },
        { "Squirtle", 44 }
    };

    private Dictionary<string, int> baseExpData = new Dictionary<string, int>()
    {
        { "Magnemite", 89 },
        { "Voltorb", 103 },
        { "Onix", 77 },
        { "Horsea", 83 },
        { "Shellder", 97 },
        { "Staryu", 106 },
        { "Starmie", 207 },
        { "Grimer", 90 },
        { "Koffing", 114 },
        { "Weedle", 39 },
        { "Kakuna", 72 },
        { "Caterpie", 39 },
        { "Metapod", 72 },
        { "Pidgey", 50 },
        { "Rattata", 51 },
        { "Ekans", 62 },
        { "Spearow", 58 },
        { "Jigglypuff", 95 },
        { "Zubat", 49 },
        { "Clefairy", 113 },
        { "Sandshrew", 93 },
        { "Paras", 70 },
        { "Pikachu", 105 },
        { "Geodude", 86 },
        { "Diglett", 81 }
    };


    public bool IsSlotEmpty(int index)
    {
        return pokemons[index] == null;
    }

    void Start()
    {

        evolutionData["Bulbasaur"] = (16, "Ivysaur", IvysaurSprite);
        evolutionData["Charmander"] = (16, "Charmeleon", CharmeleonSprite);
        evolutionData["Squirtle"] = (16, "Wartortle", WartortleSprite);
        evolutionData["Weedle"] = (7, "Kakuna", KakunaSprite);
        evolutionData["Kakuna"] = (10, "Beedrill", BeedrillSprite);
        evolutionData["Caterpie"] = (7, "Metapod", MetapodSprite);
        evolutionData["Metapod"] = (10, "Butterfree", ButterfreeSprite);
        evolutionData["Pidgey"] = (18, "Pidgeotto", PidgeottoSprite);
        evolutionData["Rattata"] = (20, "Raticate", RaticateSprite);
        evolutionData["Ekans"] = (22, "Arbok", ArbokSprite);
        evolutionData["Spearow"] = (20, "Fearow", FearowSprite);
        evolutionData["Zubat"] = (22, "Golbat", GolbatSprite);
        evolutionData["Sandshrew"] = (22, "Sandslash", SandslashSprite);

        charmaderButton.onClick.AddListener(() => AddPokemonToSlot("Charmander", charmaderSprite, 39, 0));
        bulbasaurButton.onClick.AddListener(() => AddPokemonToSlot("Bulbasaur", bulbasaurSprite, 45, 0));
        squirtleButton.onClick.AddListener(() => AddPokemonToSlot("Squirtle", squirtleSprite, 44, 0));

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => SelectSlot(index));
        }

        for (int i = 0; i < expText.Length; i++)
        {
            expText[i].gameObject.SetActive(false);
        }


    }

    void Update()
    {
        // Check for EnemyAttack text update
        if (EnemyAttack.text != EnemyAttackTextCheck)
        {
            EnemyAttackTextCheck = EnemyAttack.text;

            int damage = ExtractDamageFromText(EnemyAttack.text);
            if (damage > 0)
            {
                DealDamageToSlot0(damage);
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) 
        {
            AddPidgeyToSlot(1);  
        }

        if (Input.GetKeyDown(KeyCode.E)) 
        {
            AddExpToAllPokemons(1000); 
        }

        // Check if the defeated Pokémon's name has changed (or is updated)
        if (checkPokemonDefeatedText.text != previousDefeatedText && !string.IsNullOrEmpty(checkPokemonDefeatedText.text))
        {
            DropEXP();  // Call DropEXP only once when the Pokémon name in the text changes (defeated Pokémon)
            previousDefeatedText = checkPokemonDefeatedText.text;  // Update the previous defeated text to the current one

            // After EXP is dropped, reset the defeated Pokémon text to prevent repeated calls
            checkPokemonDefeatedText.text = ""; // Reset the text after calling DropEXP()
        }

        // If the defeated Pokémon name is cleared, reset the previous text to empty
        if (string.IsNullOrEmpty(checkPokemonDefeatedText.text))
        {
            previousDefeatedText = "";  // Reset the text when no Pokémon is defeated
        }
    }

    // Extracts first number from a string
    int ExtractDamageFromText(string text)
    {
        foreach (var word in text.Split(' '))
        {
            if (int.TryParse(word, out int number))
            {
                return number;
            }
        }
        return 0;
    }

    // Only damage Pokémon in slot 0
    void DealDamageToSlot0(int damageAmount)
    {
        if (pokemons.Length > 0 && pokemons[0] != null)
        {
            pokemons[0].currentHP -= damageAmount;
            if (pokemons[0].currentHP < 0)
            {
                pokemons[0].currentHP = 0;
            }

            UpdateSlotUI(0);

            if (battleUI != null)
            {
                Pokemon p = pokemons[0];
                battleUI.SetupAlly(
                    p.name,
                    p.image,
                    p.level,
                    p.CalculateHP(),
                    p.currentHP,
                    p.CalculateEXP(),
                    p.exp
                );
            }
        }
    }

    void AddExpToAllPokemons(int amount)
    {
        for (int i = 0; i < pokemons.Length; i++)
        {
            if (pokemons[i] != null)
            {
                // Add EXP to the Pokémon
                pokemons[i].exp += amount;

                // Check if the Pokémon has leveled up
                int expRequired = pokemons[i].CalculateEXP();
                while (pokemons[i].exp >= expRequired)  // Loop for multiple level ups if EXP exceeds required
                {
                    // Calculate how much EXP remains after leveling up
                    int remainingExp = pokemons[i].exp - expRequired;

                    // Level up the Pokémon
                    pokemons[i].level++;

                    // Set the current HP to max HP for the new level
                    pokemons[i].currentHP = pokemons[i].CalculateHP();

                    // Check if the Pokémon should evolve
                    pokemons[i].CheckEvolution(evolutionData);

                    // Update the EXP for the next level (subtract full required EXP for this level)
                    pokemons[i].exp = remainingExp;

                    // Update the new required EXP for the next level
                    expRequired = pokemons[i].CalculateEXP();
                }

                // Update the UI for this Pokémon slot
                UpdateSlotUI(i);
            }

            if (i == 0 && battleUI != null)
            {
                Pokemon p = pokemons[0];
                battleUI.SetupAlly(
                    p.name,
                    p.image,
                    p.level,
                    p.CalculateHP(),
                    p.currentHP,
                    p.CalculateEXP(),
                    p.exp
                );
            }

        }
    }

    public void DropEXP()
    {
        string defeatedPokemonName = checkPokemonDefeatedText.text.Trim();  // Get the name of the defeated Pokémon

        // Retrieve and parse the enemy level from the TMP_Text field
        string levelText = checkEnemyLevelText.text.Trim();  // Get the text from checkEnemyLevelText
        int enemyLevel = 1;  // Default to level 1 in case parsing fails

        if (levelText.StartsWith("Lv."))
        {
            // Try to parse the level after "Lv."
            string levelNumber = levelText.Substring(3).Trim();  // Remove "Lv." and trim spaces
            if (int.TryParse(levelNumber, out int parsedLevel))
            {
                enemyLevel = parsedLevel;  // Successfully parsed the enemy level
            }
            else
            {
                Debug.LogWarning("Failed to parse enemy level from the text: " + levelText);
            }
        }
        else
        {
            Debug.LogWarning("Enemy level text is not in the expected format: " + levelText);
        }

        if (baseExpData.ContainsKey(defeatedPokemonName))
        {
            int baseExp = baseExpData[defeatedPokemonName];  // Get base EXP of defeated Pokémon

            // Calculate the EXP drop based on the formula: (BaseEXP × Enemy Level) / 7
            int expDrop = (baseExp * enemyLevel) / 7;

            // Check how many Pokémon are in the slots
            int activePokemons = 0;
            for (int i = 0; i < pokemons.Length; i++)
            {
                if (pokemons[i] != null)
                {
                    activePokemons++;
                }
            }

            // Distribute the EXP drop based on how many active Pokémon are in the slots
            if (activePokemons == 1)
            {
                // If there's only one Pokémon, they get 100% of the EXP drop
                pokemons[0].exp += expDrop;
            }
            else
            {
                // Otherwise, the first Pokémon gets 80% and the rest get 20% each
                int expForFirst = Mathf.FloorToInt(expDrop * 0.8f);
                int expForOthers = Mathf.FloorToInt(expDrop * 0.2f);

                for (int i = 0; i < pokemons.Length; i++)
                {
                    if (pokemons[i] != null)
                    {
                        if (i == 0)
                        {
                            pokemons[i].exp += expForFirst;
                        }
                        else
                        {
                            pokemons[i].exp += expForOthers;
                        }
                    }
                }
            }

            // After distributing EXP, check for level-ups and evolutions for each Pokémon
            for (int i = 0; i < pokemons.Length; i++)
            {
                if (pokemons[i] != null)
                {
                    // Level-up check: while EXP exceeds required EXP for the next level, level up the Pokémon
                    int expRequired = pokemons[i].CalculateEXP();
                    while (pokemons[i].exp >= expRequired)
                    {

                        pokemons[i].level++;
                        int remainingExp = pokemons[i].exp - expRequired;
                        pokemons[i].currentHP = pokemons[i].CalculateHP();
                        pokemons[i].exp = remainingExp;
                        expRequired = pokemons[i].CalculateEXP();
                        pokemons[i].CheckEvolution(evolutionData);
                    }

                    // Update the UI for this Pokémon slot
                    UpdateSlotUI(i);
                }

                if (i == 0 && battleUI != null)
                {
                    Pokemon p = pokemons[0];
                    battleUI.SetupAlly(
                        p.name,
                        p.image,
                        p.level,
                        p.CalculateHP(),
                        p.currentHP,
                        p.CalculateEXP(),
                        p.exp
                    );
                }

            }
        }
        else
        {
            Debug.LogWarning("Defeated Pokémon name not found in baseExpData: " + defeatedPokemonName);
        }
    }


    void AddPidgeyToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 6)
        {
            Debug.LogError("Invalid slot index!");
            return;
        }

        // Create the Pidgey Pokémon and add it to the slot if the slot is empty
        if (pokemons[slotIndex] == null)
        {
            pokemons[slotIndex] = new Pokemon("Pidgey", 30, pidgeySprite, 5);  // Pidgey at level 5
            UpdateSlotUI(slotIndex);  // Update UI for that slot
        }
        else
        {
            Debug.LogError("Slot is already occupied!");
        }
    }

    public void AddPokemonToSlot(string name, Sprite sprite, int baseHP, int slotIndex, int level = 5)
    {
        if (slotIndex < 0 || slotIndex >= 6)
        {
            Debug.LogError("Invalid slot index!");
            return;
        }

        pokemons[slotIndex] = new Pokemon(name, baseHP, sprite, level);

        UpdateSlotUI(slotIndex);

        // Sync with Battle UI if it's the first slot
        if (slotIndex == 0 && battleUI != null)
        {
            Pokemon p = pokemons[0];
            battleUI.SetupAlly(
                p.name,
                p.image,
                p.level,
                p.CalculateHP(),
                p.currentHP,
                p.CalculateEXP(),
                p.exp
            );
        }
    }

    void SelectSlot(int index)
    {
        if (pokemons[index] == null) return;

        if (Bag.Instance.IsUsingPotion())
        {
            int maxHP = pokemons[index].CalculateHP();
            if (pokemons[index].currentHP < maxHP)
            {
                pokemons[index].currentHP = Mathf.Min(pokemons[index].currentHP + Bag.Instance.GetPotionHealAmount(), maxHP);
                UpdateSlotUI(index);
                Bag.Instance.UsePotionItem();
                Bag.Instance.CancelPotionUse();
            }
            return;
        }

        // Original selection logic
        Button slotButton = slotButtons[index];
        Image slotImage = slotButton.GetComponent<Image>();

        if (selectedSlot == index)
        {
            slotImage.color = defaultColor;
            selectedSlot = -1;
        }
        else if (selectedSlot == -1)
        {
            slotImage.color = selectedColor;
            selectedSlot = index;
        }
        else
        {
            SwapPokemon(selectedSlot, index);
            slotButtons[selectedSlot].GetComponent<Image>().color = defaultColor;
            selectedSlot = -1;
        }
    }

    void SwapPokemon(int slotA, int slotB)
    {
        Pokemon temp = pokemons[slotA];
        pokemons[slotA] = pokemons[slotB];
        pokemons[slotB] = temp;

        UpdateSlotUI(slotA);
        UpdateSlotUI(slotB);
    }

    public void UpdateSlotUI(int index)
    {
        if (pokemons[index] == null) return;

        Pokemon pokemon = pokemons[index];
        nameTexts[index].text = pokemon.name;
        hpTexts[index].text = $"{pokemon.currentHP} / {pokemon.CalculateHP()}";
        levelTexts[index].text = "Lv." + pokemon.level;  // This will automatically show the updated level
        pokemonImages[index].sprite = pokemon.image;

        healthBars[index].value = (float)pokemon.currentHP / pokemon.CalculateHP() * 100;

        int expRequired = pokemon.CalculateEXP();
        expBars[index].value = (float)pokemon.exp / expRequired * 100;  // This will now show the progress bar correctly
        expText[index].text = $"{pokemon.exp} / {expRequired}";  // This will show the updated EXP as text

        // Sync with Battle UI if it's the first slot
        if (index == 0 && battleUI != null)
        {
            Pokemon p = pokemons[0];
            battleUI.SetupAlly(
                p.name,
                p.image,
                p.level,
                p.CalculateHP(),
                p.currentHP,
                p.CalculateEXP(),
                p.exp
            );
        }
    }

    public void ReduceHealthAllSlots(int damageAmount)
    {
        for (int i = 0; i < pokemons.Length; i++)
        {
            if (pokemons[i] != null) // Check if slot is occupied
            {
                pokemons[i].currentHP -= damageAmount;
                if (pokemons[i].currentHP < 0)
                {
                    pokemons[i].currentHP = 0; // Ensure HP doesn't go negative
                }
                UpdateSlotUI(i); // Refresh UI
            }

            // Always sync the first slot (slot 0) with Battle UI
            if (i == 0 && battleUI != null)
            {
                Pokemon p = pokemons[0];
                battleUI.SetupAlly(
                    p.name,
                    p.image,
                    p.level,
                    p.CalculateHP(),
                    p.currentHP,
                    p.CalculateEXP(),
                    p.exp
                );
            }
        }
    }

    public void HealAllPokemon()
    {
        for (int i = 0; i < pokemons.Length; i++)
        {
            if (pokemons[i] != null)
            {
                pokemons[i].currentHP = pokemons[i].CalculateHP(); // Set HP to max
                UpdateSlotUI(i); // Refresh UI
            }
        }

        // Always sync the first slot (slot 0) with Battle UI after healing
        if (battleUI != null)
        {
            Pokemon p = pokemons[0];
            battleUI.SetupAlly(
                p.name,
                p.image,
                p.level,
                p.CalculateHP(),
                p.currentHP,
                p.CalculateEXP(),
                p.exp
            );
        }
    }
}

