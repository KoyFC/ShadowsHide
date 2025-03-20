using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    private PlayerController m_Player;
    private PlayerControls m_PlayerControls;

    public EventSystem m_EventSystem;
    public GameObject PauseButton;
    public GameObject PauseMenuO;
    public GameObject ResumeButton;
    private bool GamePaused = false;

    private void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        m_PlayerControls = m_Player.m_PlayerControls;
        if (m_EventSystem != null) m_EventSystem.SetSelectedGameObject(ResumeButton);
        Resume();
    }

    private void Update()
    {
        if(m_PlayerControls.Player.Pause.triggered)
        {
            if (GamePaused)
            {
                Resume();
                if (m_EventSystem != null) m_EventSystem.SetSelectedGameObject(ResumeButton);
            }
            else
            {
                Pause();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GamePaused)
            {
                Reset();
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (GamePaused)
            {
                Close();
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GamePaused)
            {
                Resume();
            }
        }
    }

    public void Pause()
    {
        GamePaused = true;
        Time.timeScale = 0f;
        PauseButton.SetActive(false);
        PauseMenuO.SetActive(true);
    }

    public void Resume()
    {
        GamePaused = false;
        Time.timeScale = 1f;
        PauseButton.SetActive(true);
        PauseMenuO.SetActive(false);
    }

    public void Reset()
    {
        GamePaused = false;
        Time.timeScale = 1f;
        // PlayerController.m_HasTriggeredBossFight = false;
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        PauseMenuO.SetActive(false);
        m_Player.TriggerDeath();
    }

    public void Close()
    {
        PlayerController.m_HasTriggeredBossFight = false;
        SceneManager.LoadScene("TitleScreen");
    }
}
    
