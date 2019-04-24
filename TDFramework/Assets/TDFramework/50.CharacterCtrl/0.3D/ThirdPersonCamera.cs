using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    #region 单例
    private static ThirdPersonCamera m_instance = null;
    public static ThirdPersonCamera Instance {
        get {
            if (m_instance == null) {
                m_instance = GameObject.FindObjectOfType<ThirdPersonCamera> ();
                if (m_instance != null)
                    DontDestroyOnLoad (m_instance.gameObject);
            }
            return m_instance;
        }
    }
    #endregion

    #region 字段
    public Transform m_target;

    public float m_rightOffset = 0.0f;
    public float m_defaultDistance = 2.5f;
    public float m_height = 1.4f;
    public float m_smoothFollow = 10.0f;
    public float m_xMouseSensitivity = 3.0f;
    public float m_yMouseSensitivity = 3.0f;
    public float m_yMinLimit = -40.0f;
    public float m_yMaxLimit = 80.0f;

    private Camera m_camera;
    [HideInInspector]
    public float offSetPlayerPivot;
    [HideInInspector]
    public Transform m_currentTarget;
    private Vector3 m_currentTargetPos;

    private Transform m_targetLookAt;
    private float m_mouseY = 0f;
    private float m_mouseX = 0f;
    private float m_distance = 5f;
    private float m_currentHeight = 0.0f;
    private float m_cullingDistance = 0.0f;
    private float m_forward = -1f;
    private Vector3 m_current_cPos;
    private Vector3 m_desired_cPos;
    private float clipPlaneMargin = 0f;
    #endregion

    #region Unity生命周期
    void Start () {

    }
    void FixedUpdate () {
        if (m_target == null || m_targetLookAt == null) return;
    }
    #endregion

    public void Init () {
        if (m_target == null) return;
        m_camera = GetComponent<Camera> ();
        m_currentTarget = m_target;
        m_currentTargetPos = new Vector3 (m_currentTarget.position.x, m_currentTarget.position.y + offSetPlayerPivot, m_currentTarget.position.z);

        m_targetLookAt = new GameObject ("TargetLookAt").transform;
        m_targetLookAt.position = m_currentTarget.position;
        m_targetLookAt.rotation = m_currentTarget.rotation;
        m_targetLookAt.hideFlags = HideFlags.HideInHierarchy;

        m_mouseX = m_currentTarget.eulerAngles.x;
        m_mouseY = m_currentTarget.eulerAngles.y;

        m_distance = m_defaultDistance;
        m_currentHeight = m_height;
    }

    private void CameraMovement () {
        if (m_currentTarget == null) return;

        m_distance = Mathf.Lerp (m_distance, m_defaultDistance, m_smoothFollow * Time.deltaTime);
        m_cullingDistance = Mathf.Lerp (m_cullingDistance, m_distance, Time.deltaTime);
        var camDir = (m_forward * m_targetLookAt.forward) + (m_rightOffset * m_targetLookAt.right);
        camDir = camDir.normalized;

        var targetPos = new Vector3 (m_currentTarget.position.x, m_currentTarget.position.y + offSetPlayerPivot, m_currentTarget.position.z);
        m_currentTargetPos = targetPos;
        m_desired_cPos = targetPos + new Vector3 (0, m_height, 0);
        m_current_cPos = m_currentTargetPos + new Vector3 (0, m_currentHeight, 0);

        RaycastHit hitInfo;
        ClipPlanePoints planePoints = m_camera.NearClipPlanePoints (m_current_cPos + (camDir * (m_distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = m_camera.NearClipPlanePoints (m_desired_cPos + (camDir * m_distance), clipPlaneMargin);
        

    }
    private bool CullingRaycast(Vector3 from, ClipPlanePoints to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;
        if(Physics.Raycast(from, to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            m_cullingDistance = hitInfo.distance;
        }
        if(Physics.Raycast(from, to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if(m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
        }
        if(Physics.Raycast(from, to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if(m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
        }
        if(Physics.Raycast(from, to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if(m_cullingDistance > hitInfo.distance) m_cullingDistance = hitInfo.distance;
        }
        return value;
    }
}