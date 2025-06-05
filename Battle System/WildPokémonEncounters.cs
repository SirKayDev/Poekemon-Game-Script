    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;

    public class WildPokemonEncounters : MonoBehaviour
    {
        public string areaName;
        public int stepsToEncounter = 1;

        private int stepCounter = 0;
        private System.Random random = new System.Random();
        private Coroutine stepTracker;

        public BattleUI battleUI;

        [Header("Wild Pokémon Sprites")]
        public Sprite pidgeySprite;
        public Sprite rattataSprite;
        public Sprite spearowSprite;
        public Sprite caterpieSprite;
        public Sprite weedleSprite;
        public Sprite metapodSprite;
        public Sprite kakunaSprite;
        public Sprite pikachuSprite;
        public Sprite jigglypuffSprite;
        public Sprite zubatSprite;
        public Sprite geodudeSprite;
        public Sprite parasSprite;
        public Sprite clefairySprite;
        public Sprite ekansSprite;
        public Sprite sandshrewSprite;

        private Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();

        [System.Serializable]
        public class Pokemon
        {
            public string name;
            public int hp;
            public int attack;
            public int defense;
            public int expDrop;
            public int catchRate;

            public Pokemon(string name, int hp, int attack, int defense, int expDrop, int catchRate)
            {
                this.name = name;
                this.hp = hp;
                this.attack = attack;
                this.defense = defense;
                this.expDrop = expDrop;
                this.catchRate = catchRate;
            }
        }

        private Dictionary<string, List<(Pokemon, int, float)>> areaPokemonData = new Dictionary<string, List<(Pokemon, int, float)>>()
        {
            { "Route 1", new List<(Pokemon, int, float)> {
                (new Pokemon("Pidgey", 40, 45, 40, 50, 255), 2, 10f),
                (new Pokemon("Pidgey", 40, 45, 40, 50, 255), 3, 19f),
                (new Pokemon("Rattata", 30, 56, 35, 30, 255), 2, 10f),
                (new Pokemon("Rattata", 30, 56, 35, 30, 255), 3, 36f),
                (new Pokemon("Spearow", 40, 60, 30, 52, 255), 3, 7f),
                (new Pokemon("Spearow", 40, 60, 30, 52, 255), 5, 7f)
            } },
            { "Route 2", new List<(Pokemon, int, float)> {
                (new Pokemon("Rattata", 30, 56, 35, 30, 255), 2, 12f),
                (new Pokemon("Pidgey", 40, 45, 40, 50, 255), 2, 15f),
                (new Pokemon("Caterpie", 45, 30, 35, 39, 255), 3, 8f),
                (new Pokemon("Weedle", 40, 35, 30, 39, 255), 3, 8f)
            } },
            { "Viridian Forest", new List<(Pokemon, int, float)> {
                (new Pokemon("Caterpie", 45, 30, 35, 39, 255), 3, 15f),
                (new Pokemon("Metapod", 50, 20, 55, 72, 120), 4, 8f),
                (new Pokemon("Weedle", 40, 35, 30, 39, 255), 3, 15f),
                (new Pokemon("Kakuna", 45, 25, 50, 72, 120), 4, 8f),
                (new Pokemon("Pikachu", 35, 55, 35, 112, 190), 3, 5f)
            } },
            { "Route 3", new List<(Pokemon, int, float)> {
                (new Pokemon("Pidgey", 40, 45, 40, 50, 255), 6, 20f),
                (new Pokemon("Spearow", 40, 60, 30, 52, 255), 5, 10f),
                (new Pokemon("Jigglypuff", 115, 45, 20, 95, 190), 3, 10f),
                (new Pokemon("Rattata", 30, 56, 35, 30, 255), 4, 10f)
            } },
            { "Mt. Moon", new List<(Pokemon, int, float)> {
                (new Pokemon("Zubat", 40, 45, 35, 49, 255), 6, 22f),
                (new Pokemon("Geodude", 40, 80, 100, 60, 255), 7, 20f),
                (new Pokemon("Paras", 35, 70, 55, 58, 255), 10, 5f)
            } },
            { "Route 4", new List<(Pokemon, int, float)> {
                (new Pokemon("Rattata", 30, 56, 35, 30, 255), 6, 20f),
                (new Pokemon("Spearow", 40, 60, 30, 52, 255), 6, 10f),
                (new Pokemon("Sandshrew", 50, 75, 85, 60, 255), 6, 10f)
            } }
        };

        private void Start()
        {
            spriteLookup["Pidgey"] = pidgeySprite;
            spriteLookup["Rattata"] = rattataSprite;
            spriteLookup["Spearow"] = spearowSprite;
            spriteLookup["Caterpie"] = caterpieSprite;
            spriteLookup["Weedle"] = weedleSprite;
            spriteLookup["Metapod"] = metapodSprite;
            spriteLookup["Kakuna"] = kakunaSprite;
            spriteLookup["Pikachu"] = pikachuSprite;
            spriteLookup["Jigglypuff"] = jigglypuffSprite;
            spriteLookup["Zubat"] = zubatSprite;
            spriteLookup["Geodude"] = geodudeSprite;
            spriteLookup["Paras"] = parasSprite;
            spriteLookup["Clefairy"] = clefairySprite;
            spriteLookup["Ekans"] = ekansSprite;
            spriteLookup["Sandshrew"] = sandshrewSprite;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (stepTracker == null)
                    stepTracker = StartCoroutine(TrackSteps());

                TryTriggerEncounter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (stepTracker != null)
                {
                    StopCoroutine(stepTracker);
                    stepTracker = null;
                }

                TryTriggerEncounter();
            }
        }

        private IEnumerator TrackSteps()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                stepCounter++;

                if (stepCounter >= stepsToEncounter)
                {
                    stepCounter = 0;
                    TryTriggerEncounter();
                }
            }
        }

        private void TryTriggerEncounter()
        {
            if (battleUI != null && battleUI.IsBattleActive)
                return;

            if (random.Next(0, 100) < 10)
            {
                TriggerEncounter();
            }
        }

        private void TriggerEncounter()
        {
            if (!areaPokemonData.ContainsKey(areaName)) return;

            float roll = (float)random.NextDouble() * 100;
            float cumulative = 0f;

            foreach (var (pokemon, level, chance) in areaPokemonData[areaName])
            {
                cumulative += chance;
                if (roll < cumulative)
                {
                    StartCoroutine(SetupBattleUI(pokemon, level));
                    break;
                }
            }
        }

        private IEnumerator SetupBattleUI(Pokemon pokemon, int level)
        {
            battleUI.ActivateBattle();
            battleUI.SetPokemonCheck("Wild Pokémon");
            yield return null;

            int realHP = CalculateRealHP(pokemon.hp, level);

            if (spriteLookup.TryGetValue(pokemon.name, out Sprite sprite))
            {
                // Use SetupEnemy method from BattleUI to set everything properly
                battleUI.SetupEnemy(pokemon.name, sprite, level, realHP, realHP);
            }
        }

        private int CalculateRealHP(int baseHP, int level)
        {
            return (int)((2f * baseHP * level) / 100f) + level + 10;
        }
    }

