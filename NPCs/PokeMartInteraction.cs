using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PokeMartInteraction : MonoBehaviour
{
    [Header("General References")]
    public GameObject talkText;
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public Transform playerCamera;
    public Transform clerk;
    public PlayerController playerController;

    [Header("Shop UI")]
    public GameObject shopUI;
    public TextMeshProUGUI playerMoneyText;

    [Header("Item Buttons")]
    public Button potionButton;
    public Button superPotionButton;
    public Button pokeBallButton;
    public Button greatBallButton;

    [Header("Text Selection")]
    public GameObject selectionBoxUI;
    public TextMeshProUGUI selectionText;

    [Header("Number Entry")]
    public Button enterNumberButton;
    public TextMeshProUGUI enterNumberText;
    public Button buyButton;
    public Button cancelButton;

    private bool isPlayerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    private Coroutine typingCoroutine;

    private string selectedItem = "";
    private int selectedPrice = 0;
    private int selectedQuantity = 1;
    private string currentTypedString = "";
    private bool typingNumber = false;
    public static int playerMoney = 3000;

    private Color defaultButtonColor;
    private Button currentSelectedButton;

    private Coroutine messageCoroutine;

    private Color enterNumberActiveColor = new Color(1f, 0.89f, 0.082f); // FFE315
    private Color enterNumberInactiveColor = new Color(1f, 1f, 1f); // FFFFFF
    private Color confirmButtonColor = new Color(0f, 1f, 0f); // Green

    public GameObject pokemonButton;
    public GameObject bagButton;
    public GameObject settingButton;


    void Start()
    {
        ResetAllUI();

        potionButton.onClick.AddListener(() => SelectItem("Potion", 300, potionButton));
        superPotionButton.onClick.AddListener(() => SelectItem("Super Potion", 700, superPotionButton));
        pokeBallButton.onClick.AddListener(() => SelectItem("Poké Ball", 200, pokeBallButton));
        greatBallButton.onClick.AddListener(() => SelectItem("Great Ball", 600, greatBallButton));

        enterNumberButton.onClick.AddListener(() => ToggleNumberTyping());
        buyButton.onClick.AddListener(() => ConfirmPurchase());
        cancelButton.onClick.AddListener(() => CancelPurchase());

        defaultButtonColor = potionButton.image.color;
        UpdateMoneyUI();
        ResetSelection();

        SetMenuButtonsVisible(true);

    }


    void Update()
    {
        if (talkText.activeSelf)
        {
            talkText.transform.position = clerk.position + Vector3.up * 2;
            talkText.transform.LookAt(playerCamera);
            talkText.transform.Rotate(0, 180, 0);
        }

        if (isPlayerInRange && Input.GetMouseButtonDown(0))
        {
            if (!shopUI.activeSelf)
            {
                if (!isDialogueActive)
                    StartCoroutine(StartDialogueSequence());
                else if (isTyping)
                    skipTyping = true;
                else
                    OpenShopUI();
            }
        }

        if (typingNumber)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsDigit(c)) currentTypedString += c;
                else if (c == '\b' && currentTypedString.Length > 0)
                    currentTypedString = currentTypedString.Substring(0, currentTypedString.Length - 1);
                else if (c == '\n' || c == '\r')
                    FinalizeNumberInput();
            }

            if (string.IsNullOrEmpty(currentTypedString))
            {
                enterNumberText.text = "Enter the number";
                selectedQuantity = 1;
            }
            else
            {
                enterNumberText.text = currentTypedString;
                if (int.TryParse(currentTypedString, out int parsed))
                    selectedQuantity = Mathf.Clamp(parsed, 1, 99);
            }

            UpdateSelectionText();
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        ShowDialogue("Welcome! How can I help you?");
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(0.5f);
        ShowDialogue("Please select your item.");
    }

    private void ShowDialogue(string message)
    {
        if (shopUI.activeSelf)
        {
            ForceHideDialogue();
            return;
        }

        dialogueUI.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        isDialogueActive = true;
        playerController.SetCanMove(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
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

    private void ForceHideDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = "";
        dialogueUI.SetActive(false);
        isDialogueActive = false;
        isTyping = false;
        skipTyping = false;
    }

    private void OpenShopUI()
    {
        ForceHideDialogue();
        shopUI.SetActive(true);
        selectionBoxUI.SetActive(true);
        isDialogueActive = false;
    }

    private void SelectItem(string itemName, int price, Button button)
    {
        if (selectedItem == itemName)
        {
            ResetSelection(); // Toggle off
            return;
        }

        selectedItem = itemName;
        selectedPrice = price;
        selectedQuantity = Mathf.Clamp(selectedQuantity, 1, 99);

        if (currentSelectedButton != null)
            currentSelectedButton.image.color = defaultButtonColor;

        button.image.color = new Color(0.973f, 0.89f, 0.306f);
        currentSelectedButton = button;

        UpdateSelectionText();
    }

    private void UpdateSelectionText()
    {
        if (string.IsNullOrEmpty(selectedItem))
            selectionText.text = "Please select your item.";
        else
            selectionText.text = $"Would you like to buy {selectedQuantity} {selectedItem}(s)?";
    }

    private void ToggleNumberTyping()
    {
        if (string.IsNullOrEmpty(selectedItem))
        {
            if (messageCoroutine != null)
                StopCoroutine(messageCoroutine);

            messageCoroutine = StartCoroutine(ShowTemporaryMessage("Please select an item.", 3f));
            return;
        }

        if (!typingNumber)
        {
            typingNumber = true;
            currentTypedString = "";
            enterNumberText.text = "Enter the number";
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            enterNumberButton.image.color = enterNumberActiveColor;
            selectionText.text = "Press 'Enter' to confirm";
            buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm";
            buyButton.image.color = confirmButtonColor;
        }
        else
        {
            FinalizeNumberInput();
        }
    }

    private void FinalizeNumberInput()
    {
        typingNumber = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        enterNumberButton.image.color = enterNumberInactiveColor;

        if (int.TryParse(currentTypedString, out int result))
        {
            selectedQuantity = Mathf.Clamp(result, 1, 99);
            enterNumberText.text = selectedQuantity.ToString();
        }
        else
        {
            selectedQuantity = 1;
            enterNumberText.text = "Enter the number";
        }

        UpdateSelectionText();
    }

    private void ConfirmPurchase()
    {
        // Check if an item is selected and the button is in "Confirm" state
        if (string.IsNullOrEmpty(selectedItem) || buyButton.GetComponentInChildren<TextMeshProUGUI>().text != "Confirm")
            return;

        // Calculate the total cost based on the price and selected quantity
        int totalCost = selectedPrice * selectedQuantity;

        // Check if the player has enough money
        if (playerMoney >= totalCost)
        {
            // Deduct the total cost from the player's money
            playerMoney -= totalCost;

            // Update the money UI in both the Poké Mart and the Bag UI
            UpdateMoneyUI();

            // Add the purchased item(s) to the Bag UI
            if (Bag.Instance != null)
                Bag.Instance.AddItem(selectedItem, selectedQuantity);

            // Log the purchase for debugging
            Debug.Log($"Purchased {selectedQuantity} {selectedItem}(s) for ¥{totalCost}!");

            // Finalize the purchase and reset the selection
            FinalizePurchase(true);
        }
        else
        {
            // If player doesn't have enough money, show a message
            if (messageCoroutine != null)
                StopCoroutine(messageCoroutine);

            messageCoroutine = StartCoroutine(ShowTemporaryInsufficientFundsMessage("You have insufficient money to buy this item(s).", 3f));
        }
    }


    private IEnumerator ShowTemporaryInsufficientFundsMessage(string message, float duration)
    {
        string previousMessage = selectionText.text;
        selectionText.text = message;

        // Reset relevant parts of UI
        typingNumber = false;
        currentTypedString = "";
        enterNumberText.text = "Enter the number";
        enterNumberButton.image.color = enterNumberInactiveColor;
        buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
        buyButton.image.color = Color.white;

        yield return new WaitForSeconds(duration);

        // Fully reset item selection (back to starting state)
        ResetSelection();
    }

    private void FinalizePurchase(bool wasSuccessful)
    {
        typingNumber = false;
        currentTypedString = "";
        enterNumberText.text = "Enter the number";
        enterNumberButton.image.color = enterNumberInactiveColor;
        buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
        buyButton.image.color = Color.white;

        if (wasSuccessful)
            ResetSelection();
        else
            UpdateSelectionText(); // keeps item info
    }

    private void CancelPurchase()
    {
        ResetAllUI();
        playerController.SetCanMove(true);
    }

    private void ResetAllUI()
    {
        StopAllCoroutines();
        shopUI.SetActive(false);
        dialogueUI.SetActive(false);
        selectionBoxUI.SetActive(false);
        talkText.SetActive(false);
        dialogueText.text = "";

        isDialogueActive = false;
        isTyping = false;
        skipTyping = false;
        typingNumber = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        enterNumberButton.image.color = enterNumberInactiveColor;

        ResetSelection();
    }

    public void UpdateMoneyUI()
    {
        playerMoneyText.text = $"¥{playerMoney}";
        if (Bag.Instance != null)
            Bag.Instance.UpdateMoneyUI(playerMoney);
    }

    private void ResetSelection()
    {
        selectedItem = "";
        selectedPrice = 0;
        selectedQuantity = 1;
        currentTypedString = "";
        enterNumberText.text = "Enter the number";
        selectionText.text = "Please select your item.";
        ResetButtonColor();
    }

    private void ResetButtonColor()
    {
        if (currentSelectedButton != null)
        {
            currentSelectedButton.image.color = defaultButtonColor;
            currentSelectedButton = null;
        }

        enterNumberButton.image.color = enterNumberInactiveColor;
    }

    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        string previousMessage = selectionText.text;
        selectionText.text = message;
        yield return new WaitForSeconds(duration);
        UpdateSelectionText(); // Restore proper prompt
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
            isPlayerInRange = false;
            ResetAllUI();
            playerController.SetCanMove(true);
            SetMenuButtonsVisible(true);

        }
    }

    private void SetMenuButtonsVisible(bool visible)
    {
        if (pokemonButton != null) pokemonButton.SetActive(visible);
        if (bagButton != null) bagButton.SetActive(visible);
        if (settingButton != null) settingButton.SetActive(visible);
    }


}
