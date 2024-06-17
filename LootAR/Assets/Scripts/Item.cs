using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    
    [Header("Item Attributes")]
    [SerializeField] public string itemName;
    [SerializeField] public int itemQuantity;
    [SerializeField] private AudioClip itemSound;
    
    [Header("Game Objects")]
    [SerializeField] private CatchManager catchManager;
    [SerializeField] private Text debugText;
    [SerializeField] private GameObject arCamera;
    [SerializeField] private GameObject player;
    
    private Vector3 playerPosition;

    private AudioSource audioSource;

    private void Awake()
    {
        // Initialize components and validate necessary references
        playerPosition = player.transform.position;
        //playerPosition = transform.TransformPoint(player.transform.position);
        arCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        audioSource = GetComponent<AudioSource>();
        Assert.IsNotNull(audioSource);
        Assert.IsNotNull(itemSound);
        
    }

    // Properties for accessing and setting item data
    public string GetItemName
    { get { return itemName; } }
    public string SetItemName
    { set { itemName = value; } }
    public AudioClip GetItemSound
    { get { return itemSound; } }
    public int ItemQuantity
    { get { return itemQuantity; } set { itemQuantity = value; } }
    public Vector2d GeographicLocation { get; set; }

    private void Start()
    {
        //playerPosition = player.transform.position;
        playerPosition = player.transform.TransformPoint(player.transform.position);
        arCamera.transform.TransformPoint(playerPosition);
        //arCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        // Update the AR camera's position to follow the player each frame
        playerPosition = player.transform.position;
        //playerPosition = player.transform.TransformPoint(player.transform.position);
        Debug.Log("Player position: " + playerPosition);
        //arCamera.transform.TransformPoint(playerPosition);
        arCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        Debug.Log("AR Camera position: " + arCamera.transform.position);
    }

    public void OnMouseDown()
    {
        //GameObject arCamera = GameObject.Find("Main Camera AR");
        playerPosition = player.transform.position;
        arCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        audioSource.PlayOneShot(itemSound);
        //catchManager = Object.FindObjectOfType<CatchManager>();
        Debug.Log(catchManager.gameObject.name);

        // Handler for when the item is clicked
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Check for the catch manager and AR camera state
        if (catchManager != null)
        {
            if (arCamera != null && arCamera.activeSelf)
            {
                // Calculate distance between AR camera and the item
                float maxRayDistance = 15f;
                float distance = Vector3.Distance(arCamera.transform.position, transform.position);
                
                Debug.Log("Distance: " + distance);
                if (distance <= maxRayDistance)
                {
                    // If within range, trigger the catch item screen
                    catchManager.CatchItemScreen(this);
                    Debug.Log("Item clicked");
                    
                    // Calculate distance between player and item for debug purposes
                    distance = Vector3.Distance(arCamera.transform.position, transform.position);
                    debugText.text = "Item position: " + this.transform.position + "Player position: " + arCamera.transform.position + "Distance: " + distance;
                }
                else
                {
                    // If out of range, show cannot catch screen
                    catchManager.CannotCatchScreenItemToFarAway(this);
                    Debug.Log("Item clicked too far away");
                    
                    // Calculate distance between player and item for debug purposes
                    distance = Vector3.Distance(arCamera.transform.position, transform.position);
                    debugText.text = "Item position: " + this.transform.position + "Player position: " + arCamera.transform.position + "Distance: " + distance;
                }
            }
            else
            {
                // If AR mode is not active, show cannot catch screen
                catchManager.CannotCatchScreen(this);
                Debug.Log("Item clicked in non-AR mode");
            }
        }
    }
}
