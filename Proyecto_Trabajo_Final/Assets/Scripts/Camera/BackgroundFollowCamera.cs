using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFollowCamera : MonoBehaviour
{
    private Transform m_CameraToFollow;
    public float m_VerticalOffset = 0.0f;

    void Awake()
    {
        m_CameraToFollow = Camera.main.transform;
    }

    void Start()
    {
        transform.position = new Vector3(m_CameraToFollow.position.x, m_CameraToFollow.position.y - m_VerticalOffset, transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Follow the camera on the y position
        transform.position = new Vector3(transform.position.x, m_CameraToFollow.position.y - m_VerticalOffset, transform.position.z);
    }
}