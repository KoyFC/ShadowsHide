using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDamageScript : MonoBehaviour
{
    public int m_DefaultLightDamage = 1;
    public int m_CurrentLightDamage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Intento conseguir el componente SlimeController del objeto con el que colisiono
            EnemyScript thisEnemy = other.GetComponentInParent<EnemyScript>();
            BossProyectileScript thisEnemyProyectile = other.GetComponent<BossProyectileScript>();
            
            if (thisEnemy != null)
            {
                thisEnemy.GetDamage(m_CurrentLightDamage);
            }
            if (thisEnemyProyectile != null)
            {
                thisEnemyProyectile.GetDamage(m_CurrentLightDamage);
            }

            SoundsManager.Instance.PlayEnemyHitSound();
        }
        else if (other.gameObject.CompareTag("Boss"))
        {
            EnemyScript thisBoss = other.GetComponentInParent<EnemyScript>();
            if (thisBoss != null)
            {
                thisBoss.GetDamage(m_CurrentLightDamage * 2);
            }

            //SoundsManager.Instance.PlayEnemyHitSound();
        }
    }
}
