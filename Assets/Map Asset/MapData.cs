using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapData", menuName = "Map/Map Data")]
public class MapData : ScriptableObject
{
    [System.Serializable]
    public class PlacedItem
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 rotation;   // 新增：保存欧拉角
    }

    public List<PlacedItem> placedItems = new List<PlacedItem>();
}