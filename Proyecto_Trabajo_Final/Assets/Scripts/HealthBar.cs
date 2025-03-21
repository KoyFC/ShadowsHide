using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image barImage;

    public void UpdateHealthBar(float m_MaxLifePoints, float m_CurrentLifePoints)
    {
        if (gameObject != null)
        {
            if (barImage != null) barImage.fillAmount = m_CurrentLifePoints / m_MaxLifePoints;
            if (m_CurrentLifePoints <= 0)
            {
                Destroy(gameObject);
            }
        }  
    }
}
