using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static void LoadSceneClean(string sceneName)
    {
        // 销毁所有 DontDestroyOnLoad 单例
        DestroyIfExists<GameManager>();
        DestroyIfExists<EventManager>();
        DestroyIfExists<IngredientSystem>();
        DestroyIfExists<DataManager>();
        DestroyIfExists<CoffeeOrderManager>();
        DestroyIfExists<UpgradeManager>();
        DestroyIfExists<AchievementManager>();
        DestroyIfExists<ZLayerManager>();

        SceneManager.LoadScene(sceneName);
    }

    static void DestroyIfExists<T>() where T : MonoBehaviour
    {
        T obj = GameObject.FindObjectOfType<T>();
        if (obj != null)
        {
            GameObject.Destroy(obj.gameObject);
        }
    }
}
