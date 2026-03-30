using UnityEngine;

public class ObjectiveDebugInput : MonoBehaviour
{
    [SerializeField] private SimpleObjectiveUI objectiveUI;

    private void Update()
    {
        if (objectiveUI == null) return;

        if (Input.GetKeyDown(KeyCode.N))
            objectiveUI.NextObjective();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            objectiveUI.SetObjectiveIndex(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            objectiveUI.SetObjectiveIndex(1);

        if (Input.GetKeyDown(KeyCode.H))
            objectiveUI.ShowPanel(false);

        if (Input.GetKeyDown(KeyCode.J))
            objectiveUI.ShowPanel(true);
    }
}