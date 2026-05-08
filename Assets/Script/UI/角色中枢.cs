using UnityEngine;
using TMPro;
using System.Collections; // 引入协程库

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance; // 单例，方便条幅调用

    [Header("UI 面板引用")]
    public GameObject characterSelectPanel; // 左侧的选人主面板
    public GameObject descriptionPanel;     // 右侧的描述面板
    public TextMeshProUGUI descriptionText; // 描述文字

    [Header("条幅动画设置 (向右滑入)")]
    public float slideDistance = 400f; // 条幅从左侧多远的地方开始滑入
    public float slideDuration = 0.35f; // 每个条幅滑动完成所需的时间
    public float staggerTime = 0.08f;   // 依次弹出的间隔时间，营造阶梯错落感

    [Header("条幅引用")]
    public RectTransform[] bannerRects; // 请在这里放入你的 5 个条幅

    [Header("状态记录")]
    private CharacterBannerNode currentSelectedBanner = null;

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

        // 游戏开始时确保它们是关闭的
        characterSelectPanel.SetActive(false);
        descriptionPanel.SetActive(false);
    }

    // 绑定给左上角的按钮
    public void OpenCharacterSelect()
    {
        characterSelectPanel.SetActive(true);
        CloseDescription(); // 每次打开时，确保描述框是收起状态

        // 触发向右滑入的动态展开效果
        if (bannerRects != null && bannerRects.Length > 0)
        {
            if (currentSlideCoroutine != null)
            {
                StopCoroutine(currentSlideCoroutine);
            }
            currentSlideCoroutine = StartCoroutine(SlideBannersIn());
        }

       
    }

    // 显示角色描述
    public void ShowDescription(CharacterBannerNode banner, string desc)
    {
        // 如果之前已经选中了其他条幅，先让那个旧条幅“缩回去”并取消选中状态
        if (currentSelectedBanner != null && currentSelectedBanner != banner)
        {
            currentSelectedBanner.ResetState();
        }

        currentSelectedBanner = banner;
        descriptionText.text = desc;
        descriptionPanel.SetActive(true);
    }

    // 关闭描述框并重置条幅
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

        // 通知条幅更新玩家名字
        if (PlayerNameDisplay.Instance != null)
        {
            PlayerNameDisplay.Instance.UpdateName(characterID);
        }

        // 关闭整个选人界面，回退到选关状态
        characterSelectPanel.SetActive(false);
        CloseDescription();
    }

    // --- 核心动画协程：统筹条幅依次向右滑入 ---
    private IEnumerator SlideBannersIn()
    {
        // 瞬间将所有条幅移动到左侧的起始点，准备滑入
        for (int i = 0; i < bannerRects.Length; i++)
        {
            if (bannerRects[i] != null)
            {
                bannerRects[i].anchoredPosition = originalBannerPositions[i] - new Vector2(slideDistance, 0);
            }
        }

        // 依次为每个条幅开启单独的滑动动画
        for (int i = 0; i < bannerRects.Length; i++)
        {
            if (bannerRects[i] != null)
            {
                StartCoroutine(SlideSingleBanner(bannerRects[i], originalBannerPositions[i]));
                // 核心：等待 staggerTime 之后，再滑入下一个条幅，产生机械展开感
                yield return new WaitForSeconds(staggerTime);
            }
        }
    }

    // --- 单个条幅的平滑缓动 ---
    private IEnumerator SlideSingleBanner(RectTransform banner, Vector2 targetPos)
    {
        float elapsedTime = 0f;
        Vector2 startPos = targetPos - new Vector2(slideDistance, 0);

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;

            // 经典的 Ease-Out 缓动公式，在动画快结束时产生物理阻尼感
            t = t * (2f - t);

            banner.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 动画结束，强制对齐绝对完美位置
        banner.anchoredPosition = targetPos;
    }

   
}