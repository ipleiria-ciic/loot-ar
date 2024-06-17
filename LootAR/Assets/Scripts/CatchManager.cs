using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CatchManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text usernameText;
    [SerializeField] private Text xpText;
    [SerializeField] private Text levelText;
    
    [Header("Buttons")]
    [SerializeField] private AudioClip itemSound;
    [SerializeField] private GameObject inventoryButton;
    [SerializeField] private GameObject catchItemScreen;
    [SerializeField] private GameObject cannotCatchScreen;
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private GameObject inventoryBackBtn;
    [SerializeField] private GameObject catchButton;
    [SerializeField] private GameObject dismissButton;

    [Header("Debug Elements")]
    [SerializeField] private Text debugText;
    
    [Header("Game Objects")]
    [SerializeField] private GameObject player;
 
    [Header("Inventory Elements")]
    [SerializeField] private Text numWoodText;
    [SerializeField] private Text numClothText;
    [SerializeField] private Text numMetalText;
    [SerializeField] private Text numFoodText;
    
    [Header("Catch Screen Elements")]
    [SerializeField] private Text catchItemText;
    [SerializeField] private Text cannotCatchItemText;

    [Header("Auxiliary Variables")]
    private AudioSource audioSource;
    private int numWoodCaught;
    private int numClothCaught;
    private int numMetalCaught;
    private int numFoodCaught;
    private Item selectedItem; 
    private int randomAmount;
    
    private void Awake()
    {
        //initialize components
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // Continuously update player's XP and level information
        updateLevel();
        updateXP();
    }

    //function to inform the player that the item cannot be caught
    //because the player is not in AR mode
    public void CannotCatchScreen(Item Item)
    {
        // Handle logic when item cannot be caught in non-AR mode
        //item clicked will be the selected item
        selectedItem = Item;
        audioSource.PlayOneShot(itemSound);
        //the cannot catch screen will be displayed
        cannotCatchScreen.SetActive(true);
        //the text will be displayed
        cannotCatchItemText.text = "You cannot catch " + selectedItem.GetItemName + " unless in AR mode!";
        
        //calculate the distance between the player and the item for debug purposes
        float distance = Vector3.Distance(player.transform.position, selectedItem.transform.position);
        debugText.text = "Position" + selectedItem.transform.position + "Player" + player.transform.position + "Distance" + distance;
    }

    //function to inform the player that the item cannot be caught
    //because the player is too far away
    public void CannotCatchScreenItemToFarAway(Item item)
    {
        // Handle logic when item cannot be caught because player is too far away
        //item clicked will be the selected item
        selectedItem = item;
        audioSource.PlayOneShot(itemSound);
        //the cannot catch screen will be displayed
        cannotCatchScreen.SetActive(true);
        //the text will be displayed
        cannotCatchItemText.text = "You are too far away to catch " + selectedItem.GetItemName + "! Get closer to the item!";
    }
    
        
    //function to dismiss the cannot catch screen
    public void DismissCannotCatchScreen()
    {
        //if the cannot catch screen is active, it will be dismissed
        if (cannotCatchScreen.activeSelf)
        {
            audioSource.PlayOneShot(itemSound);
            //the cannot catch screen will be dismissed
            cannotCatchScreen.SetActive(false);
        }
    }

    //function to catch the item
    public void CatchItemScreen(Item Item)
    {
        // Handle logic when item can be caught
        //item clicked will be the selected item
        selectedItem = Item;
        audioSource.PlayOneShot(itemSound);
        //a random amount of the item will be generated
        randomAmount = GetRamdomAmount();
        //the catch item screen will be displayed
        catchItemScreen.SetActive(true);
        //the text will be displayed
        catchItemText.text = "Do you want to catch " + randomAmount + " of " + selectedItem.GetItemName + "?";
    }

    //function to catch the item
    public void CatchItem()
    {
        //if the catch item screen is active, the item will be caught
        if (catchItemScreen.activeSelf)
        {

            //get the item name from the item that was clicked
            string itemName = selectedItem.GetItemName;
            Debug.Log("Item caught: " + itemName);

            audioSource.PlayOneShot(itemSound);

            //add the item to the inventory
            Item caughtItem = new Item { itemName = itemName, itemQuantity = randomAmount };
            GameManager.Instance.CurrentPlayer.AddItems(caughtItem);

            //update the inventory quantoity
            int quantity = GameManager.Instance.CurrentPlayer.GetItems.Find(x => x.GetItemName == itemName).ItemQuantity;
            
            //update the inventory text
            switch (itemName)
            {
                case "Wood":
                    numWoodText.text = itemName + " (" + quantity.ToString() + ")";
                    break;
                case "Cloth":
                    numClothText.text = itemName + " (" + quantity.ToString() + ")";
                    break;
                case "Metal":
                    numMetalText.text = itemName + " (" + quantity.ToString() + ")";
                    break;
                case "Food":
                    numFoodText.text = itemName + " (" + quantity.ToString() + ")";
                    break;
            }

            //calculate the XP earned
            int xpEarned = CalculateXPEarned(randomAmount);
            GameManager.Instance.CurrentPlayer.AddXP(xpEarned);   

            //save the data
            DataManager.SaveData(GameManager.Instance.CurrentPlayer);

            //destroy the item
            Spawner.Instance.DestroyItem(selectedItem);
            catchItemScreen.SetActive(false);
            
        }
    }

    //function to dismiss the catch item screen
    public void DismissItem()
    {
        //if the catch item screen is active, it will be dismissed
        if (catchItemScreen.activeSelf) 
        {
            audioSource.PlayOneShot(itemSound);
            //the catch item screen will be dismissed
            catchItemScreen.SetActive(false);
        }
    }

    //function to toggle the inventory screen
    public void toggleInventory()
    {
        audioSource.PlayOneShot(itemSound);
        //the inventory screen will be displayed
        inventoryScreen.SetActive(!inventoryScreen.activeSelf);
    }

    //function to close the inventory screen
    public void toggleInventoryBackBtn()
    {
        audioSource.PlayOneShot(itemSound);
        //if the inventory screen is active, it will be closed
        if (inventoryScreen.activeSelf)
        {
            
            //the inventory screen will be closed
            inventoryScreen.SetActive(false);
        }
    }

    //function to update the XP
    public void updateXP()
    {
        xpText.text = GameManager.Instance.CurrentPlayer.GetXP.ToString();
    }

    //function to update the level
    public void updateLevel()
    {
        levelText.text = GameManager.Instance.CurrentPlayer.GetLevel.ToString();
    }

    //function to update the username
    public void updateUsername()
    {
        usernameText.text = GameManager.Instance.CurrentPlayer.GetUsername;
    }
    
    //function to get a random amount of the item
    private int GetRamdomAmount()
    {
        int amount = 1;
        amount = GetRandomAmountProbabilities(new float[] { 0.7f, 0.4f, 0.2f, 0.1f, 0.05f }, new int[] { 1, 2, 3, 4, 5 });

        return amount;
    }

    //function to randomize the probabilities of the amount of the item
    private int GetRandomAmountProbabilities(float[] probabilities, int[] amounts)
    {
        if (probabilities.Length != amounts.Length)
        {
            return 1;
        }

        float randomValue = UnityEngine.Random.value;
        float comulativeProbability = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            comulativeProbability += probabilities[i];
            if (randomValue < comulativeProbability)
            {
                return amounts[i];
            }
        }

        return 1;
    }

    //function to calculate the XP earned
    private int CalculateXPEarned(int amount)
    {
        int baseXP = 10;
        float xpMultiplier = 1.5f;
        int xpEarned = (int)(baseXP * (Mathf.Pow(xpMultiplier, amount) - 1) / (xpMultiplier - 1));

        return xpEarned;
    }
}
