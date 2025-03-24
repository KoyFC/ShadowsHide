using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private int m_MusicIndex = 0;

    private void Start()
    {
        SoundsManager.Instance.PlayMusic(m_MusicIndex);
    }

    public void FullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }
}
