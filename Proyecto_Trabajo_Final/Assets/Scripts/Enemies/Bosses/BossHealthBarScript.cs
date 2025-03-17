using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossGhostScript;

public class BossHealthBarScript : MonoBehaviour
{
    private GameObject m_Player;
    private GameObject m_Boss;
    public GameObject m_HealthBar;

    private void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Boss = GameObject.FindGameObjectWithTag("Boss");
    }

    private void Update()
    {
        if (m_Boss != null)
        {
            if (m_Boss.GetComponent<EnemyScript>().m_ActivateHealthBar)
            {
                ShowHealthBar();
                m_Boss.GetComponent<EnemyScript>().m_ActivateHealthBar = false;
            }
        }   
    }

    public void ShowHealthBar()
    {
        m_HealthBar.SetActive(true);
    }
}
