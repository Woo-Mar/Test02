using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource; // 背景音乐播放器

    public AudioSource sfxSource; // 音效播放器
    public AudioSource makeSource; // make音效播放器
    public AudioSource emptySource; // empty音效播放器

    [Header("Audio Clips")]
    public AudioClip buttonClickSound; // 按钮点击音效
    public AudioClip makeClickSound; // make点击音效
    public AudioClip emptyClickSound; // empty点击音效

    private void Awake()
    {
        // 实现单例模式，确保跨场景不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 确保背景音乐在游戏暂停 (Time.timeScale = 0 或 AudioListener.pause = true) 时依然播放
        bgmSource.ignoreListenerPause = true;

        // 如果想让BGM不受Time.timeScale影响，也可以设置：
        // bgmSource.pitch = 1f; 
    }

    private void Start()
    {
        // 游戏一开始就播放BGM
        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    // 播放按钮音效的方法
    public void PlayButtonSound()
    {
        if (buttonClickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayMakeSound()
    {
        if (makeClickSound != null && makeSource != null)
        {
            makeSource.PlayOneShot(makeClickSound);
        }
    }

    public void PlayEmptySound()
    {
        if (emptyClickSound != null && emptySource != null)
        {
            emptySource.PlayOneShot(emptyClickSound);
        }
    }
}
