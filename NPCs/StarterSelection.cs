using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StarterSelection : MonoBehaviour
{
    public GameObject talkText;
    public GameObject selectionUI;
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public Button charmanderButton, bulbasaurButton, squirtleButton;
    public Transform playerCamera;
    public Transform professorOak;
    public PokemonSlotsManager pokemonSlotsManager;
    public PlayerController playerController;
    public Sprite charmanderSprite, bulbasaurSprite, squirtleSprite;

    public GameObject barrier; // <-- New reference to the Barrier GameObject

    private bool isPlayerInRange = false;
    private bool isDialogueActive = false;
    private bool isPokemonSelected = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    private Coroutine typingCoroutine;

    public GameObject pokemonButton;
    public GameObject bagButton;
    public GameObject settingButton;

    private float cooldownTime = 1.8f; // Cooldown duration (1.8 seconds)
    private float lastClickTime = -Mathf.Infinity; // Time of the last click

    void Start()
    {
        talkText.SetActive(false);
        selectionUI.SetActive(false);
        dialogueUI.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        SetMenuButtonsVisible(true);

        charmanderButton.onClick.AddListener(() => SelectStarter("Charmander", charmanderSprite, 39));
        bulbasaurButton.onClick.AddListener(() => SelectStarter("Bulbasaur", bulbasaurSprite, 45));
        squirtleButton.onClick.AddListener(() => SelectStarter("Squirtle", squirtleSprite, 44));
    }

    void Update()
    {
        if (talkText.activeSelf)
        {
            talkText.transform.position = professorOak.position + Vector3.up * 2;
            talkText.transform.LookAt(playerCamera);
            talkText.transform.Rotate(0, 180, 0);

            // Update talk text label based on cooldown
            TextMeshProUGUI talkTextTMP = talkText.GetComponentInChildren<TextMeshProUGUI>();
            if (Time.time - lastClickTime < cooldownTime)
            {
                talkTextTMP.text = "Wait!";
            }
            else
            {
                talkTextTMP.text = "Click to Talk!";
            }
        }

        // Handle input only if enough time has passed
        if (isPlayerInRange && Input.GetMouseButtonDown(0))
        {
            // Only allow interaction if the cooldown has passed
            if (Time.time - lastClickTime >= cooldownTime)
            {
                lastClickTime = Time.time; // Set the last click time to now

                if (!isDialogueActive && !isPokemonSelected)
                {
                    ShowStarterSelectionUI();
                }
                else if (isDialogueActive)
                {
                    if (isTyping)
                    {
                        skipTyping = true;
                    }
                    else
                    {
                        HideDialogueUI();
                    }
                }
                else if (!isDialogueActive && isPokemonSelected)
                {
                    ShowDialogueUI("Good luck with your journey!");
                }
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

            if (!isPokemonSelected)
            {
                ShowStarterSelectionUI();
            }
            else if (!isDialogueActive)
            {
                ShowDialogueUI("Good luck with your journey!");
            }
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

    private void ShowStarterSelectionUI()
    {
        selectionUI.SetActive(true);
    }

    private void HideStarterSelectionUI()
    {
        selectionUI.SetActive(false);
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
    }

    private void SelectStarter(string name, Sprite sprite, int baseHP)
    {
        if (pokemonSlotsManager != null)
        {
            pokemonSlotsManager.AddPokemonToSlot(name, sprite, baseHP, 0);
        }

        HideStarterSelectionUI();
        isPokemonSelected = true;
        charmanderButton.interactable = false;
        bulbasaurButton.interactable = false;
        squirtleButton.interactable = false;

        // ?? Destroy the Barrier once a starter is selected
        if (barrier != null)
        {
            Destroy(barrier);
        }

        ShowDialogueUI("Congratulations! You have selected your Pokémon!");
    }

    private void SetMenuButtonsVisible(bool visible)
    {
        if (pokemonButton != null) pokemonButton.SetActive(visible);
        if (bagButton != null) bagButton.SetActive(visible);
        if (settingButton != null) settingButton.SetActive(visible);
    }
}

