using UnityEngine;
using UnityEngine.SceneManagement;

public class ShuffleDeckButton2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandView handView;

    [Header("Round")]
    [SerializeField] private bool onlyFromRound2 = true;
    [SerializeField] private string round2SceneName = "Ronda2";

    [Header("Usos")]
    [SerializeField] private bool oneFreeUsePerSceneEntry = true;

    private bool freeUseSpentThisScene = false;

    private void Start()
    {
        freeUseSpentThisScene = false;
    }

    private void OnMouseDown()
    {
        TryUseShuffle();
    }

    public void TryUseShuffle()
    {
        if (!IsAllowedInThisScene()) return;
        if (handView == null) return;
        if (!handView.CanShuffleNow()) return;

        bool canUse = false;

        if (oneFreeUsePerSceneEntry && !freeUseSpentThisScene)
        {
            freeUseSpentThisScene = true;
            canUse = true;
        }
        else if (GameProgress.Instance != null && GameProgress.Instance.TryConsumeToken(1))
        {
            canUse = true;
        }

        if (!canUse) return;

        handView.ShuffleHandAndTable();
    }

    private bool IsAllowedInThisScene()
    {
        if (!onlyFromRound2) return true;
        return SceneManager.GetActiveScene().name == round2SceneName;
    }
}