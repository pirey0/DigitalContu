using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum visualObjectType
{
    None,
    Castle,
    StartingVillage,
    Village,
}
public class VisualPlayerOwnedObject : MonoBehaviour
{
    [SerializeField]
    TileType owner;
    visualObjectType type;

    private void Start()
    {
        UpdateColor();
    }

    [Button]
    public void UpdateColor ()
    {
        Material playerMaterial = VisualDataHolder.GetPlayerMaterial(owner);

        if (playerMaterial == null)
            return;

        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = playerMaterial;
        }
    }

    public void Destroy ()
    {
        Destroy(gameObject);
    }
}
