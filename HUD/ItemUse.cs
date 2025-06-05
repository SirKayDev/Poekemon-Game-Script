using UnityEngine;
using UnityEngine.UI;

public class ItemUse : MonoBehaviour
{
    public Button potionButton;
    public Button superPotionButton;

    void Start()
    {
        potionButton.onClick.AddListener(OnPotionClicked);
        superPotionButton.onClick.AddListener(OnSuperPotionClicked);
    }

    void OnPotionClicked()
    {
        Debug.Log("Potion button clicked");
    }

    void OnSuperPotionClicked()
    {
        Debug.Log("Super Potion button clicked");
    }
}

