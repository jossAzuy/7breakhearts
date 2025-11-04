using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleShowInfoUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Image itemIconImage;

    public void Show(CollectibleSO itemData)
    {
        if (itemData != null)
        {
            itemNameText.text = itemData.itemName;
            itemDescriptionText.text = itemData.ItemDescription;
            ///itemIconImage.sprite = itemData.icon;
            gameObject.SetActive(true);
        }
       /*  else
        {
            Hide();
        } */
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
