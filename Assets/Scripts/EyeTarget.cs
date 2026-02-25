using UnityEngine;

public class EyeTarget : MonoBehaviour
{
    [SerializeField] private EyeBossController boss;

    public bool IsLeftEye = true;

    public void OnShot()
    {
        if (boss != null)
            boss.OnEyeShot(this);
    }
}