using System.Collections;
using System.Collections.Generic;
using Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [Header("Game Objects")]
    [SerializeField] private Player currentPlayer;
    [SerializeField] private List<GameObject> itemPrefabs;
    
    [Header("Item Prefabs")]
    private Dictionary<string, GameObject> itemPrefabDictionary;

    [Header("Inventory Elements")]
    [SerializeField] private Text numWoodText;
    [SerializeField] private Text numClothText;
    [SerializeField] private Text numMetalText;
    [SerializeField] private Text numFoodText;

    
    // Public property to access the current player externally.
    public Player CurrentPlayer { get { return currentPlayer; } }

    private void Awake()
    {
        // Ensure the currentPlayer is not null.
        Assert.IsNotNull(currentPlayer, "Current player is null");

        // Initialize the dictionary to store item prefabs.
        itemPrefabDictionary = new Dictionary<string, GameObject>();
        foreach (var prefab in itemPrefabs)
        {
            Item item = prefab.GetComponent<Item>();
            if (item != null && !itemPrefabDictionary.ContainsKey(item.GetItemName))
            {
                itemPrefabDictionary.Add(item.GetItemName, prefab);
            }
        }
    }

    private void Start()
    {
        // Load player data when the game starts.
        LoadData();
    }

    public void LoadData()
    {
        // Load player data from a persistent source.
        PlayerData playerData = DataManager.LoadData();
        if (playerData != null)
        {
            // Update player's XP based on loaded data.
            currentPlayer.AddXP(playerData.xp);
            foreach (ItemData itemData in playerData.items)
            {
                // Instantiate item prefabs and add them to the player's inventory.
               if (itemPrefabDictionary.TryGetValue(itemData.itemName, out GameObject itemPrefab))
                {
                    // Instantiate the item prefab and add it to the player's inventory.
                    currentPlayer.UpdateInventory(itemData.itemName, itemData.itemQuantity);
                }
                else
                {
                    Debug.LogWarning("Prefab for item " + itemData.itemName + " not found.");
                } 
                
                UpdateItemTextUI(itemData.itemName, itemData.itemQuantity);
            }
        }
    }

    public void UpdateItemTextUI(string itemName, int quantity)
    {
        // Update the UI text for the specified item.
        switch (itemName)
        {
            case "Wood":
                numWoodText.text = $"Wood ({quantity})";
                break;
            case "Cloth":
                numClothText.text = $"Cloth ({quantity})";
                break;
            case "Metal":
                numMetalText.text = $"Metal ({quantity})";
                break;
            case "Food":
                numFoodText.text = $"Food ({quantity})";
                break;
        }
    }
}
