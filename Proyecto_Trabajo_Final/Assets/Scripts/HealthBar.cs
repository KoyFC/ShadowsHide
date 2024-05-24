using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image barImage;

    public void UpdateHealthBar(float m_MaxLifePoints, float m_CurrentLifePoints)
    {
        barImage.fillAmount = m_CurrentLifePoints / m_MaxLifePoints;
    }
}
