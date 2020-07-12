using NaughtyAttributes;
using System;
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
    [SerializeField]
    visualObjectType type;

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

    private void Start()
    {
        UpdateColor();
    }

    internal void Init(TileType type)
    {
        owner = type;

        UpdateColor();
    }
}
