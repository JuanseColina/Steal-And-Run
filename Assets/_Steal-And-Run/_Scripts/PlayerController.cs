using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private MoneyHolder moneyHolder;
    public MoneyHolder MoneyHolder => moneyHolder;

    
    private float _moveFactorX, _lastFrameFingerPositionX;

    [SerializeField] Animator m_Animator;
    
    [SerializeField] float m_HorizontalSpeedFactor = 0.5f;

    [SerializeField] private GameObject robberyBag, smokeInRun;

    public GameObject RobberyBag => robberyBag;
    private ParticleSystem _particleSystem;

    [SerializeField] private GameObject follower;
    //[SerializeField] private Transform pos;
    
    private Rigidbody[] rbRagdoll;
    private SkinnedMeshRenderer bagRobberySkin;

    private bool _isDead;

    private bool _winMode;

    public bool winMode1
    {
        get => _winMode;
        set => _winMode = value;
    }

    
    #region MovParameters
    Vector3 m_LastPosition;
    float m_StartHeight;
    Transform m_Transform;
    bool m_HasInput = false;
    [SerializeField] float m_MaxXPosition;
    float m_XPos;
    float m_ZPos;
    float m_TargetPosition;
    [SerializeField] float m_Speed;
    float m_startSpeed;
    private static readonly int Mode = Animator.StringToHash("Mode");

    #endregion

    public float MSpeed
    {
        get => m_Speed;
        set => m_Speed = value;
    }


    private void Awake()
    {
        m_Transform = transform;
        m_startSpeed = m_Speed;
        CancelMovement();
    }

    private void Start()
    {
        bagRobberySkin = robberyBag.GetComponent<SkinnedMeshRenderer>();
        GameManager.Instance.PlayerController = this;
        rbRagdoll = GetComponentsInChildren<Rigidbody>();
        DisactivateRagdoll(true);
        _particleSystem = smokeInRun.GetComponent<ParticleSystem>();

        moneyHolder.GetComponent<CashCtrl>().UpdateCubePosition(follower.transform, true);
    }

    // Update is called once per frame
    void Update()
    {
        var deltaTime = Time.deltaTime;
        var speed = m_Speed * deltaTime;

        if (m_HasInput)
        {
            m_ZPos += speed;
            float horizontalSpeed = speed * m_HorizontalSpeedFactor;

            float newPositionTarget = Mathf.Lerp(m_XPos, m_TargetPosition, horizontalSpeed);
            float newPositionDifference = newPositionTarget - m_XPos;

            newPositionDifference = Mathf.Clamp(newPositionDifference, -horizontalSpeed, horizontalSpeed);

            m_XPos += newPositionDifference;
        }
        m_Transform.position = new Vector3(m_XPos, m_Transform.position.y, m_ZPos);

        if (m_Transform.position != m_LastPosition && m_HasInput)
        {
            m_Transform.forward = Vector3.Lerp(m_Transform.forward, (m_Transform.position - m_LastPosition).normalized, speed);
        }

        m_LastPosition = m_Transform.position;
        var transform1 = m_Animator.transform;
        var y = transform1.localPosition;
        y.y = 0;
        transform1.localPosition = y;

        
        if (_winMode)
        {
            //if (GameManager.Instance.CanvasManager.Slider() <= 5)
            //{
                //m_startSpeed = 0;
                //StartCoroutine(GameManager.Instance.WinAction());
                //SetAnimation(0);
            //}
        }
        //ChangeSpeedFromSlider();
    }
    
    /// <summary>
    /// Adjust the player's current speed
    /// </summary>
    public void AdjustSpeed(float speed)
    {
        m_Speed += speed;
        m_Speed = Mathf.Max(0.0f, m_startSpeed);
    }
    
    /// <summary>
    /// Sets the target X position of the player
    /// </summary>
    public void SetDeltaPosition(float normalizedDeltaPosition)
    {
        float fullWidth = m_MaxXPosition * 2.0f;
        m_TargetPosition = m_TargetPosition + fullWidth * normalizedDeltaPosition;
        m_TargetPosition = Mathf.Clamp(m_TargetPosition, -m_MaxXPosition, m_MaxXPosition);
        m_HasInput = true;
    }
    
    /// <summary>
    /// Stops player movement
    /// </summary>
    public void CancelMovement()
    {
        m_HasInput = false;
        SetAnimation(0);
    }
    public void StartMovement()
    {
        m_HasInput = true;
        SetAnimation(1);
    }

    public void Punch(string side)
    {
        m_Animator.SetTrigger(side);
    }

    public void ChangeSpeedFromSlider()
    {
        if (!_isDead)
        {
            float sliderValue = GameManager.Instance.CanvasManager.Slider();

            if (sliderValue >= 80)
            {
                m_Speed = m_startSpeed + 6;
                SetRunSpeedAnimation(1.5f);
            }
            else
            {
                if (sliderValue >= 30)
                {
                    _particleSystem.Play();
                    m_Speed = m_startSpeed + 3;
                    SetRunSpeedAnimation(1.3f);
                }
                else if (sliderValue < 30)
                {
                    _particleSystem.Stop();
                    m_Speed = m_startSpeed;
                    SetRunSpeedAnimation(1);
                }
            }
        }
    }

    private void SetAnimation(int number)
    {
        m_Animator.SetInteger(Mode, number);
    }

    private void SetRunSpeedAnimation(float parameter)
    {
        m_Animator.speed = parameter;
    }

    private void DisactivateRagdoll(bool ragdoll)
    {
        foreach (var t in rbRagdoll)
        {
            t.isKinematic = ragdoll;
        }
    }

    public void PlayerDeadMode()
    {
        GameManager.Instance.GameOverAction();
        CancelMovement();
        DisactivateRagdoll(false);
        GetComponent<BoxCollider>().enabled = false;
        m_Animator.enabled = false;
    }
    //
    // public void StopPlayer()
    // {
    //     m_Speed = 0;
    //     m_startSpeed = 0;
    //     SetAnimation(0);
    //     m_HorizontalSpeedFactor = 0;
    //     transform.GetComponent<Rigidbody>().isKinematic = true;
    // }

    public void AddShapesToBag(bool add, float value)
    {
        if (add)
        {
            value += bagRobberySkin.GetBlendShapeWeight(0);
        }
        else
        {
            value -= bagRobberySkin.GetBlendShapeWeight(0);
        }
        bagRobberySkin.SetBlendShapeWeight(0, Mathf.Abs(value));
    }
}
