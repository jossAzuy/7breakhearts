using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool IsDead { get; set; }

    void Awake()
    {
        IsDead = false;
    }
}
