using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform preview;
    [SerializeField] private Slider zoomSlider;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;
    private float _xMaxSpeed;
    private float _yMaxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _xMaxSpeed = cinemachineFreeLook.m_XAxis.m_MaxSpeed;
        _yMaxSpeed = cinemachineFreeLook.m_YAxis.m_MaxSpeed;
        cinemachineFreeLook.m_XAxis.m_MaxSpeed = 0;
        cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0;
        cinemachineFreeLook.m_CommonLens = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
    }

    public void Zoom()
    {
        //cinemachineFreeLook.m_Lens.FieldOfView -= Input.mouseScrollDelta.y* zoomSpeed;
        preview.localScale = new Vector3(zoomSlider.value, zoomSlider.value, zoomSlider.value);
    }

    public void ResetZoom()
    {
        //cinemachineFreeLook.m_Lens.FieldOfView -= Input.mouseScrollDelta.y* zoomSpeed;
        zoomSlider.value = 1; 
        Zoom();
    }

    private void Move()
    {
        if (Input.GetMouseButton(0))
        {
            cinemachineFreeLook.m_XAxis.m_MaxSpeed = _xMaxSpeed;
            cinemachineFreeLook.m_YAxis.m_MaxSpeed = _yMaxSpeed;
        }
        else
        {
            cinemachineFreeLook.m_XAxis.m_MaxSpeed = 0;
            cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0;
        }
    }
}
