using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager1 : MonoBehaviour
{
    public static AudioManager1 Instance { get; private set; }

    [Header("音频源")]
    public AudioSource sfxSource;        // 音效
    public AudioSource ambientSource;     // 环境音（用于天气）

    [Header("种植音效")]
    public AudioClip plantSeed;           // 播种
    public AudioClip harvest;             // 收获

    [Header("天气音效")]
    public AudioClip rainSound;           // 下雨声
    public AudioClip windSound;           // 风声

    [Header("攻击音效")]
    public AudioClip attackSound;         // 攻击音效

    [Header("交互音效")]
    public AudioClip interactSound;       // 交互音效（按E/T键）

    [Header("商店音效")]
    public AudioClip buySound;            // 购买音效
    public AudioClip sellSound;           // 出售音效

    void Awake()
    {
        // 场景级单例模式：如果当前场景已经有一个实例，销毁多余的
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // ★★★ 删除了 DontDestroyOnLoad(gameObject); 这样跳转场景时它就会随场景自动销毁 ★★★

        // 如果没有指定音频源，自动添加
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.4f;
        }

        // 添加环境音源
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = 0.5f;
        }
    }

    // ★★★ 新增：当物体被销毁（如跳转场景）时，清空单例引用，防止野指针报错 ★★★
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 播放音效
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log($"播放音效: {clip.name}");
        }
    }

    // 播放环境音（用于天气）
    public void PlayAmbientSound(AudioClip clip, float volume = 0.5f)
    {
        if (ambientSource != null)
        {
            if (clip != null && ambientSource.clip != clip)
            {
                ambientSource.Stop();
                ambientSource.clip = clip;
                ambientSource.volume = volume;
                ambientSource.Play();
                Debug.Log($"播放环境音: {clip.name}");
            }
            else if (clip == null)
            {
                ambientSource.Stop();
                ambientSource.clip = null;
                Debug.Log("停止环境音");
            }
        }
    }

    // 停止环境音
    public void StopAmbientSound()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.clip = null;
        }
    }
}
