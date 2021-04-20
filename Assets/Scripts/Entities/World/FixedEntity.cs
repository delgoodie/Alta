using UnityEngine;
public class FixedEntity : MonoBehaviour, IEntity
{
    [SerializeField]
    private string editorType;
    public string type { get; private set; }
    private void Awake()
    {
        type = editorType;
    }
}
