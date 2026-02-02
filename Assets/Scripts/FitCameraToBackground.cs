using UnityEngine;

[ExecuteAlways]
public class FitCameraToBackground : MonoBehaviour
{
    public Camera cam;
    public SpriteRenderer background;
    public bool fitWidth = true; // si false, ajusta por altura

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!cam || !background) return;

        cam.orthographic = true;

        Bounds b = background.bounds; // tamaño en unidades del mundo
        float bgWidth = b.size.x;
        float bgHeight = b.size.y;

        float screenAspect = (float)Screen.width / Screen.height;
        float bgAspect = bgWidth / bgHeight;

        if (fitWidth)
        {
            // Ajustar para que el ancho del background quepa en pantalla
            cam.orthographicSize = (bgWidth / screenAspect) * 0.5f;
        }
        else
        {
            // Ajustar para que la altura del background quepa en pantalla
            cam.orthographicSize = bgHeight * 0.5f;
        }
    }
}