# LootAR
LootAR is a Unity-based AR game that combines geolocation with augmented reality. Explore a real-world map, find virtual items, and collect them in AR mode. Features include item collection and obfuscation functionalities. This  experience combines the power of Mapbox, ARFoundation and Google ARCore, and Sentis.

## Features
* **Geolocation Integration:** Utilizes Mapbox to display a real-world map with item spawn locations.
* **AR Exploration:** Leverages ARFoundation and Google AR Core to provide an augmented reality experience where players can see and interact with virtual items in their environment.
* **Item Collection:** Players can collect items by getting close to them in AR mode and tapping on them.
* **Obfuscation Model:** Includes functionalities to obfuscate sensitive information in the AR view using techniques like blurring, masking, and pixelation.

## Setup Instructions
Note: Some packages and SDKs may already be installed in the project. Verify this in the Package Manager to avoid duplicate installations.
1. **Clone the Repository:** Clone the repository to your local machine using `git clone https://github.com/CIIC-C-T-Polytechnic-of-Leiria/LootAR.git`
2. **Open the Project in Unity:** Ensure you have Unity version `2022.3.10f1` installed for compatibility. 
3. **Install Required Packages:**
   * **AR:**
     * Open Unity
     * Go to `Window > Package Manager`
     * In the Package Manager, search for `AR`
     * Click on `AR` in Features and then click `Install`
     * This will install a set of 5 packages including ARFoundation and Google AR Core
   * **Sentis:**
     * In the Package Manager window, search for `Sentis`
     * Click on `Sentis` and then click `Install`
   * **Mapbox SDK:**
     * Download the Mapbox SDK from the Mapbox website
     * In Unity, go to `Assets > Import Package > Custom Package`
     * Select the downloaded Mapbox SDK package and import all assets
4. **Configure Player Settings:**
   * Go to `File > Build Settings`
   * Select the target platform (Android)
   * Click on `Player Settings` in the Build Settings window
     * In Other Settings:
       * Disable Auto Graphics API
       * Set Minimum API Level to 24
       * Set Scripting Backend to IL2CPP
5. **Configure XR Plug-in Management Settings:**
   * Go to `Edit > Project Settings`
   * Click on `XR Plug-in Management` in the Project Settings window
     * Ensure Google ARCore is checked.
6. **Build and Run:**
   * Build Settings:
     * Click on `Build` and choose a location to save the build files
     * Once the build is complete, deploy the app to your target mobile device

## Changing Obfuscation Settings
The obfuscation settings can be customized to apply different types of obfuscation to various classes (objects recognized by YOLO). This is done by modifying the obfuscationTypes dictionary in the `ARCameraManager` script. You can add or remove entries and change the obfuscation type (Masking, Pixelation, Blurring, or None) for different classes.
```csharp
obfuscationTypes = new Dictionary<int, Obfuscation.Type>
{
    { 0, Obfuscation.Type.Pixelation }, // person
    { 1, Obfuscation.Type.Masking }, // bicycle
    { 2, Obfuscation.Type.Blurring }, //car
    { 3, Obfuscation.Type.Blurring }, //motorcycle
    { 63, Obfuscation.Type.Masking }, //laptop
    { 67, Obfuscation.Type.Blurring } // cell phone
    //add or remove entries as needed
};
```
### Available Obfuscation Types
* **Masking:** Covers the object with a mask.
* **Pixelation:** Applies pixelation to the object.
* **Blurring:** Blurs the object.
* **None:** No obfuscation applied.

## How to Play
1. **Explore the Map:** Open the app where the map is launched to see where virtual items are spawned. Use the map to navigate to these locations.
2. **Switch to AR Mode:** Tap the camera button to switch to the augmented reality view. This will enable the AR camera, allowing you to see virtual items in your physical environment.
3. **Collect Items:** Move close to the virtual items in AR mode and tap on them to collect them. Ensure you are within the interaction range to successfully collect the items.

## Citation 

If you use any part of the code in this repository, please cite the following paper:

```bibtex
@InProceedings{10.1007/978-3-031-81713-7_13,
author="Ribeiro, Tiago and Marto, Anabela and Gon{\c{c}}alves, Alexandrino and Santos, Leonel and Rabad{\~a}o, Carlos and de C. Costa, Rog{\'e}rio Lu{\'i}s",
title="SafeARUnity: Real-Time Image Processing to Enhance Privacy Protection in LBARGs",
booktitle="Videogame Sciences and Arts",
year="2025",
publisher="Springer Nature Switzerland",
address="Cham",
pages="186--200",
isbn="978-3-031-81713-7",
doi="10.1007/978-3-031-81713-7_13"
}
```

## Acknowledgements
This work is funded by FCT - Fundação para a Ciência e a Tecnologia, I.P., through project with reference 2022.09235.PTDC.


<hr style="height:0.5px; background-color:grey; border:none;">

<p align="center">
<img src="assets/CIIC_logo.png" width="700px"/>
</p>

<hr style="height:0.5px; background-color:grey; border:none;">
