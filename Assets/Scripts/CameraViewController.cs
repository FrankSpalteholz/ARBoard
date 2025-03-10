using UnityEngine;

public class CameraViewController : MonoBehaviour
{
    public string viewerCameraTag = "FPVRenderCam"; // Tag der Kamera
    public RenderTexture ScreenProjectionTexture; // RenderTexture vom iPhone
    public GameObject screenObject; // Das GameObject, das den Bildschirm darstellt (3D-Objekt)
    public GameObject FPVCamGameObject;

    private Camera viewerCamera;
    private MeshRenderer meshRenderer; // Der MeshRenderer des Objekts, auf dem dieses Skript läuft

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            Debug.LogWarning("MeshRenderer nicht gefunden!");
        }
        else
        {
            Debug.Log("MeshRenderer gefunden!");

        }
    }

    void Start()
    {
        // // Finde die Kamera mit dem Tag 'FPVRenderCam'
        // viewerCamera = GameObject.FindGameObjectWithTag(viewerCameraTag)?.GetComponent<Camera>();

        // // Falls die Kamera nicht gefunden wurde, gebe eine Warnung aus
        // if (viewerCamera == null)
        // {
        //     Debug.LogWarning("Kamera mit Tag 'FPVRenderCam' nicht gefunden!");
        // }

        // Hole den MeshRenderer des aktuellen Objekts (auf dem das Skript angewendet wird)
        

        
    }

    void Update()
    {
        if (meshRenderer != null)
        {

            // Berechne den Mittelpunkt der Bounding Box im World Space
            Vector3 screenCenterWorldSpace = screenObject.GetComponent<Renderer>().bounds.center;
            
            // Berechne die Größe des Bildschirms in World Space (Breite und Höhe)
            Vector3 screenSize = screenObject.GetComponent<Renderer>().bounds.size;
            float screenScaleFactor = Mathf.Max(screenSize.x, screenSize.y); // Größte Kante als Skalierungsfaktor

            // Die Position der Kamera und die Position des Bildschirms an den Shader übergeben
            Vector3 cameraPosition = FPVCamGameObject.transform.position;
            Quaternion cameraRotation = FPVCamGameObject.transform.rotation;

            // Die Position der Kamera, Rotation, die Bildschirm-Mitte und die Bildschirmgröße an den Shader übergeben
            meshRenderer.material.SetVector("_ViewerPosition", cameraPosition); // Setze die Kamera-Position im Shader
            meshRenderer.material.SetVector("_ViewerRotation", cameraRotation.eulerAngles); // Setze die Rotation im Shader
            meshRenderer.material.SetVector("_ScreenPosition", screenCenterWorldSpace); // Setze die Bildschirm-Mitte im Shader
            meshRenderer.material.SetVector("_ScreenSize", screenSize); // Setze die Bildschirmgröße im Shader
            meshRenderer.material.SetFloat("_ScreenScaleFactor", screenScaleFactor); // Setze den Skalierungsfaktor im Shader

            // Die Textur für den Shader übergeben
            meshRenderer.material.SetTexture("_MainTex", ScreenProjectionTexture); // Setze die Textur (RenderTexture vom iPhone)

        }
    }
}
