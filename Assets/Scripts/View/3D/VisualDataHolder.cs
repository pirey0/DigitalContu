using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VisualDataHolder : MonoBehaviour
{
    static VisualDataHolder instance;

    [SerializeField] Material playerOneMat, playerTwoMat;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static Material GetPlayerMaterial(TileType player)
    {
        switch(player)
        {
            case TileType.Player1:
                return instance.playerOneMat;

            case TileType.Player2:
                return instance.playerTwoMat;

        }

        return null;
    }
}
