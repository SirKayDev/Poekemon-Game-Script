
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;
using static UnityEngine.Rendering.DebugUI.Table;

[System.Serializable]
public class Pokemon
{
    public string pokemonName;
    public int level;
    public int hp;
    public int attack;
    public int defense;
    public int expRate;
    public int catchRate;
    [SerializeField] private Sprite sprite;

    [System.NonSerialized] public float currentHP; // This is updated during battle
    [System.NonSerialized] public float maxHP;     // Calculated from base stat

    public Pokemon(string pokemonName, int level, int hp, int attack, int defense, int expRate, int catchRate, Sprite sprite = null)
    {
        this.pokemonName = pokemonName;
        this.level = level;
        this.hp = hp;
        this.attack = attack;
        this.defense = defense;
        this.expRate = expRate;
        this.catchRate = catchRate;
        this.sprite = sprite;

        this.maxHP = CalculateMaxHP();      // Calculate once at creation
        this.currentHP = this.maxHP;        // Start full HP
    }

    public Sprite GetSprite() => sprite;

    public float CalculateMaxHP()
    {
        return ((2f * hp * level) / 100f) + level + 10f;
    }
}


[System.Serializable]
public class EnemyTrainer
{
    public string trainerName;
    public List<string> trainerDialogues;
    public List<Pokemon> pokemonTeam;
    public int moneyDrop;

    public EnemyTrainer(string trainerName, List<string> trainerDialogues, List<Pokemon> pokemonTeam, int moneyDrop)
    {
        this.trainerName = trainerName;
        this.trainerDialogues = trainerDialogues;
        this.pokemonTeam = pokemonTeam;
        this.moneyDrop = moneyDrop;
    }   

}


public class EnemyTrainerDatabase : MonoBehaviour
{
    public List<EnemyTrainer> trainers = new List<EnemyTrainer>();

    // Add sprites for Pokémon
    public Sprite weedleSprite;
    public Sprite kakunaSprite;
    public Sprite caterpieSprite;
    public Sprite metapodSprite;
    public Sprite pidgeySprite;
    public Sprite rattataSprite;
    public Sprite ekansSprite;
    public Sprite spearowSprite;
    public Sprite jigglypuffSprite;
    public Sprite magnemiteSprite;
    public Sprite voltorbSprite;
    public Sprite zubatSprite;
    public Sprite clefairySprite;
    public Sprite sandshrewSprite;
    public Sprite geodudeSprite;
    public Sprite onixSprite;
    public Sprite horseaSprite;
    public Sprite shellderSprite;
    public Sprite staryuSprite;
    public Sprite starmieSprite;
    public Sprite grimerSprite;
    public Sprite koffingSprite;
    public Sprite diglettSprite;


    //Player Interaction 
    public GameObject talkText;
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public Transform playerCamera;
    public Transform enemyTrainer;
    public PlayerController playerController;

    // Menu buttons to hide/show
    public GameObject pokemonButton;
    public GameObject bagButton;
    public GameObject settingButton;

    private bool isPlayerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    private Coroutine typingCoroutine;

    private float dialogueCooldown = 1.8f; // 1.8 seconds cooldown
    private float lastDialogueTime = -Mathf.Infinity;

    private int currentRound = 0; // Keep track of the current round in battle
    private List<string> defeatedPokemons = new List<string>(); // To track defeated Pokémon

    private BattleUI battleUI;  // Reference to the Battle UI script

    private Pokemon currentEnemyPokemon;
    private float currentEnemyHP;
    private EnemyTrainer currentTrainer; // New reference for active trainer

    public GameObject brock;  // Reference to Brock's GameObject
    public GameObject misty;  // Reference to Misty's GameObject

    public PokemonSlotsManager pSM;

    public TextMeshProUGUI AllyAttack;
    private string previousAllyAttackText = "";

    public TextMeshProUGUI EnemyHPText;


    void Start()
    {
        brock.SetActive(false);  // Hide Brock initially
        misty.SetActive(false);  // Hide Misty initially

        // Setup UI states here as before...
        talkText.SetActive(false);
        dialogueUI.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        SetMenuButtonsVisible(true);

        battleUI = FindFirstObjectByType<BattleUI>(); // Find the BattleUI component
        pSM = FindFirstObjectByType<PokemonSlotsManager>(); // Find the BattleUI component


        if (trainers == null || trainers.Count == 0)
        {
            InitializeTrainerData();
        }
    }


    void Update()
    {

        // Check if the text changed
        if (AllyAttack.text != previousAllyAttackText)
        {
            previousAllyAttackText = AllyAttack.text;

            // Try to extract a number from the text
            int damage = ExtractDamageFromText(AllyAttack.text);
            if (damage > 0)
            {
                DamageEnemyPokemon(damage);
            }
        }

        if (battleUI.IsBattleActive)
        {
            // Add this:
            if (Input.GetKeyDown(KeyCode.T))
            {
                DamageEnemyPokemon(10f); // Or whatever damage value you want
            }

            return;
        }

        if (talkText.activeSelf)
        {
            talkText.transform.position = enemyTrainer.position + Vector3.up * 2;
            talkText.transform.LookAt(playerCamera);
            talkText.transform.Rotate(0, 180, 0);

            // Update talk text label based on cooldown
            TextMeshProUGUI talkTextTMP = talkText.GetComponentInChildren<TextMeshProUGUI>();
            if (Time.time - lastDialogueTime < dialogueCooldown)
            {
                talkTextTMP.text = "Wait!";
            }
            else
            {
                talkTextTMP.text = "Click to Talk!";
            }
        }

        if (isPlayerInRange && Input.GetMouseButtonDown(0))
        {
            // Check cooldown
            if (Time.time - lastDialogueTime < dialogueCooldown)
                return;

            if (!isDialogueActive)
            {
                StartCoroutine(StartDialogueSequence());
                lastDialogueTime = Time.time; // Start cooldown timer
            }
            else if (isTyping)
            {
                skipTyping = true;
            }
        }


    }

    // This extracts the first number it finds in the string
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

    public void DamageEnemyPokemon(float damage)
    {
        if (currentEnemyPokemon == null)
            return;

        currentEnemyPokemon.currentHP -= damage;
        currentEnemyPokemon.currentHP = Mathf.Clamp(currentEnemyPokemon.currentHP, 0, currentEnemyPokemon.maxHP);
        currentEnemyHP = currentEnemyPokemon.currentHP;

        battleUI.UpdateEnemyHP(currentEnemyHP, currentEnemyPokemon.maxHP);

        // Check for fainting
        if (currentEnemyHP <= 0)
        {
            Debug.Log($"{currentEnemyPokemon.pokemonName} fainted!");
            StartCoroutine(HandleDefeatedPokemon());
        }

    }

    private IEnumerator HandleDefeatedPokemon()
    {
        // Show fainted message
        battleUI.ShowTemporaryMessage($"{currentEnemyPokemon.pokemonName} fainted!", 2.5f);
        yield return new WaitForSeconds(2.5f);

        // Add the defeated Pokémon to the list
        defeatedPokemons.Add(currentEnemyPokemon.pokemonName);

        EnemyTrainer trainer = currentTrainer;

        // Check if all Pokémon of the trainer are defeated
        if (defeatedPokemons.Count == trainer.pokemonTeam.Count)
        {
            // All Pokémon defeated, trainer is defeated
            Debug.Log($"{trainer.trainerName} is completely defeated!");

            // Show a victory message
            battleUI.ShowTemporaryMessage("You defeated the trainer!", 1f);
            yield return new WaitForSeconds(1f);

            // Update money after defeating the trainer
            PokeMartInteraction.playerMoney += trainer.moneyDrop; // Increase player's money
            Bag.Instance.UpdateMoneyUI(PokeMartInteraction.playerMoney); // Update the Bag UI
            FindObjectOfType<PokeMartInteraction>().UpdateMoneyUI(); // Update money UI for PokeMart

            // Hide trainer after defeat
            enemyTrainer.gameObject.SetActive(false);  // Hide the enemy trainer GameObject

            // End battle UI
            battleUI.battleUI.SetActive(false);  // Deactivate the battle UI
            battleUI.ToggleHUDButtons(false);
            battleUI.UnfreezePlayer();  // Unfreeze player after battle is over
            battleUI.EndBattle();
            playerController.SetCanMove(true);  // Allow player movement after defeating the trainer

            // Show Gym Leader if applicable (e.g., after defeating a Jr. Trainer)
            if (trainer.trainerName == "Jr. Trainer Male 1")
            {
                brock.SetActive(true);  // Show Brock after defeating Jr. Trainer Male 1
            }
            else if (trainer.trainerName == "Jr. Trainer Male 2")
            {
                misty.SetActive(true);  // Show Misty after defeating Jr. Trainer Male 2
            }
        }
        else
        {
            // Continue the battle with the next round (if any Pokémon left)
            if (currentRound < trainer.pokemonTeam.Count)
            {
                StartRound(trainer);
            }
            else
            {
                // If no more rounds are left, the trainer is considered defeated
                Debug.Log("All Pokémon defeated!");
                battleUI.SetTextBox("You defeated the trainer!");
                battleUI.ActivateBattle(false); // Hide UI
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            talkText.SetActive(true);
            isPlayerInRange = true;
            playerController.SetCanMove(false);
            SetMenuButtonsVisible(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            talkText.SetActive(false);
            isPlayerInRange = false;
            playerController.SetCanMove(true);
            SetMenuButtonsVisible(true);
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        string trainerTag = enemyTrainer.tag;  // Use the tag instead of name
        EnemyTrainer matchedTrainer = trainers.Find(trainer => trainer.trainerName == trainerTag);

        if (matchedTrainer != null)
        {
            foreach (var line in matchedTrainer.trainerDialogues)
            {
                ShowDialogueUI(line);
                yield return new WaitUntil(() => !isTyping);
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            ShowDialogueUI("Hey! You there!");
            yield return new WaitUntil(() => !isTyping);
            yield return new WaitForSeconds(0.5f);
        }

        // Hide the dialogue UI and start the battle
        HideDialogueUI();
        StartBattle(matchedTrainer);
    }

    private void StartBattle(EnemyTrainer trainer)
    {
        currentTrainer = trainer;

        if (trainer == null)
        {
            Debug.LogError("Trainer is null. Cannot start battle.");
            return;
        }

        currentRound = 0;
        defeatedPokemons.Clear();
        battleUI.SetPokemonCheck("Trainer");

        // Important: reset currentEnemyPokemon to null before battle
        currentEnemyPokemon = null;
        currentEnemyHP = 0;

        battleUI.ActivateBattle(true);
        playerController.SetCanMove(false);
        StartRound(trainer);

        // Check if this is Jr. Trainer Male 1 or Jr. Trainer Male 2 to unlock Gym Leaders
        if (trainer.trainerName == "Jr. Trainer Male 1")
        {
            brock.SetActive(true);  // Unlock Brock
        }
        else if (trainer.trainerName == "Jr. Trainer Male 2")
        {
            misty.SetActive(true);  // Unlock Misty
        }
    }





    private void StartRound(EnemyTrainer trainer)
    {
        if (currentRound < trainer.pokemonTeam.Count)
        {
            currentEnemyPokemon = trainer.pokemonTeam[currentRound];
            currentEnemyHP = currentEnemyPokemon.currentHP;

            // Update the Battle UI with the enemy Pokémon's details
            battleUI.SetupEnemy(
                currentEnemyPokemon.pokemonName,
                currentEnemyPokemon.GetSprite(),
                currentEnemyPokemon.level,
                currentEnemyPokemon.hp,
                currentEnemyHP
            );

            currentRound++;
        }
        else
        {
            // Battle finished
            Debug.Log("All enemy Pokémon defeated!");
            battleUI.SetTextBox("You defeated the trainer!");
            battleUI.ActivateBattle(false); // Hide UI
        }
    }


    private void ShowDialogueUI(string message)
    {
        dialogueUI.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        isDialogueActive = true;
        playerController.SetCanMove(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextLTR(message));
    }

    private IEnumerator TypeTextLTR(string message)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            if (skipTyping)
            {
                dialogueText.text = message;
                break;
            }
            dialogueText.text += message[i];
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        skipTyping = false;
    }

    private void HideDialogueUI()
    {
        dialogueUI.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        isDialogueActive = false;
        playerController.SetCanMove(true);

        // You can add battle trigger logic here later
    }

    private void SetMenuButtonsVisible(bool visible)
    {
        if (pokemonButton != null) pokemonButton.SetActive(visible);
        if (bagButton != null) bagButton.SetActive(visible);
        if (settingButton != null) settingButton.SetActive(visible);
    }


    private void InitializeTrainerData()
    {
        trainers = new List<EnemyTrainer>
    {

         // Route 1
        new EnemyTrainer("Youngster 1", new List<string> { "Hey!", "Don’t think I’ll go easy on you", "just ‘cause I’m young!" }, new List<Pokemon> {
                new Pokemon("Rattata", 3, 30, 56, 35, 30, 255, rattataSprite)
        }, 60),
        new EnemyTrainer("Youngster 2", new List<string> { "I just caught these guys!", "Let’s see how they do!" }, new List<Pokemon> {
                new Pokemon("Rattata", 3, 30, 56, 35, 30, 255, rattataSprite),
                new Pokemon("Pidgey", 3, 30, 56, 35, 30, 255, pidgeySprite)
        }, 30),

        // Route 2
        new EnemyTrainer("Youngster 3", new List<string> { "This route’s mine!", "You’ll have to get through me first!" }, new List<Pokemon> {
            new Pokemon("Rattata", 3, 30, 56, 35, 30, 255, rattataSprite)
        }, 60),
        new EnemyTrainer("Youngster 4", new List<string> { "You’re not getting past without a battle!" }, new List<Pokemon> {
            new Pokemon("Rattata", 4, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Pidgey", 4, 30, 56, 35, 30, 255, pidgeySprite) 
        }, 30),

        // Viridian Forest
        new EnemyTrainer("Bug Catcher 1", new List<string> { "Get ready!", "My bugs are ready to swarm you!" }, new List<Pokemon> {
            new Pokemon("Weedle", 6, 40, 35, 30, 39, 255, weedleSprite) 
        }, 60),
        new EnemyTrainer("Bug Catcher 2", new List<string> { "Don’t underestimate my bugs!", "They’ll make quick work of you!" },  new List<Pokemon> {
            new Pokemon("Caterpie", 7, 45, 30, 35, 39, 255, caterpieSprite)
        }, 60),
        new EnemyTrainer("Bug Catcher 3", new List<string> { "Looks like you’re up against some real bug power now!" }, new List<Pokemon> {
            new Pokemon("Weedle", 7, 40, 35, 30, 39, 255, weedleSprite),
            new Pokemon("Kakuna", 7, 45, 25, 50, 72, 120, kakunaSprite)
        }, 30),
        new EnemyTrainer("Bug Catcher 4", new List<string> { "My bugs won’t go easy on you!", "Are you ready for the challenge?" }, new List<Pokemon> {
            new Pokemon("Caterpie", 6, 45, 30, 35, 39, 255, caterpieSprite),
            new Pokemon("Metapod", 6, 50, 20, 55, 72, 120, metapodSprite)
        }, 30),
        new EnemyTrainer("Bug Catcher 5", new List<string> { "My bugs are stronger than you think.", "Ready to feel their sting?" }, new List<Pokemon> {
            new Pokemon("Metapod", 7, 50, 20, 55, 72, 120, metapodSprite)
        }, 60),

        // Pewter City Gym
        new EnemyTrainer("Jr. Trainer Male 1", new List<string> { "I’ve been training hard!", "Let’s see what you’ve got!" }, new List<Pokemon> {
            new Pokemon("Diglett", 11, 30, 55, 25, 53, 255, diglettSprite),
            new Pokemon("Sandshrew", 11, 50, 75, 85, 60, 255, sandshrewSprite)
        }, 60),
        new EnemyTrainer("Gym Leader Brock", new List<string> { "I’m Brock!", "My rock-hard willpower is evident in my Pokémon!", "Let’s battle!" }, new List<Pokemon> {
            new Pokemon("Geodude", 12, 40, 80, 100, 60, 255, geodudeSprite),
            new Pokemon("Onix", 14, 35, 45, 160, 77, 25, onixSprite)
        }, 720),

        // Route 3
        new EnemyTrainer("Bug Catcher 6", new List<string> { "My bugs are tough, just like me!", "You’re not getting past them!" }, new List<Pokemon> {
            new Pokemon("Caterpie", 10, 45, 30, 35, 39, 255, caterpieSprite),
            new Pokemon("Weedle", 10, 40, 35, 30, 39, 255, weedleSprite)
        }, 30),
        new EnemyTrainer("Bug Catcher 7", new List<string> { "I’m surrounded by bugs.", "No way you’re getting through this!" }, new List<Pokemon> {
            new Pokemon("Weedle", 9, 40, 35, 30, 39, 255, weedleSprite),
            new Pokemon("Kakuna", 9, 45, 25, 50, 72, 120, kakunaSprite),
            new Pokemon("Caterpie", 9, 45, 30, 35, 39, 255, caterpieSprite),
            new Pokemon("Metapod", 9, 50, 20, 55, 72, 120, metapodSprite) 
        }, 15),
        new EnemyTrainer("Youngster 5", new List<string> { "I’m not just a kid..", "you know!", "Let’s see if you can beat me!" },  new List<Pokemon> {
            new Pokemon("Rattata", 11, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Ekans", 11, 35, 60, 44, 58, 255, ekansSprite)
        }, 30),
        new EnemyTrainer("Lass 1", new List<string> { "My Pokémon are as cute as they are strong!", "You won’t win this one!" }, new List<Pokemon> {
            new Pokemon("Pidgey", 9, 40, 45, 40, 50, 255, pidgeySprite),
            new Pokemon("Pidgey", 9, 40, 45, 40, 50, 255, pidgeySprite) 
        }, 60),
        new EnemyTrainer("Bug Catcher 8", new List<string> { "Bug power’s in full force!", "You can’t win this one!" }, new List<Pokemon> {
            new Pokemon("Caterpie", 11, 45, 30, 35, 39, 255, caterpieSprite),
            new Pokemon("Metapod", 11, 50, 20, 55, 72, 120, metapodSprite)
        }, 30),
        new EnemyTrainer("Youngster 6", new List<string> { "My Pokémon and I are unstoppable!", "You’re going down!" }, new List<Pokemon> {
            new Pokemon("Rattata", 10, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Spearow", 10, 40, 60, 30, 58, 255, spearowSprite)
        }, 30),
        new EnemyTrainer("Lass 2", new List<string> { "Think you can beat me?", "My Pokémon are adorable and tough!" }, new List<Pokemon> {
            new Pokemon("Jigglypuff", 14, 115, 45, 20, 95, 190, jigglypuffSprite)
        }, 60),

        // Mt. Moon
        new EnemyTrainer("Bug Catcher 9", new List<string> { "Bug power’s at its peak!", "Ready to feel the sting?" }, new List<Pokemon> {
            new Pokemon("Weedle", 11, 40, 35, 30, 39, 255, weedleSprite),
            new Pokemon("Kakuna", 11, 45, 25, 50, 72, 120, kakunaSprite),
            new Pokemon("Caterpie", 11, 45, 30, 35, 39, 255, caterpieSprite),
            new Pokemon("Metapod", 11, 50, 20, 55, 72, 120, metapodSprite)
        }, 15),
        new EnemyTrainer("Super Nerd 1", new List<string> { "I’ve studied Pokémon for years!", "You don’t stand a chance!" },  new List<Pokemon> {
            new Pokemon("Magnemite", 11, 25, 35, 70, 65, 190, magnemiteSprite),
            new Pokemon("Voltorb", 11, 40, 30, 50, 66, 190, voltorbSprite) 
        }, 90),
        new EnemyTrainer("Lass 3", new List<string> { "My Clefairy’s got more power than you think!", "Let’s do this!" }, new List<Pokemon> {
            new Pokemon("Clefairy", 14, 70, 45, 48, 113, 150, clefairySprite)
        }, 60),
        new EnemyTrainer("Team Rocket Grunt 1", new List<string> { "Heh!", "Prepare for trouble..", "kid!", "This won’t end well for you!" }, new List<Pokemon> {
            new Pokemon("Rattata", 11, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Zubat", 11, 40, 45, 35, 49, 255, zubatSprite) 
        }, 120),
        new EnemyTrainer("Team Rocket Grunt 2", new List<string> { "You’re messing with Team Rocket now!", "Don’t even try to win!"}, new List<Pokemon> {
            new Pokemon("Sandshrew", 11, 50, 75, 85, 60, 255, sandshrewSprite),
            new Pokemon("Rattata", 11, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Zubat", 11, 40, 45, 35, 49, 255, zubatSprite) 
        }, 80),
        new EnemyTrainer("Hiker", new List<string> { "Hah!", "My rock-solid Pokémon will crush you like a pebble!" },  new List<Pokemon> {
            new Pokemon("Geodude", 10, 40, 80, 100, 60, 255, geodudeSprite),
            new Pokemon("Geodude", 10, 40, 80, 100, 60, 255, geodudeSprite),
            new Pokemon("Onix", 10, 35, 45, 160, 77, 25, onixSprite)
        }, 90),
        new EnemyTrainer("Team Rocket Grunt 3", new List<string> { "You think you can stop Team Rocket?", "We’ll show you just how strong we are!" }, new List<Pokemon> {
            new Pokemon("Rattata", 13, 30, 56, 35, 30, 255, rattataSprite)
        }, 240),
        new EnemyTrainer("Super Nerd (Fossil Keeper) 2", new List<string> { "I know everything there is to know about fossils and Pokémon!", "Prepare yourself!" }, new List<Pokemon> {
            new Pokemon("Grimer", 12, 80, 80, 50, 65, 190, grimerSprite),
            new Pokemon("Voltorb", 12, 40, 40, 60, 66, 190, voltorbSprite),
            new Pokemon("Koffing", 12, 40, 60, 95, 65, 190, koffingSprite) 
        }, 60),

        // Route 4
        new EnemyTrainer("Youngster 7", new List<string> { "Don’t blink..", "you might miss my winning move!" }, new List<Pokemon> {
            new Pokemon("Rattata", 14, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Spearow", 14, 40, 60, 30, 58, 255, spearowSprite) 
        }, 30),
        new EnemyTrainer("Youngster 8", new List<string> { "I’m on a roll today!", "Let’s keep it going with a win!" }, new List<Pokemon> {
            new Pokemon("Rattata", 14, 30, 56, 35, 30, 255, rattataSprite),
            new Pokemon("Spearow", 14, 40, 60, 30, 58, 255, spearowSprite)
        }, 30),

            // Cerulean Gym
        new EnemyTrainer("Jr. Trainer Male 2", new List<string> { "I’ve trained under Misty’s waves!", "Get ready to sink!" }, new List<Pokemon> {
            new Pokemon("Horsea", 16, 30, 40, 70, 58, 190, horseaSprite),
            new Pokemon("Shellder", 16, 30, 65, 100, 58, 190, shellderSprite) 
        }, 60),
        new EnemyTrainer("Gym Leader Misty", new List<string> { "I’m Misty!", "You better not take me lightly or..", "you’ll get washed away!" }, new List<Pokemon> {
            new Pokemon("Staryu", 18, 30, 45, 50, 68, 255, staryuSprite),
            new Pokemon("Starmie", 21, 60, 75, 85, 225, 45, starmieSprite) 
        }, 720),
    };

    }

}
