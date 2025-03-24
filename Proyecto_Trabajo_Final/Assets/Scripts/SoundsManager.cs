using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer m_AudioMixer;

    public static SoundsManager Instance = null;
    [Header("Music")]
    [SerializeField] private AudioSource m_MusicAudioSource = null;
    [SerializeField] private AudioClip m_MainMenuMusic = null;
    [SerializeField] private AudioClip m_GameMusic = null;
    [SerializeField] private AudioClip m_BossMusic = null;
    [SerializeField] private AudioClip m_VictoryMusic = null;

    [Header("Player Sounds")]
    [SerializeField] private AudioSource m_PlayerAudioSource = null;
    [Space]
    [SerializeField] private AudioClip m_CheckpointSound = null;
    [Space]
    [SerializeField] private AudioClip m_SummonSound = null;
    [Space]
    [SerializeField] private AudioClip m_WhiteActionSound = null;
    [SerializeField] private AudioClip m_RedActionSound = null;
    [SerializeField] private AudioClip m_BlueActionSound = null;
    [SerializeField] private AudioClip m_GreenActionSound = null;
    [SerializeField] private AudioClip m_YellowActionSound = null;
    [Space]
    [SerializeField] private AudioClip m_HitSound = null;
    [SerializeField] private AudioClip m_DeathSound = null;

    [Header("Enemy Sounds")]
    [SerializeField] private AudioSource m_EnemyAudioSource = null;
    [SerializeField] private AudioClip m_EnemyHitSound = null;
    [SerializeField] private AudioClip m_EnemyDeathSound = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ChangeVolume(-25);
    }

    public void ChangeVolume(float volume)
    {
        m_AudioMixer.SetFloat("Volume", volume);
    }

    public void PlayMusic(int index)
    {
        switch (index)
        {
            case 0:
                m_MusicAudioSource.clip = m_MainMenuMusic;
                break;
            case 1:
                m_MusicAudioSource.clip = m_GameMusic;
                break;
            case 2:
                m_MusicAudioSource.clip = m_BossMusic;
                break;
            case 3:
                m_MusicAudioSource.clip = m_VictoryMusic;
                break;
        }
        m_MusicAudioSource.Play();
    }

    public void PlayCheckpointSound()
    {
        m_PlayerAudioSource.clip = m_CheckpointSound;
        m_PlayerAudioSource.Play();
    }

    public void PlaySummonSound()
    {
        m_PlayerAudioSource.clip = m_SummonSound;
        m_PlayerAudioSource.Play();
    }

    public void PlayActionSound(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0:
                m_PlayerAudioSource.clip = m_WhiteActionSound;
                break;
            case 1:
                m_PlayerAudioSource.clip = m_RedActionSound;
                break;
            case 2:
                m_PlayerAudioSource.clip = m_BlueActionSound;
                break;
            case 3:
                m_PlayerAudioSource.clip = m_GreenActionSound;
                break;
            case 4:
                m_PlayerAudioSource.clip = m_YellowActionSound;
                break;
        }
        m_PlayerAudioSource.Play();
    }

    public void PlayHitSound()
    {
        m_PlayerAudioSource.clip = m_HitSound;
        m_PlayerAudioSource.Play();
    }

    public void PlayDeathSound()
    {
        m_PlayerAudioSource.clip = m_DeathSound;
        m_PlayerAudioSource.Play();
    }

    public void PlayEnemyHitSound()
    {
        m_EnemyAudioSource.clip = m_EnemyHitSound;
        m_EnemyAudioSource.Play();
    }

    public void PlayEnemyDeathSound()
    {
        m_EnemyAudioSource.clip = m_EnemyDeathSound;
        m_EnemyAudioSource.Play();
    }
}
