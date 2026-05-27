using UnityEngine;
using System.Collections;

public class RedBarLoader : MonoBehaviour
{
    public RectTransform redBarRect; // 拖入你的 LoadingBar_Red
    public float loadTime = 3.0f;    // 加载总时间

    void OnEnable()
    {
        // 每次面板打开时，自动触发延伸
        StartCoroutine(ExtendRedBar());
    }

    IEnumerator ExtendRedBar()
    {
        // 初始状态：缩放为 0（隐藏）
        redBarRect.localScale = new Vector3(0, 1, 1);

        float timer = 0f;
        while (timer < loadTime)
        {
            timer += Time.unscaledDeltaTime; // 使用不受暂停影响的时间
            float progress = Mathf.Clamp01(timer / loadTime);

            // 改变 X 轴的缩放，从左向右延伸
            redBarRect.localScale = new Vector3(progress, 1, 1);

            yield return null;
        }

        // 确保最后刚好填满
        redBarRect.localScale = new Vector3(1, 1, 1);
    }
}