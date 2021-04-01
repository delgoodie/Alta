using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject playerPrefab;
    private GameObject player;
    public bool activePlayer;

    private void Awake()
    {
        Instance = this;
        activePlayer = false;
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
    }

    private void Update()
    {
    }

    public void CreatePlayer(Vector3 position, Quaternion rotation)
    {
        player = Instantiate(playerPrefab, position, rotation);
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