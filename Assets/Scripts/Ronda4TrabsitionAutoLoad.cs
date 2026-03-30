using UnityEngine;
using UnityEngine.SceneManagement;

public class Ronda4TrabsitionAutoLoad : MonoBehaviour
{
    [Header("Escena destino")]
    [SerializeField] private string sceneToLoad = "Ronda4";

    private void Start()
    {
        // Apenas entra a la escena, carga Ronda4
        if (!string.IsNullOrWhiteSpace(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("El nombre de la escena está vacío en Ronda4TrabsitionAutoLoad.");
        }
    }
}