using UnityEngine;

[CreateAssetMenu(fileName = "SpawnTable", menuName = "Potentacles/Map Data")]
public class MapSpawnTable : ScriptableObject
{
    [Header("Wall Rarities")]
    public WallRarity[] wallRarities = new WallRarity[]
    {
        new WallRarity { name = "Normal", weight = 70f },
        new WallRarity { name = "Uncommon", weight = 25f },
        new WallRarity { name = "Rare", weight = 5f }
    };

}
