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

    public Vector3Int PlayerChunkPosition()
    {
        Vector3 pos = player.transform.position;
        if (pos.x < 0) pos.x -= 15f;
        if (pos.y < 0) pos.y -= 15f;
        if (pos.z < 0) pos.z -= 15f;
        float one_fifteenth = 0.06666666667f;

        return new Vector3Int((int)(pos.x * one_fifteenth), (int)(pos.y * one_fifteenth), (int)(pos.z * one_fifteenth));
    }

    public GameObject GetPlayer()
    {
        return player;
    }

    private void OnDrawGizmos()
    {
    }
}