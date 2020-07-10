using System;
using UnityEngine;

public class VisualObjectSpawner : MonoBehaviour
{
    static VisualObjectSpawner instance;

    [SerializeField]
    Transform castle, startingVillage, village;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public static VisualPlayerOwnedObject SpawnObject(visualObjectType typeToSpawn, VisualTile3D tile)
    {
        return Instantiate(instance.GetPrefabByType(typeToSpawn), tile.GetPosition(), tile.GetForwardAsQuaternion()).GetComponent<VisualPlayerOwnedObject>();

    }

    private Transform GetPrefabByType(object typeToSpawn)
    {
        switch (typeToSpawn)
        {
            case visualObjectType.Castle:
                return castle;

            case visualObjectType.StartingVillage:
                return startingVillage;

            case visualObjectType.Village:
                return village;
        }

        return transform;
    }
}
