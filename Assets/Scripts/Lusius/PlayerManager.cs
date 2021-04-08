using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public static PlayerManager Instance;
    [HideInInspector]
    public bool activePlayer;
    [HideInInspector]
    public GameObject playerPrefab;
    private GameObject player;
    [SerializeField]
    private Transform UnderwaterVolume;
    [SerializeField]
    private Transform SurfaceBottom;

    private void Awake()
    {
        Instance = this;
        activePlayer = false;
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
    }

    private void Update()
    {
        if (activePlayer)
        {
            SurfaceBottom.position = new Vector3(player.transform.position.x, SurfaceBottom.position.y, player.transform.position.z);
            UnderwaterVolume.position = player.transform.position;
        }
    }

    public void CreatePlayer(Vector3 position, Quaternion rotation)
    {
        player = Instantiate(playerPrefab, position, rotation, transform);
        activePlayer = true;
    }

    public Vector3 PlayerPosition()
    {
        return player.transform.position;
    }

    public GameObject GetPlayer()
    {
        return player;
    }

    private void OnDrawGizmos()
    {
    }
}