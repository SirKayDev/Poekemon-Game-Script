using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PokemonMove
{
    public string Name;                  
    public int? BasePower;               
    public string Type;
    public string MoveEffect;

    public StatChange Buff;            
    public StatChange Nerf;           

    public float LandChance;            
    public bool SleepMode;              

   
    public PokemonMove(string name, int? basePower, string type, string moveEffect,
                       StatChange buff = null, StatChange nerf = null,
                       float landChance = 100f, bool sleepMode = false)
    {
        Name = name;
        BasePower = basePower;
        Type = type;
        MoveEffect = moveEffect;
        Buff = buff;
        Nerf = nerf;
        LandChance = landChance;
        SleepMode = sleepMode;
    }
}

[System.Serializable]
public class StatChange
{
    public string Stat;                 // "attack", "defense", "status", etc.
    public float Amount;                // Multiplier or % effect (e.g., 0.66f)
    public string Effect;               // Optional status effect like "sleep", "paralyze"

    public StatChange(string stat, float amount = 1f, string effect = null)
    {
        Stat = stat;
        Amount = amount;
        Effect = effect;
    }
}

[System.Serializable]
public class PokemonStats
{
    public string Name;
    public int BaseAttack;
    public int BaseDefense;

    public PokemonStats(string name, int baseAttack, int baseDefense)
    {
        Name = name;
        BaseAttack = baseAttack;
        BaseDefense = baseDefense;
    }
}

public class PokemonMoveDatabase : MonoBehaviour
{
    public List<PokemonMove> MoveList = new List<PokemonMove>();
    public List<PokemonStats> PokemonStatsList = new List<PokemonStats>();
    public Dictionary<string, string[]> pokemonMoves = new Dictionary<string, string[]>();

    public TextMeshProUGUI AllyPokemonName;
    public TextMeshProUGUI AllylevelCheck;
    public TextMeshProUGUI AllyHPText;
    public Button AllyMove1;
    public Button AllyMove2;
    public Button AllyMove3;
    public Button AllyMove4;
    public TextMeshProUGUI AllyMovename1;
    public TextMeshProUGUI AllyMovename2;
    public TextMeshProUGUI AllyMovename3;
    public TextMeshProUGUI AllyMovename4;

    public TextMeshProUGUI EnemyPokemonName;
    public TextMeshProUGUI EnemylevelCheck;
    public TextMeshProUGUI EnemyHpText;
    public Button EnemyMove1;
    public Button EnemyMove2;
    public Button EnemyMove3;
    public Button EnemyMove4;
    public TextMeshProUGUI EnemyMovename1;
    public TextMeshProUGUI EnemyMovename2;
    public TextMeshProUGUI EnemyMovename3;
    public TextMeshProUGUI EnemyMovename4;

    public string lastAllyName = "";
    public string lastEnemyName = "";

    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI EnemyUseText;
    public GameObject BattleUI;

    private enum BattleTurn { None, Ally, Enemy }
    private BattleTurn currentTurn = BattleTurn.None;
    private bool battleStarted = false;
    private bool allyActionTaken = false;

    public TextMeshProUGUI AllyDamageDisplay;
    public TextMeshProUGUI EnemyDamageDisplay;
    private string lastEnemyDamage = "";

    public TextMeshProUGUI PokemonCheck;

    public void Update()
    {
        // Check Ally
        if (AllyPokemonName != null && AllyPokemonName.text != lastAllyName)
        {
            lastAllyName = AllyPokemonName.text;
            UpdateAllyMoves(lastAllyName);
        }

        // Check Enemy
        if (EnemyPokemonName != null && EnemyPokemonName.text != lastEnemyName)
        {
            lastEnemyName = EnemyPokemonName.text;
            UpdateEnemyMoves(lastEnemyName);
        }

        // Start Battle if opened
        if (BattleUI.activeSelf && !battleStarted)
        {
            battleStarted = true;
            StartCoroutine(BattleLoop());
        }

        // Reset battle state when closed
        if (!BattleUI.activeSelf && battleStarted)
        {
            battleStarted = false;
            currentTurn = BattleTurn.None;
        }

        
    }

    private IEnumerator ApplyDamageToAlly(int damage)
    {
        // Parse the AllyHPText, expecting format like "82 / 100"
        string[] parts = AllyHPText.text.Split('/');
        if (parts.Length == 2 &&
            int.TryParse(parts[0].Trim(), out int currentHP) &&
            int.TryParse(parts[1].Trim(), out int maxHP))
        {
            int newHP = Mathf.Max(currentHP - damage, 0);
            AllyHPText.text = $"{newHP} / {maxHP}";
        }

        // Show damage text briefly
        EnemyDamageDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        EnemyDamageDisplay.gameObject.SetActive(false);
    }


    private IEnumerator BattleLoop()
    {
        // Randomize starting turn
        currentTurn = (Random.value > 0.5f) ? BattleTurn.Ally : BattleTurn.Enemy;

        while (BattleUI.activeSelf)
        {
            yield return StartCoroutine(HandleTurn(currentTurn));

            // Toggle turn
            currentTurn = (currentTurn == BattleTurn.Ally) ? BattleTurn.Enemy : BattleTurn.Ally;
        }
    }


    private IEnumerator HandleTurn(BattleTurn turn)
    {
        currentTurn = turn;
        TurnText.text = turn == BattleTurn.Enemy ? "Enemy Turn" : "Ally Turn";

        if (turn == BattleTurn.Enemy)
        {
            string[] moves = pokemonMoves.ContainsKey(EnemyPokemonName.text) ? pokemonMoves[EnemyPokemonName.text] : null;

            if (moves != null && moves.Length >= 4)
            {
                int moveIndex = Random.Range(0, 4);
                string chosenMove = moves[moveIndex];

                PokemonMove move = MoveList.Find(m => m.Name == chosenMove);

                if (move != null && move.MoveEffect == "Damage")
                {
                    PokemonStats enemyStats = PokemonStatsList.Find(p => p.Name == EnemyPokemonName.text);
                    PokemonStats allyStats = PokemonStatsList.Find(p => p.Name == AllyPokemonName.text);

                    if (enemyStats != null && allyStats != null && move.BasePower.HasValue)
                    {
                        string rawLevelText = EnemylevelCheck.text;
                        string numericLevel = System.Text.RegularExpressions.Regex.Match(rawLevelText, @"\d+").Value;

                        if (int.TryParse(numericLevel, out int enemyLevel))
                        {
                            float damage = ((enemyStats.BaseAttack * move.BasePower.Value * enemyLevel) / (allyStats.BaseDefense * 50f)) + 2;
                            int intDamage = Mathf.FloorToInt(damage);
                            EnemyDamageDisplay.text = intDamage.ToString();
                            StartCoroutine(ApplyDamageToAlly(intDamage)); // Always apply

                        }
                        else
                        {
                            Debug.LogError("Failed to parse Enemy level: " + rawLevelText);
                        }
                    }
                }

                EnemyUseText.text = $"{EnemyPokemonName.text} used {chosenMove}!";
                yield return new WaitForSeconds(2f);
            }
            else
            {
                EnemyUseText.text = $"{EnemyPokemonName.text} is confused...";
                yield return new WaitForSeconds(2f);
            }
        }
        else
        {
            EnableAllyButtons(true);
            allyActionTaken = false;

            while (!allyActionTaken)
                yield return null;

            EnableAllyButtons(false);
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnAllyMoveUsed(int moveIndex)
    {
        if (currentTurn == BattleTurn.Ally)
        {
            string[] moves = pokemonMoves.ContainsKey(AllyPokemonName.text) ? pokemonMoves[AllyPokemonName.text] : null;

            if (moves != null && moveIndex >= 1 && moveIndex <= moves.Length)
            {
                string chosenMove = moves[moveIndex - 1];
                PokemonMove move = MoveList.Find(m => m.Name == chosenMove);

                if (move != null && move.MoveEffect == "Damage")
                {
                    // Get stats
                    PokemonStats allyStats = PokemonStatsList.Find(p => p.Name == AllyPokemonName.text);
                    PokemonStats enemyStats = PokemonStatsList.Find(p => p.Name == EnemyPokemonName.text);

                    if (allyStats != null && enemyStats != null && move.BasePower.HasValue)
                    {
                        string rawLevelText = AllylevelCheck.text;

                        // Extract digits from string (handles cases like "Lv 10" or "Level: 12")
                        string numericLevel = System.Text.RegularExpressions.Regex.Match(rawLevelText, @"\d+").Value;

                        if (int.TryParse(numericLevel, out int allyLevel))
                        {
                            float damage = ((allyStats.BaseAttack * move.BasePower.Value * allyLevel) / (enemyStats.BaseDefense * 50f)) + 2;
                            AllyDamageDisplay.text = Mathf.FloorToInt(damage).ToString();
                        }
                        else
                        {
                            Debug.LogError("Failed to parse Ally level: " + rawLevelText);
                        }
                    }
                }

                allyActionTaken = true;
                Debug.Log($"Ally used {chosenMove}");
            }
        }
    }

    private void EnableAllyButtons(bool enable)
    {
        AllyMove1.interactable = enable;
        AllyMove2.interactable = enable;
        AllyMove3.interactable = enable;
        AllyMove4.interactable = enable;
    }

    void UpdateAllyMoves(string pokemonName)
    {
        if (pokemonMoves.ContainsKey(pokemonName))
        {
            string[] selectedMoves = pokemonMoves[pokemonName];

            SetMoveButton(AllyMove1, AllyMovename1, selectedMoves, 0);
            SetMoveButton(AllyMove2, AllyMovename2, selectedMoves, 1);
            SetMoveButton(AllyMove3, AllyMovename3, selectedMoves, 2);
            SetMoveButton(AllyMove4, AllyMovename4, selectedMoves, 3);
        }
    }


    void UpdateEnemyMoves(string pokemonName)
    {
        if (pokemonMoves.ContainsKey(pokemonName))
        {
            string[] selectedMoves = pokemonMoves[pokemonName];

            SetMoveButton(EnemyMove1, EnemyMovename1, selectedMoves, 0);
            SetMoveButton(EnemyMove2, EnemyMovename2, selectedMoves, 1);
            SetMoveButton(EnemyMove3, EnemyMovename3, selectedMoves, 2);
            SetMoveButton(EnemyMove4, EnemyMovename4, selectedMoves, 3);
        }
    }

    void SetMoveButton(Button button, TextMeshProUGUI moveText, string[] moves, int index)
    {
        if (moves.Length > index)
        {
            string moveName = moves[index];
            moveText.text = moveName;

            PokemonMove move = MoveList.Find(m => m.Name == moveName);
            if (move != null && typeColors.ContainsKey(move.Type))
            {
                Color color = typeColors[move.Type];
                ColorBlock colors = button.colors;
                colors.normalColor = color;
                colors.highlightedColor = color;
                colors.pressedColor = color * 0.9f;
                colors.selectedColor = color;
                button.colors = colors;
            }
        }
        else
        {
            moveText.text = "-";
        }
    }


    void ClearButtonListeners(Button button)
    {
        button.onClick.RemoveAllListeners();
    }



    private void Awake()
    {
        // Moves
        MoveList.Add(new PokemonMove("Tackle", 40, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Thunder Shock", 40, "Electric", "Damage"));
        MoveList.Add(new PokemonMove("Metal Sound", null, "Steel", "Nerf", nerf: new StatChange("defense", 0.66f)));
        MoveList.Add(new PokemonMove("Sonic Boom", 20, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Thunder Wave", null, "Electric", "Sleep", nerf: new StatChange("status", effect: "paralyze"), landChance: 90f, sleepMode: true));
        MoveList.Add(new PokemonMove("Charge", null, "Electric", "Buff", buff: new StatChange("attack", 1.5f)));
        MoveList.Add(new PokemonMove("Explosion", 250, "Normal", "Special"));
        MoveList.Add(new PokemonMove("Rock Throw", 50, "Rock", "Damage"));
        MoveList.Add(new PokemonMove("Bind", 60, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Stealth Rock", null, "Rock", "Buff", buff: new StatChange("attack", 2f)));
        MoveList.Add(new PokemonMove("Bubble", 20, "Water", "Damage"));
        MoveList.Add(new PokemonMove("Leer", null, "Normal", "Nerf", nerf: new StatChange("defense", 0.66f)));
        MoveList.Add(new PokemonMove("Smokescreen", null, "Normal", "Nerf", nerf: new StatChange("damage", 0.66f)));
        MoveList.Add(new PokemonMove("Water Gun", 40, "Water", "Damage"));
        MoveList.Add(new PokemonMove("Withdraw", null, "Water", "Buff", buff: new StatChange("defense", 1.5f)));
        MoveList.Add(new PokemonMove("Supersonic", null, "Normal", "Sleep", nerf: new StatChange("status", effect: "confuse"), landChance: 55f, sleepMode: true));
        MoveList.Add(new PokemonMove("Harden", null, "Normal", "Buff", buff: new StatChange("defense", 1.5f))); 
        MoveList.Add(new PokemonMove("Recover", null, "Normal", "Special"));
        MoveList.Add(new PokemonMove("Psychic", 90, "Psychic", "Damage"));
        MoveList.Add(new PokemonMove("Swift", 60, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Pound", 40, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Disable", null, "Normal", "Special", nerf: new StatChange("status", effect: "disable")));
        MoveList.Add(new PokemonMove("Sludge", 65, "Poison", "Damage"));
        MoveList.Add(new PokemonMove("Astonish", 30, "Ghost", "Damage"));
        MoveList.Add(new PokemonMove("Growl", null, "Normal", "Nerf", nerf: new StatChange("attack", 0.66f)));
        MoveList.Add(new PokemonMove("Vine Whip", 45, "Grass", "Damage"));
        MoveList.Add(new PokemonMove("Sweet Scent", null, "Normal", "Nerf", nerf: new StatChange("damage", 0.66f)));
        MoveList.Add(new PokemonMove("Razor Leaf", 55, "Grass", "Damage"));
        MoveList.Add(new PokemonMove("Scratch", 40, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Ember", 40, "Fire", "Damage"));
        MoveList.Add(new PokemonMove("Fire Fang", 65, "Fire", "Damage"));
        MoveList.Add(new PokemonMove("Tail Whip", null, "Normal", "Nerf", nerf: new StatChange("defense", 0.66f)));
        MoveList.Add(new PokemonMove("Bite", 60, "Dark", "Damage"));
        MoveList.Add(new PokemonMove("Poison Sting", 45, "Poison", "Damage"));
        MoveList.Add(new PokemonMove("Protect", null, "Normal", "Buff", buff: new StatChange("defense", 1.5f)));
        MoveList.Add(new PokemonMove("String Shot", null, "Bug", "Nerf", nerf: new StatChange("defense", 0.66f)));
        MoveList.Add(new PokemonMove("Fury Attack", 65, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Poison Jab", 80, "Poison", "Damage"));
        MoveList.Add(new PokemonMove("Agility", null, "Psychic", "Buff", buff: new StatChange("damage", 1.66f)));
        MoveList.Add(new PokemonMove("Twinneedle", 50, "Bug", "Damage"));
        MoveList.Add(new PokemonMove("Confusion", 50, "Psychic", "Damage"));
        MoveList.Add(new PokemonMove("Poison Powder", null, "Poison", "Sleep", nerf: new StatChange("status", effect: "sleep"), landChance: 75f));
        MoveList.Add(new PokemonMove("Sleep Powder", null, "Grass", "Special", nerf: new StatChange("status", effect: "sleep"), landChance: 75f));
        MoveList.Add(new PokemonMove("Leech Life", 20, "Bug", "Damage"));
        MoveList.Add(new PokemonMove("Gust", 40, "Flying", "Damage"));
        MoveList.Add(new PokemonMove("Sand Attack", null, "Ground", "Nerf", nerf: new StatChange("damage", 0.66f)));
        MoveList.Add(new PokemonMove("Quick Attack", 40, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Roost", null, "Flying", "Special"));
        MoveList.Add(new PokemonMove("Hyper Fang", 80, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Focus Energy", null, "Normal", "Buff", buff: new StatChange("damage", 1.66f)));
        MoveList.Add(new PokemonMove("Crunch", 80, "Dark", "Damage"));
        MoveList.Add(new PokemonMove("Hyper Beam", 75, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Wrap", 60, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Glare", null, "Normal", "Sleep", nerf: new StatChange("status", effect: "sleep"), landChance: 50f));
        MoveList.Add(new PokemonMove("Poison Fang", 50, "Poison", "Damage"));
        MoveList.Add(new PokemonMove("Acid", 40, "Poison", "Damage"));
        MoveList.Add(new PokemonMove("Peck", 35, "Flying", "Damage"));
        MoveList.Add(new PokemonMove("Drill Peck", 80, "Flying", "Damage"));
        MoveList.Add(new PokemonMove("Sing", null, "Normal", "Sleep", nerf: new StatChange("status", effect: "sleep"), landChance: 55f));
        MoveList.Add(new PokemonMove("Defensive Curl", null, "Normal", "Buff", buff: new StatChange("defense", 1.66f)));
        MoveList.Add(new PokemonMove("Confuse Ray", null, "Ghost", "Sleep", nerf: new StatChange("status", effect: "confuse"), landChance: 50f));
        MoveList.Add(new PokemonMove("Acrobatics", 55, "Flying", "Damage"));
        MoveList.Add(new PokemonMove("Charm", null, "Fairy", "Nerf", nerf: new StatChange("attack", 0.66f)));
        MoveList.Add(new PokemonMove("Slash", 70, "Normal", "Damage"));
        MoveList.Add(new PokemonMove("Sandstorm", null, "Rock", "Buff", buff: new StatChange("attack", 1.66f)));
        MoveList.Add(new PokemonMove("Stun Spore", null, "Grass", "Sleep", nerf: new StatChange("status", effect: "sleep"), landChance: 75f));
        MoveList.Add(new PokemonMove("Thunderbolt", 95, "Electric", "Damage"));
        MoveList.Add(new PokemonMove("Double Team", null, "Normal", "Buff", buff: new StatChange("defense", 1.5f)));
        MoveList.Add(new PokemonMove("Magnitude", 75, "Ground", "Damage"));

        // Pokemon Base Stats
        PokemonStatsList.Add(new PokemonStats("Magnemite", 35, 70));
        PokemonStatsList.Add(new PokemonStats("Voltorb", 30, 50));
        PokemonStatsList.Add(new PokemonStats("Onix", 45, 160));
        PokemonStatsList.Add(new PokemonStats("Horsea", 40, 70));
        PokemonStatsList.Add(new PokemonStats("Shellder", 65, 100));
        PokemonStatsList.Add(new PokemonStats("Staryu", 45, 55));
        PokemonStatsList.Add(new PokemonStats("Starmie", 75, 85));
        PokemonStatsList.Add(new PokemonStats("Grimer", 80, 50));
        PokemonStatsList.Add(new PokemonStats("Koffing", 65, 95));
        PokemonStatsList.Add(new PokemonStats("Bulbasaur", 49, 49));
        PokemonStatsList.Add(new PokemonStats("Ivysaur", 62, 63));
        PokemonStatsList.Add(new PokemonStats("Charmander", 52, 43));
        PokemonStatsList.Add(new PokemonStats("Charmeleon", 64, 58));
        PokemonStatsList.Add(new PokemonStats("Squirtle", 48, 65));
        PokemonStatsList.Add(new PokemonStats("Wartortle", 63, 80));
        PokemonStatsList.Add(new PokemonStats("Weedle", 35, 30));
        PokemonStatsList.Add(new PokemonStats("Kakuna", 25, 50));
        PokemonStatsList.Add(new PokemonStats("Beedrill", 90, 40));
        PokemonStatsList.Add(new PokemonStats("Caterpie", 30, 35));
        PokemonStatsList.Add(new PokemonStats("Metapod", 20, 55));
        PokemonStatsList.Add(new PokemonStats("Butterfree", 45, 50));
        PokemonStatsList.Add(new PokemonStats("Pidgey", 45, 40));
        PokemonStatsList.Add(new PokemonStats("Pidgeotto", 60, 55));
        PokemonStatsList.Add(new PokemonStats("Rattata", 56, 35));
        PokemonStatsList.Add(new PokemonStats("Raticate", 81, 60));
        PokemonStatsList.Add(new PokemonStats("Ekans", 60, 44));
        PokemonStatsList.Add(new PokemonStats("Arbok", 85, 69));
        PokemonStatsList.Add(new PokemonStats("Spearow", 60, 30));
        PokemonStatsList.Add(new PokemonStats("Fearow", 90, 65));
        PokemonStatsList.Add(new PokemonStats("Jigglypuff", 45, 20));
        PokemonStatsList.Add(new PokemonStats("Zubat", 45, 35));
        PokemonStatsList.Add(new PokemonStats("Golbat", 80, 70));
        PokemonStatsList.Add(new PokemonStats("Clefairy", 45, 48));
        PokemonStatsList.Add(new PokemonStats("Sandshrew", 75, 85));
        PokemonStatsList.Add(new PokemonStats("Sandslash", 100, 110));
        PokemonStatsList.Add(new PokemonStats("Paras", 70, 55));
        PokemonStatsList.Add(new PokemonStats("Pikachu", 55, 40));
        PokemonStatsList.Add(new PokemonStats("Geodude", 80, 100));
        PokemonStatsList.Add(new PokemonStats("Diglett", 55, 25));

        // Define Pokémon and their moves (simplified to move names only)
        pokemonMoves.Add("Magnemite", new string[] { "Tackle", "Thunder Shock", "Metal Sound", "Sonic Boom" });
        pokemonMoves.Add("Voltorb", new string[] { "Tackle", "Thunder Wave", "Charge", "Explosion" });
        pokemonMoves.Add("Onix", new string[] { "Tackle", "Rock Throw", "Bind", "Stealth Rock" });
        pokemonMoves.Add("Horsea", new string[] { "Bubble", "Leer", "Smokescreen", "Water Gun" });
        pokemonMoves.Add("Shellder", new string[] { "Tackle", "Withdraw", "Supersonic", "Water Gun" });
        pokemonMoves.Add("Staryu", new string[] { "Tackle", "Water Gun", "Harden", "Recover" });
        pokemonMoves.Add("Starmie", new string[] { "Tackle", "Water Gun", "Psychic", "Swift" });
        pokemonMoves.Add("Grimer", new string[] { "Pound", "Disable", "Sludge", "Astonish" });
        pokemonMoves.Add("Koffing", new string[] { "Tackle", "Smokescreen", "Astonish", "Sludge" });
        pokemonMoves.Add("Bulbasaur", new string[] { "Tackle", "Growl", "Vine Whip", "Sweet Scent" });
        pokemonMoves.Add("Ivysaur", new string[] { "Tackle", "Vine Whip", "Sweet Scent", "Razor Leaf" });
        pokemonMoves.Add("Charmander", new string[] { "Scratch", "Growl", "Ember", "Leer" });
        pokemonMoves.Add("Charmeleon", new string[] { "Scratch", "Ember", "Leer", "Fire Fang" });
        pokemonMoves.Add("Squirtle", new string[] { "Tackle", "Tail Whip", "Water Gun", "Bubble" });
        pokemonMoves.Add("Wartortle", new string[] { "Bite", "Water Gun", "Withdraw", "Protect" });
        pokemonMoves.Add("Weedle", new string[] { "Poison Sting", "Leer", "String Shot" });
        pokemonMoves.Add("Kakuna", new string[] { "Harden", "Poison Sting" });
        pokemonMoves.Add("Beedrill", new string[] { "Fury Attack", "Poison Jab", "Agility", "Twinneedle" });
        pokemonMoves.Add("Caterpie", new string[] { "Tackle", "String Shot" });
        pokemonMoves.Add("Metapod", new string[] { "Harden" });
        pokemonMoves.Add("Butterfree", new string[] { "Confusion", "Poison Powder", "Sleep Powder", "Leech Life" });
        pokemonMoves.Add("Pidgey", new string[] { "Tackle", "Gust", "Sand Attack", "Quick Attack" });
        pokemonMoves.Add("Pidgeotto", new string[] { "Tackle", "Gust", "Quick Attack", "Roost" });
        pokemonMoves.Add("Rattata", new string[] { "Tackle", "Quick Attack", "Hyper Fang", "Focus Energy" });
        pokemonMoves.Add("Raticate", new string[] { "Hyper Fang", "Quick Attack", "Crunch", "Hyper Beam" });
        pokemonMoves.Add("Ekans", new string[] { "Wrap", "Leer", "Poison Sting", "Glare" });
        pokemonMoves.Add("Arbok", new string[] { "Bite", "Glare", "Poison Fang", "Acid" });
        pokemonMoves.Add("Spearow", new string[] { "Peck", "Leer", "Drill Peck", "Agility" });
        pokemonMoves.Add("Fearow", new string[] { "Peck", "Agility", "Drill Peck", "Fury Attack" });
        pokemonMoves.Add("Jigglypuff", new string[] { "Pound", "Sing", "Disable", "Defense Curl" });
        pokemonMoves.Add("Zubat", new string[] { "Leech Life", "Supersonic", "Quick Attack", "Poison Fang" });
        pokemonMoves.Add("Golbat", new string[] { "Bite", "Confuse Ray", "Poison Fang", "Acrobatics" });
        pokemonMoves.Add("Clefairy", new string[] { "Pound", "Charm", "Sing", "Defense Curl" });
        pokemonMoves.Add("Sandshrew", new string[] { "Scratch", "Defense Curl", "Sand Attack", "Swift" });
        pokemonMoves.Add("Sandslash", new string[] { "Slash", "Defense Curl", "Sandstorm", "Poison Sting" });
        pokemonMoves.Add("Paras", new string[] { "Scratch", "Poison Powder", "Stun Spore", "Leech Life" });
        pokemonMoves.Add("Pikachu", new string[] { "Quick Attack", "Thunder Wave", "Thunderbolt", "Double Team" });
        pokemonMoves.Add("Geodude", new string[] { "Tackle", "Defense Curl", "Rock Throw", "Magnitude" });
        pokemonMoves.Add("Diglett", new string[] { "Scratch", "Growl", "Sand Attack", "Astonish" });

        AllyMove1.onClick.AddListener(() => OnAllyMoveUsed(1));
        AllyMove2.onClick.AddListener(() => OnAllyMoveUsed(2));
        AllyMove3.onClick.AddListener(() => OnAllyMoveUsed(3));
        AllyMove4.onClick.AddListener(() => OnAllyMoveUsed(4));
        AllyMove4.onClick.AddListener(() => OnAllyMoveUsed(4));


    }

    private Dictionary<string, Color> typeColors = new Dictionary<string, Color>()
{
    { "Normal", Color.white},
    { "Electric",new Color32(255, 255, 0, 255) },
    { "Steel", new Color32(212, 206, 199, 255) },
    { "Rock", new Color32(193, 159, 116, 255) },
    { "Water", new Color32(16, 222, 255, 255) },
    { "Psychic", new Color32(255, 102, 118, 255) },
    { "Poison", new Color32(168, 130, 198, 255) },
    { "Ghost", new Color32(185, 93, 255, 255) },
    { "Grass", new Color32(81, 255, 91, 255) },
    { "Fire", new Color32(255, 82, 74, 255) },
    { "Dark", new Color32(135, 135, 135, 255) },
    { "Bug", new Color32(150, 200, 17, 255) },
    { "Flying", new Color32(179, 188, 255, 255) },
    { "Ground", new Color32(255, 189, 71, 255) },
    { "Fairy", new Color32(251, 138, 236, 255) }
};


}


