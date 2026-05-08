using System.Collections.Generic;
using UnityEngine;

public class PlayerColorController : MonoBehaviour
{
    [System.Serializable]
    public class ColorMaterialEntry
    {
        public EndpointColorType colorType;
        public Material material;
    }

    [Header("玩家要变色的Renderer")]
    public Renderer[] targetRenderers;

    [Header("颜色材质表")]
    public List<ColorMaterialEntry> colorMaterials = new List<ColorMaterialEntry>();

    [Header("初始颜色")]
    public EndpointColorType startColor = EndpointColorType.White;

    private List<EndpointColorType> unlockedColors = new List<EndpointColorType>();
    private int currentColorIndex = 0;

    public EndpointColorType CurrentColor
    {
        get
        {
            if (unlockedColors.Count == 0)
                return EndpointColorType.White;

            return unlockedColors[currentColorIndex];
        }
    }

    private void Start()
    {
        unlockedColors.Clear();
        unlockedColors.Add(startColor);
        currentColorIndex = 0;

        ApplyCurrentColor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CycleColor();
        }
    }

    public void AddColor(EndpointColorType newColor)
    {
        if (!unlockedColors.Contains(newColor))
        {
            unlockedColors.Add(newColor);
            Debug.Log("获得新颜色：" + newColor);
        }
    }

    public void CycleColor()
    {
        if (unlockedColors.Count <= 1)
            return;

        currentColorIndex++;
        if (currentColorIndex >= unlockedColors.Count)
            currentColorIndex = 0;

        ApplyCurrentColor();
        Debug.Log("当前颜色切换为：" + CurrentColor);
    }

    public bool IsCurrentColor(EndpointColorType colorType)
    {
        return CurrentColor == colorType;
    }

    public bool HasColor(EndpointColorType colorType)
    {
        return unlockedColors.Contains(colorType);
    }

    private void ApplyCurrentColor()
    {
        Material targetMat = GetMaterialByColor(CurrentColor);
        if (targetMat == null)
        {
            Debug.LogWarning("没有找到颜色 " + CurrentColor + " 对应的材质");
            return;
        }

        foreach (Renderer r in targetRenderers)
        {
            if (r != null)
            {
                r.material = targetMat;
            }
        }
    }

    private Material GetMaterialByColor(EndpointColorType colorType)
    {
        foreach (ColorMaterialEntry entry in colorMaterials)
        {
            if (entry.colorType == colorType)
                return entry.material;
        }

        return null;
    }
}