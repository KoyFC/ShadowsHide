using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private GameObject m_Player;
    private GameObject m_Boss;
    public GameObject m_MainCamera;

    void Awake()
    {
        SoundsManager.Instance.PlayMusic(0);
    }

    private void Start()
    {
        //m_Player = GameObject.FindGameObjectWithTag("Player");
        //m_Boss = GameObject.FindGameObjectWithTag("Boss");
        //m_MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        //AudioSource[] m_AudioSources;
        //ChangeVolume(-25);

        //m_AudioSources = m_MainCamera.GetComponentsInChildren<AudioSource>();

        //foreach (AudioSource source in m_AudioSources) { source.enabled = false; }  
    }

    //private void Update()
    //{
    //    if (m_Boss.GetComponent<EnemyScript>().m_ActivateMusic)
    //    {
    //        ActivateMusic();
    //        m_Boss.GetComponent<EnemyScript>().m_ActivateMusic = false;
    //    }
    //}

    public void FullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }
    public void ChangeVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }
    //public void ActivateMusic()
    //{
        
    //}
}
