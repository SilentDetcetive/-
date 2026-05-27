using UnityEngine;

using UnityEngine.SceneManagement;

using System.Threading.Tasks;



public class LevelManager : MonoBehaviour

{

    public static LevelManager Instance;



    private void Awake()

    {

        // 单例模式，确保全局唯一

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject);

            return;

        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

    }



    /// <summary>

    /// 同步加载场景（保留原方法名，原逻辑）

    /// </summary>

    public void LoadLevel(string sceneName)

    {

        SceneManager.LoadScene(sceneName);

    }



    /// <summary>

    /// 异步加载场景，避免大场景卡死（可选使用）

    /// </summary>

    public async void LoadLevelAsync(string sceneName)

    {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.allowSceneActivation = true;



        while (!asyncLoad.isDone)

        {

            await Task.Yield();

        }

    }



    /// <summary>

    /// 使用原来的接口重新加载当前关卡

    /// </summary>

    public void ReloadCurrentLevel()

    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }



    /// <summary>

    /// 保留原来加载主菜单的接口

    /// </summary>

    public void LoadMainMenu()

    {

        LoadLevel("MainMenu"); // 请确保 MainMenu 场景在 Build Settings 中

    }



    // ======================

    // 注释掉原来的倒计时相关内容

    // ======================

    // private float countdownTime = 0;

    // private bool isCountingDown = false;



    // public void StartCountdown(float time)

    // {

    //     countdownTime = time;

    //     isCountingDown = true;

    // }



    // private void Update()

    // {

    //     if (isCountingDown)

    //     {

    //         countdownTime -= Time.deltaTime;

    //         if (countdownTime <= 0)

    //         {

    //             isCountingDown = false;

    //             ReloadCurrentLevel();

    //         }

    //     }

    // }

}