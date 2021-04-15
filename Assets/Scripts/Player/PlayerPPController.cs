using UnityEngine;
using UnityEngine.Rendering;

public class PlayerPPController : MonoBehaviour
{
    [SerializeField]
    private VolumeProfile volume;
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }
    private void Update() { }
}