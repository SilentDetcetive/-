using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance; // 单例，方便条幅调用

    [Header("UI 面板引用")]
    public GameObject characterSelectPanel; // 左侧的选人主面板
    public GameObject descriptionPanel;     // 右侧的描述面板
    public TextMeshProUGUI descriptionText; // 描述文字

    [Header("指定图片展示槽位")]
    public Image selectedCharacterDisplay;

    [Header("条幅动画设置 (向右滑入)")]
    public float slideDistance = 400f; // 条幅从左侧多远的地方开始滑入
    public float slideDuration = 0.35f; // 每个条幅滑动完成所需的时间
    public float staggerTime = 0.08f;   // 依次弹出的间隔时间，营造阶梯错落感

    [Header("条幅引用")]
    public RectTransform[] bannerRects; // 请在这里放入你的 5 个条幅

    [Header("状态记录")]
    private CharacterBannerNode currentSelectedBanner = null;
    [Header("角色配置数据资产列表 (放入 C01-C05)")]
    public CharacterConfig[] characterConfigs; // 供你在 Inspector 里拖入 5 个 ScriptableObject 配置文件
    [Header("【新增】解锁面板系统")]
    public GameObject unlockPanel;           // 拖入你的解锁弹窗物体
    public TextMeshProUGUI unlockInfoText;  // 面板上的提示文字
    public Button unlockButton;           // 面板上的解锁确认按钮

    private CharacterBannerNode _pendingNode; // 记忆当前正在请求解锁的条幅

    // 用于动画的内部变量
    private Vector2[] originalBannerPositions;
    private Coroutine currentSlideCoroutine;

    void Awake()
    {
        Instance = this;

        // 游戏开始时，在面板隐藏前，记录所有条幅的初始完美位置
        if (bannerRects != null && bannerRects.Length > 0)
        {
            originalBannerPositions = new Vector2[bannerRects.Length];
            for (int i = 0; i < bannerRects.Length; i++)
            {
                if (bannerRects[i] != null)
                {
                    originalBannerPositions[i] = bannerRects[i].anchoredPosition;
                }
            }
        }
        if (unlockPanel != null) unlockPanel.SetActive(false);
        characterSelectPanel.SetActive(false);
        descriptionPanel.SetActive(false);

        // =================== 【端点注入：显示卡槽默认静默隐藏】 ===================
        if (selectedCharacterDisplay != null)
        {
            selectedCharacterDisplay.gameObject.SetActive(false); // 彻底关闭游戏物体，不留白色方块穿帮
        }
        // =======================================================================
    }

    private void Start()
    {
        // 如果游戏已经选择过角色（例如读取了存档），则允许其初始化展现
        if (PlayerDataBridge.SelectedCharacter != null && selectedCharacterDisplay != null && PlayerDataBridge.SelectedCharacter.characterIcon != null)
        {
            selectedCharacterDisplay.gameObject.SetActive(true);
            selectedCharacterDisplay.sprite = PlayerDataBridge.SelectedCharacter.characterIcon;
        }
    }

    // 绑定给左上角的按钮
    public void OpenCharacterSelect()
    {
        characterSelectPanel.SetActive(true);
        CloseDescription();

        if (bannerRects != null && bannerRects.Length > 0)
        {
            if (currentSlideCoroutine != null)
            {
                StopCoroutine(currentSlideCoroutine);
            }
            currentSlideCoroutine = StartCoroutine(SlideBannersIn());
        }
    }

    public void ShowDescription(CharacterBannerNode banner, string desc)
    {
        if (currentSelectedBanner != null && currentSelectedBanner != banner)
        {
            currentSelectedBanner.ResetState();
        }

        currentSelectedBanner = banner;
        descriptionText.text = desc;
        descriptionPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            HandleGlobalRightClick();
        }
    }

    private void HandleGlobalRightClick()
    {
        if (unlockPanel != null && unlockPanel.activeSelf)
        {
            unlockPanel.SetActive(false);
            Debug.Log("[协议中止] 解锁面板已关闭");
        }
        else if (descriptionPanel != null && descriptionPanel.activeSelf)
        {
            CloseDescription();
            Debug.Log("[协议中止] 描述面板已关闭");
        }
        else if (characterSelectPanel != null && characterSelectPanel.activeSelf)
        {
            characterSelectPanel.SetActive(false);
            Debug.Log("[协议中止] 角色中枢已关闭");
        }
    }

    public void CloseDescription()
    {
        descriptionPanel.SetActive(false);
        if (currentSelectedBanner != null)
        {
            currentSelectedBanner.ResetState();
            currentSelectedBanner = null;
        }
    }

    // 确认选择角色并返回
    public void ConfirmCharacter(string characterID)
    {
        Debug.Log("入侵协议已更新，载入人格: " + characterID);

        if (characterConfigs != null && characterConfigs.Length > 0)
        {
            foreach (CharacterConfig config in characterConfigs)
            {
                if (config != null && config.characterID == characterID)
                {
                    PlayerDataBridge.SelectedCharacter = config;

                    // ------------------ 【核心修改：解冻显示并投影图像】 ------------------
                    if (selectedCharacterDisplay != null && config.characterIcon != null)
                    {
                        selectedCharacterDisplay.gameObject.SetActive(true); // 确认选择的一瞬间，强行将显示槽充能激活！
                        selectedCharacterDisplay.sprite = config.characterIcon;
                    }
                    // ------------------------------------------------------------------

                    Debug.Log($"[系统同步] 人格数据资产全量封存入数据桥: {config.characterName} (速度: {config.moveDuration}, 物品格: {config.inventorySlots})");
                    break;
                }
            }
        }

        if (PlayerNameDisplay.Instance != null)
        {
            PlayerNameDisplay.Instance.UpdateName(characterID);
        }

        characterSelectPanel.SetActive(false);
        CloseDescription();
    }

    private IEnumerator SlideBannersIn()
    {
        for (int i = 0; i < bannerRects.Length; i++)
        {
            if (bannerRects[i] != null)
            {
                bannerRects[i].anchoredPosition = originalBannerPositions[i] - new Vector2(slideDistance, 0);
            }
        }

        for (int i = 0; i < bannerRects.Length; i++)
        {
            if (bannerRects[i] != null)
            {
                StartCoroutine(SlideSingleBanner(bannerRects[i], originalBannerPositions[i]));
                yield return new WaitForSeconds(staggerTime);
            }
        }
    }

    private IEnumerator SlideSingleBanner(RectTransform banner, Vector2 targetPos)
    {
        float elapsedTime = 0f;
        Vector2 startPos = targetPos - new Vector2(slideDistance, 0);

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            t = t * (2f - t);

            banner.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        banner.anchoredPosition = targetPos;
    }

    public void ShowUnlockDialog(CharacterBannerNode node)
    {
        _pendingNode = node;
        unlockPanel.SetActive(true);

        unlockInfoText.text =
                              $"需要消耗：<color=yellow>{node.unlockCost}</color> 数据原型\n" +
                              "是否执行接入协议？";
    }

    public void OnClick_PerformUnlock()
    {
        if (_pendingNode == null) return;

        int myPrototypes = PlayerPrefs.GetInt("DataPrototype", 0);

        if (myPrototypes >= 0)
        {
            PlayerPrefs.SetInt("DataPrototype", myPrototypes);
            PlayerPrefs.SetInt("CharacterUnlocked_" + _pendingNode.characterID, 1);
            PlayerPrefs.Save();

            _pendingNode.OnUnlockSuccess();
            unlockPanel.SetActive(false);

            Debug.Log($"[端点记录] 人格 {_pendingNode.characterID} 已成功接入。");
        }
        else
        {
            Debug.LogWarning("数据原型不足，无法执行解锁协议。");
        }
    }
}