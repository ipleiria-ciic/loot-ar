using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ARCameraManager : MonoBehaviour
{
    [Header("AR_Camera")]
    [SerializeField] private GameObject arCamera;
    [SerializeField] private GameObject arCameraButtonBack;
    private Camera arCameraComponent; // The AR camera component
    public static ARCameraManager Instance { get; private set; }
    [SerializeField] private GameObject mainCamera;

    [Header("Map, Player and RenderTexture")]
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject player;
    [SerializeField] private RenderTexture renderTexture;

    // --- Obfuscation --------------
    public Dictionary<int, Obfuscation.Type> obfuscationTypes;

    [Header("SafeARLayer Obfuscation")]
    [SerializeField] private ImgObfuscator imgObfuscator;

    [Header("Debug Plane Output")]
    private Texture2D outputTexture;
    private Texture2D currentFrame;

    [SerializeField] private Image imageUI;
    [SerializeField] private GameObject imageUIObject;
    [SerializeField] private RawImage rawImage;

    public int frameInterval = 100; // to change the image every 100 frames
    private int frameCount = 0;
    private readonly List<string> imagePaths;
    private int currentImageIndex = 0;
    
    private RectTransform canvasTransform;

    private Camera captureCamera;

    void Start()
    {
        // Find the AR camera and setup render texture
        GameObject locationBasedGame = GameObject.Find("LocationBasedGame (1)");
        GameObject player = locationBasedGame.transform.Find("Player").gameObject;
        GameObject xrOrigin = player.transform.Find("XR Origin").gameObject;
        Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
        Transform mainCameraAR = cameraOffset.Find("Main Camera AR");
        arCameraComponent = mainCameraAR.GetComponent<Camera>();

        // Setup render texture
        renderTexture = new RenderTexture(arCameraComponent.pixelWidth, arCameraComponent.pixelHeight, 24);
        renderTexture.Create();
        arCameraComponent.targetTexture = renderTexture;
        

        // Setup a secondary camera for capturing the AR feed
        /*GameObject captureCameraObject = new GameObject("CaptureCamera");
        captureCamera = captureCameraObject.AddComponent<Camera>();
        captureCamera.CopyFrom(arCameraComponent);
        captureCamera.targetTexture = renderTexture;
        captureCamera.enabled = false;*/ // Disable this camera from rendering to screen

        // Obfuscation Mapping
        obfuscationTypes = new Dictionary<int, Obfuscation.Type>
        {
            { 0, Obfuscation.Type.Pixelation }, // person
            { 1, Obfuscation.Type.Masking }, // bicycle
            { 2, Obfuscation.Type.Blurring }, //car
            { 3, Obfuscation.Type.Blurring }, //motorcycle
            { 63, Obfuscation.Type.Masking }, //laptop
            { 67, Obfuscation.Type.Blurring } // cell phone
        };

        StartCoroutine(CaptureAndProcessFrame());
    }

    IEnumerator CaptureAndProcessFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (arCamera.activeSelf)
            {
                // Ensure the primary AR camera is rendering to the screen
                //arCameraComponent.targetTexture = null;

                // Manually render the capture camera to the render texture
                //captureCamera.Render();

                currentFrame = ToTexture2D(renderTexture);

                if (outputTexture != null)
                {
                    Destroy(outputTexture);
                    outputTexture = null;
                }

                outputTexture = imgObfuscator.Run(currentFrame, obfuscationTypes);

                rawImage.enabled = true;
                rawImage.texture = outputTexture;
            }
            else
            {
                rawImage.enabled = false;
            }
        }
    }

    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        //RenderTexture.active = null;
        return tex;
    }

    Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                flipped.SetPixel(x, original.height - y - 1, original.GetPixel(x, y));
            }
        }

        flipped.Apply();
        return flipped;
    }

    public void ARCameraOn()
    {
        arCamera.SetActive(!arCamera.activeSelf);
        if (arCamera.activeSelf)
        {
            Debug.Log("AR Camera On");
            arCameraButtonBack.SetActive(true);
            map.SetActive(false);
            player.SetActive(false);
        }
        else
        {
            arCameraButtonBack.SetActive(false);
            map.SetActive(true);
            player.SetActive(true);
        }
    }

    public void BackToMap()
    {
        arCamera.SetActive(false);
        arCameraButtonBack.SetActive(false);
        map.SetActive(true);
        player.SetActive(true);
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
            renderTexture = null;
        }

        if (currentFrame != null)
        {
            Destroy(currentFrame);
            currentFrame = null;
        }

        if (captureCamera != null)
        {
            Destroy(captureCamera.gameObject);
        }
    }
}
