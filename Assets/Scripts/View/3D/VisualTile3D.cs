using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualTile3D : MonoBehaviour
{
    List<VisualPlayerOwnedObject> visualizations = new List<VisualPlayerOwnedObject>();
    TileType owner;

    internal void SwitchTo(TileType type)
    {
        foreach (var visualization in visualizations)
        {
            visualization.Destroy();
        } 
        
        if (type == TileType.Player1 || type == TileType.Player2)
        {
            VisualObjectSpawner.SpawnObject(visualObjectType.Village,this);
        }
    }

    internal Vector3 GetPosition()
    {
        return transform.position;
    }

    internal Quaternion GetForwardAsQuaternion()
    {
        return Quaternion.identity;
    }
}
