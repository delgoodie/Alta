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
        Debug.Log("PlayerManager Awake");
    }

    private void Update()
    {
    }

    public void CreatePlayer()
    {
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
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