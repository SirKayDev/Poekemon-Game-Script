using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class AreaSound
    {
        public string areaName;
        public AudioClip musicClip;
    }

    public List<AreaSound> areaSounds;
    public GameObject battleUI;
    public AudioClip battleMusic;

    [Range(0f, 1f)]
    public float musicVolume = 0.1f;

    private AudioSource audioSource;
    private string currentArea = "";
    private bool isInBattle = false;
    private bool playerInArea = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Only allow one instance of SoundManager
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = musicVolume;
    }

    void Update()
    {
        audioSource.volume = musicVolume;

        // Check if battle UI is active and handle battle music
        if (battleUI != null)
        {
            bool battleActive = battleUI.activeInHierarchy;

            if (battleActive && !isInBattle)
            {
                // Switch to battle music
                isInBattle = true;
                audioSource.Stop();
                audioSource.clip = battleMusic;
                audioSource.Play();
            }
            else if (!battleActive && isInBattle)
            {
                // Exit battle, revert to area music
                isInBattle = false;
                if (playerInArea)
                    PlayAreaMusic(currentArea);
                else
                    audioSource.Stop();
            }
        }
    }

    public void EnterArea(string areaName)
    {
        playerInArea = true;
        currentArea = areaName;

        if (!isInBattle)
            PlayAreaMusic(areaName);
    }

    public void ExitArea(string areaName)
    {
        if (currentArea == areaName)
        {
            playerInArea = false;
            currentArea = "";

            if (!isInBattle)
                audioSource.Stop();
        }
    }

    private void PlayAreaMusic(string areaName)
    {
        AreaSound area = areaSounds.Find(a => a.areaName == areaName);
        if (area != null && area.musicClip != null)
        {
            audioSource.clip = area.musicClip;
            audioSource.Play();
        }
    }
}

