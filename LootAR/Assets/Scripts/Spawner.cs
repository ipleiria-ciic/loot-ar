using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using UnityEngine.Assertions;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class Spawner : Singleton<Spawner>
{
    [SerializeField] private AbstractMap _map;
    [SerializeField] private GameObject arCamera;
    [SerializeField] private GameObject parentItems;
    [SerializeField] private GameObject arScene;
    [SerializeField] private GameObject arGui;
    [SerializeField] private GameObject loader;
    [SerializeField] private Item[] availableItems;
    [SerializeField] private Text itemCloseByText;
    [SerializeField] private GameObject ItemCloseByPanel;
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Text debugText;
    [SerializeField] private Text debugText1;
    [SerializeField] private Player player;
    [SerializeField] private float waitTime = 30f;
    [SerializeField] private float minRange = 3f;
    [SerializeField] private float maxRange = 15f;

    private float time = 3f;

    private bool isRotated = true;

    public float minScale = 0.1f;
    public float maxScale = 0.5f;
    public float viewDistanceScaleFactor = 0.1f; // Adjust this factor based on testing to get the desired effect
    private int maxItemsToSpawn = 5;
    private static bool isPersisted = false;

    private List<Item> spawnedItems = new List<Item>();
    private Item selectedItem;
    private Vector3 playerPosition;

    private Dictionary<string, List<Vector2d>> itemLocations = new Dictionary<string, List<Vector2d>>();


    public Item SelectedItem
    { get { return selectedItem; } }

    public List<Item> SpawnedItems { get { return spawnedItems; } }

    private void Awake()
    {
        //playerPosition = transform.TransformPoint(player.transform.position);
        playerPosition = player.transform.position;
        //arCamera.transform.TransformPoint(playerPosition);
        arCamera.transform.position = playerPosition;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Assert.IsNotNull(availableItems);
        Assert.IsNotNull(player);

        itemLocations = new Dictionary<string, List<Vector2d>>()
        {
            { "Cloth", new List<Vector2d> { LocationConstants.patioA, LocationConstants.edificioDCentro,
                LocationConstants.edificioDDireita, LocationConstants.edificioDEsquerda, LocationConstants.edificioA, LocationConstants.edificioB,
                LocationConstants.edificioC, LocationConstants.edificioE, LocationConstants.esslei, LocationConstants.biblioteca } },
            { "Metal", new List<Vector2d> { LocationConstants.estacionamentoADireita, LocationConstants.estacionamentoAEsquerda,
                LocationConstants.estacionamentoDEsquerda, LocationConstants.estacionamentoDDireita, LocationConstants.estacionamentoE,
                LocationConstants.estacionamentoAtrasE, LocationConstants.estacionamentoESSLEI, LocationConstants.estacionamentoProfessores,
                LocationConstants.dakar } },
            { "Wood", new List<Vector2d> { LocationConstants.zonaVerdeDakar, LocationConstants.zonaVerdeEdificioC } },
            { "Food", new List<Vector2d> { LocationConstants.cantinaBaixo, LocationConstants.cantinaCima, LocationConstants.barA } }
        };
        StartCoroutine(SpawnItemRoutine());
        /* if (!isPersisted)
        {
            DontDestroyOnLoad(arScene);
            DontDestroyOnLoad(arGui);
            DontDestroyOnLoad(loader);
            isPersisted = true;
        } */
    }

    private void Update()
    {
        playerPosition = player.transform.position;
        //playerPosition = transform.TransformPoint(player.transform.position);
        //arCamera.transform.TransformPoint(playerPosition);
        arCamera.transform.position = playerPosition;
        debugText1.text = "Player position: " + playerPosition + " AR Camera position: " + arCamera.transform.position;

        AdjustItemSize();
        ItemCloseBy();
        /* if (time > 0)
        {
            time -= Time.deltaTime;
        } else
        {
            time = 3f;
            refreshItems();
        } */

    }

    public void ItemWasSelected(Item item)
    {
        selectedItem = item;
    }

    private IEnumerator SpawnItemRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            foreach (var itemEntry in itemLocations)
            {

                string itemName = itemEntry.Key;

                List<Vector2d> locations = itemEntry.Value;

                foreach (var location in locations)
                {
                    //if (CanItemsSpawnInLocation(location))
                    //{
                    //Debug.Log("Location: " + location);
                    Item itemPrefab = GetItemPrefabByName(itemName);
                    //Debug.Log("Item prefab: " + itemPrefab.GetItemName);
                    Vector3 locationCalculateDistance = _map.GeoToWorldPosition(location, true);
                    float distance = Vector3.Distance(playerPosition, locationCalculateDistance);
                    //Debug.Log("Distance: " + distance);
                    //Debug.Log("Player position: " + playerPosition);
                    //Debug.Log("Location calculate distance: " + locationCalculateDistance);

                    if (distance < 30) // Adjust the threshold as needed
                    {
                        int itemsSpawned = CountItemsInLocation(location);
                        int itemsToSpawn = maxItemsToSpawn - itemsSpawned;

                        //if (itemsToSpawn > 0)
                        //{
                        for (int i = 0; i < itemsToSpawn; i++)
                        {
                            SpawnItem(itemPrefab, location);
                            yield return new WaitForSeconds(waitTime);
                        }
                        //}

                        Debug.Log("Items spawned: " + itemsSpawned);
                        Debug.Log("Items to spawn: " + itemsToSpawn);

                    }
                    //}

                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private Item GetItemPrefabByName(string itemName)
    {
        Item matchingItem = null;

        // Find the item prefab based on its name
        int currentIndex = 0;
        while (matchingItem == null && currentIndex < availableItems.Length)
        {
            var item = availableItems[currentIndex];
            //Debug.Log("Item name: " + item.GetItemName);
            //Debug.Log("Item name string: " + itemName);
            if (item.GetItemName == itemName)
            {
                matchingItem = item; // Store the matching item
            }
            currentIndex++; // Move to the next item in the array
        }

        return matchingItem; // Return the matching item if found, or null if not found
    }

    public void SpawnItem(Item itemPrefab, Vector2d location)
    {
        Vector3 spawnPosition = _map.GeoToWorldPosition(location, true); // Assuming "_map" is your Mapbox map object
        spawnPosition += new Vector3(GenerateRange(), 10, GenerateRange()); // Offset in X and Z directions
        Debug.Log("Spawn position: " + spawnPosition);
        Item spawnedItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity); //, parentItems.transform;
        //Debug.Log("Spawn position: " + spawnPosition);
        spawnedItem.GeographicLocation = location;
        spawnedItems.Add(spawnedItem);
    }

    /* public void refreshItems()
    {
        foreach (var item in spawnedItems)
        {
            Vector3 currentPosition = item.transform.position;
            item.transform.position = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
            Debug.Log("Seconds: " + time + " Item position: " + item.transform.position + " Player position: " + player.transform.position);
        }
    } */

    private int currentIndex = 0; // Add this to track the current index in the predefined ranges
    private float[] predefinedRanges = { 0, -5, 5, -10, 10}; // Example predefined ranges
    private float GenerateRange()
    {
        /* float randomNumber = Random.Range(minRange, maxRange);
        bool isPositive = Random.Range(0, 10) < 5;
        return randomNumber * (isPositive ? 1 : -1); */
        float range = predefinedRanges[currentIndex];
        currentIndex = (currentIndex + 1) % predefinedRanges.Length; // Cycle through the predefined ranges
        return range;
    }

    private int CountItemsInLocation(Vector2d location)
    {
        Debug.Log("Location: " + location);
        int count = 0;
        Debug.Log("Spawned items count: " + spawnedItems.Count);
        foreach (var item in spawnedItems)
        {
            float distance = Vector3.Distance(item.transform.position, _map.GeoToWorldPosition(location, true));
            if (distance < 30)
            {
                count++;
            }
        }
        return count;
    }

    private bool CanItemsSpawnInLocation(Vector2d location)
    {
        return CountItemsInLocation(location) < maxItemsToSpawn;
    }

    public void DestroyItem(Item item)
    {
        spawnedItems.Remove(item);
        Destroy(item.gameObject);
    }

    public void AdjustItemSize()
    {
        // Adjust the size of the item based on the distance from the player
        /* float distance = Vector3.Distance(player.transform.position, transform.position);
        float scale = 1f / distance;
        transform.localScale = Vector3.one * scale; */
        
        foreach (var item in spawnedItems)
        {
            if (arCamera.activeSelf)
            {
                /* if (isRotated)
                {
                    Vector3 auxRotation = parentItems.transform.eulerAngles;
                    auxRotation.y += 180f;
                    parentItems.transform.eulerAngles = auxRotation;
                    isRotated = false;
                }  */
                if(item != null)
                {
                    item.gameObject.SetActive(true);
                    /* if (tempIsRotated)
                    {
                        item.transform.position = new Vector3(-item.transform.position.x, 1, -item.transform.position.z);
                    } */
                    item.transform.position = new Vector3(item.transform.position.x, -4, item.transform.position.z);

                    float distance = Vector3.Distance(arCamera.transform.position, item.transform.position);

                    float minScale; // Set these as per your game's need
                    float maxScale; // Set these as per your game's need
                    //float scaleDistance = 3.0f; // Distance at which scale is max
                    float minDistance = 1f;
                    float maxDistance = 20f;

                    float clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);
                    //float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
                    float normalizedDistance = (clampedDistance - minDistance) / (maxDistance - minDistance);

                    switch (item.GetItemName)
                    {
                        case "Cloth":
                            minScale = 0.2f;
                            maxScale = 0.5f;
                            float scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
                            item.transform.localScale = new Vector3(scale, scale, scale);
                            //item.gameObject.SetActive(true);
                            break;
                        case "Metal":
                            minScale = 20f;
                            maxScale = 50f;
                            scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
                            item.transform.localScale = new Vector3(scale, scale, scale);
                            //item.gameObject.SetActive(true);
                            break;
                        case "Wood":
                            minScale = 0.4f;
                            maxScale = 1f;
                            scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
                            item.transform.localScale = new Vector3(scale, scale, scale);
                            //item.gameObject.SetActive(true);
                            //debugText.text = "item position: " + item.transform.position + " player position: " + player.transform.position;
                            break;
                        case "Food":
                            minScale = 0.8f;
                            maxScale = 2f;
                            scale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
                            item.transform.localScale = new Vector3(scale, scale, scale);
                            //item.gameObject.SetActive(true);
                            break;
                    } 
                }

            }
            else if (!arCamera.activeSelf)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(true);
                    /* if (isRotated == false)
                    {
                        Vector3 auxRotation = parentItems.transform.eulerAngles;
                        auxRotation.y += 180f;
                        parentItems.transform.eulerAngles = auxRotation;
                        isRotated = true;
                    } */
                    /* if (tempIsRotated == false)
                    {
                        item.transform.position = new Vector3(-item.transform.position.x, 10, -item.transform.position.z);
                    } */
                    item.transform.position = new Vector3(item.transform.position.x, 10, item.transform.position.z);

                    switch (item.GetItemName)
                    {
                        case "Cloth":
                            item.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                            item.gameObject.SetActive(true);
                            break;
                        case "Metal":
                            item.transform.localScale = new Vector3(50f, 50f, 50f);
                            item.gameObject.SetActive(true);
                            break;
                        case "Wood":
                            item.transform.localScale = new Vector3(1f, 1f, 1f);
                            item.gameObject.SetActive(true);
                            break;
                        case "Food":
                            item.transform.localScale = new Vector3(2f, 2f, 2f);
                            item.gameObject.SetActive(true);
                            break;
                    }
                }
            }
        }
    }

    private void ItemCloseBy()
    {
        //bool itemsCloseBy = false; // Flag to track if any items are close by
        string message = "No Items Close By, Keep Exploring!";
        ItemCloseByPanel.SetActive(true);

        if (arCamera.activeSelf)
        {
            Vector3 cameraForward = arCamera.transform.forward; // Get the camera's forward direction

            foreach (var item in spawnedItems)
            {
                if (item != null)
                {
                    float distance = Vector3.Distance(arCamera.transform.position, item.transform.position);
                    //Debug.Log("Distance: " + distance);
                    float threshold = 20f;
                    if (distance <= threshold)
                    {
                        //ItemCloseByPanel.SetActive(true);
                        Vector3 itemDirection = (item.transform.position - arCamera.transform.position).normalized;
                        float angle = Vector3.Angle(cameraForward, itemDirection);
                        //Debug.Log("Angle: " + angle);

                        if (angle < 45f)
                        {
                            message = "There is " + item.GetItemName + " in front of you!";
                        }
                        else if (angle > 135f)
                        {
                            message = "There is " + item.GetItemName + " behind you!";
                        }
                        else
                        {
                            float dotProduct = Vector3.Dot(arCamera.transform.right, itemDirection);

                            if (dotProduct > 0)
                            {
                                message = "There is " + item.GetItemName + " on your right!";
                            }
                            else
                            {
                                message = "There is " + item.GetItemName + " on your left!";
                            }
                        }

                        //itemsCloseBy = true;
                    }
                }
            }
        }

        // Set the message based on the flag
        //itemCloseByText.text = itemsCloseBy ? message : "No Items Close By, Keep Exploring!";
        itemCloseByText.text = message;

        if (!arCamera.activeSelf)
        {
            ItemCloseByPanel.SetActive(false);
            itemCloseByText.text = "";
        }
    }

    private Vector3 CalculateScaleForItem(float distance, float minScale, float maxScale)
    {
        float scaleDistance = 7.0f; // Distance at which scale is max   
        distance = Mathf.Max(distance, 0.1f);
        if (distance == 0) distance = 0.01f; // Prevents divide by zero error
        float scale = Mathf.Clamp(1 / (distance * scaleDistance), minScale, maxScale);

        Debug.Log($"Item Distance: {distance}, Calculated Scale: {scale}");
        return new Vector3(scale, scale, scale);
    }
}


