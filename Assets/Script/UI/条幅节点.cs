using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterBannerNode : MonoBehaviour, IPointerClickHandler
{
    [Header("角色配置数据资产")]
    [Tooltip("请把项目文件夹里的 C01_Config 等对应的 ScriptableObject 配置文件拖到这里")]
    public CharacterConfig characterConfig;

    [Header("角色数据")]
    public string characterID = "李华";
    [TextArea(3, 5)]
    public string characterDesc = "默认单位，初始携带2个增速模块。";

    [Header("视觉表现")]
    public RectTransform rectTransform;
    public float enlargeScale = 1.15f;

    private bool isSelected = false;
    private Vector3 originalScale;
    [Header("安全解锁协议设置")]
    public int unlockCost = 2;
    private bool isUnlocked = false;

    void Start()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        if (characterID == "李华")
        {
            isUnlocked = true;
            PlayerPrefs.SetInt("CharacterUnlocked_" + characterID, 1);
            PlayerPrefs.Save();
        }
        else
        {
            isUnlocked = PlayerPrefs.GetInt("CharacterUnlocked_" + characterID, 0) == 1;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!isUnlocked)
            {
                CharacterSelectManager.Instance.ShowUnlockDialog(this);
                return;
            }

            if (!isSelected)
            {
                isSelected = true;
                rectTransform.localScale = originalScale * enlargeScale;
                CharacterSelectManager.Instance.ShowDescription(this, characterDesc);
            }
            else
            {
                // =================== 【最高优先直连通道：同步解冻并投影】 ===================
                if (characterConfig != null)
                {
                    PlayerDataBridge.SelectedCharacter = characterConfig;

                    // 双击确认时，如果保险机制生效，也需要同步强制将照片槽 SetActive(true) 唤醒！
                    if (CharacterSelectManager.Instance.selectedCharacterDisplay != null && characterConfig.characterIcon != null)
                    {
                        CharacterSelectManager.Instance.selectedCharacterDisplay.gameObject.SetActive(true); // 唤醒区域
                        CharacterSelectManager.Instance.selectedCharacterDisplay.sprite = characterConfig.characterIcon;
                    }
                }
                // ==========================================================================

                CharacterSelectManager.Instance.ConfirmCharacter(characterID);
            }
        }
    }

    public void OnUnlockSuccess()
    {
        isUnlocked = true;
        Debug.Log($"[系统记录] 人格 {characterID} 现已接入。");
    }

    public void ResetState()
    {
        isSelected = false;
        rectTransform.localScale = originalScale;
    }
}