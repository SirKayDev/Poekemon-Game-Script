using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PokemonCenterInteraction : MonoBehaviour
{
    public GameObject talkText;
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public Transform playerCamera;
    public Transform nurseJoy;
    public PlayerController playerController;
    public PokemonSlotsManager pokemonSlotsManager;

    public GameObject pokemonButton;
    public GameObject bagButton;
    public GameObject settingButton;

    private bool isPlayerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    private bool hasInteracted = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        talkText.SetActive(false);
        dialogueUI.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        SetMenuButtonsVisible(true);
    }

    void Update()
    {
        // Make the talk bubble face the player
        if (talkText.activeSelf)
        {
            talkText.transform.position = nurseJoy.position + Vector3.up * 2;
            talkText.transform.LookAt(playerCamera);
            talkText.transform.Rotate(0, 180, 0);
        }

        // Handle interaction
        if (isPlayerInRange && Input.GetMouseButtonDown(0) && !isDialogueActive && !hasInteracted)
        {
            StartCoroutine(StartDialogueSequence());
        }
        else if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                skipTyping = true;
            }
        }

        // For testing: hurt all Pokémon
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DamageAllPokemon(5);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            talkText.SetActive(true);
            hasInteracted = false;

            // Stop player immediately
            playerController.SetCanMove(false);

            // Hide menu buttons while in range
            SetMenuButtonsVisible(false);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            talkText.SetActive(false);
            hasInteracted = false;
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        isDialogueActive = true;
        hasInteracted = true;
        talkText.SetActive(false);
        playerController.SetCanMove(false);
        SetMenuButtonsVisible(false);

        // Line 1
        ShowDialogueUI("Welcome! To the Pokémon Center.");
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // Line 2
        ShowDialogueUI("I will recover your Pokémon.");
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // Heal
        if (pokemonSlotsManager != null)
        {
            pokemonSlotsManager.HealAllPokemon();
        }

        // Line 3
        ShowDialogueUI("We hope to see you again!");
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        EndDialogue();
    }

    private void ShowDialogueUI(string message)
    {
        dialogueUI.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in message)
        {
            if (skipTyping)
            {
                dialogueText.text = message;
                break;
            }
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        skipTyping = false;
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        isDialogueActive = false;
        playerController.SetCanMove(true);
        SetMenuButtonsVisible(true);
    }

    private void SetMenuButtonsVisible(bool visible)
    {
        if (pokemonButton != null) pokemonButton.SetActive(visible);
        if (bagButton != null) bagButton.SetActive(visible);
        if (settingButton != null) settingButton.SetActive(visible);
    }

    public void DamageAllPokemon(int damageAmount)
    {
        if (pokemonSlotsManager != null)
        {
            pokemonSlotsManager.ReduceHealthAllSlots(damageAmount);
        }
    }
}

