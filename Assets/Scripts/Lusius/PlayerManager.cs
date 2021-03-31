using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public Transform spawnPoint;
    public GameObject playerPrefab;
    private GameObject orbPrefab;
    private GameObject player;
    private GameObject orb;
    public bool activePlayer;

    private void Awake()
    {
        Instance = this;
        activePlayer = false;
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        orbPrefab = Resources.Load("Prefabs/Orb") as GameObject;
    }

    private void Update()
    {
    }

    public void CreatePlayer()
    {
        player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        orb = Instantiate(orbPrefab, spawnPoint.position + Vector3.forward * 2f, Quaternion.identity);
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

    public GameObject GetOrb()
    {
        return orb;
    }

    private void OnDrawGizmos()
    {
    }
}