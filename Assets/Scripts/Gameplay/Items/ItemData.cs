using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Thông số")]
    public float value;
}