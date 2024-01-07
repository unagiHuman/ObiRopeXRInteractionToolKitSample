using UniRx;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// ObiRopeとの橋渡しオブジェクトを掴む為のクラス
/// </summary>
public class RopeGrabInteractable : XRGrabInteractable, IClimbGrabbable
{
    #region Climb
    const float k_DefaultMaxInteractionDistance = 0.1f;

    [SerializeField]
    [Tooltip("The climb provider that performs locomotion while this interactable is selected. " +
             "If no climb provider is configured, will attempt to find one.")]
    ClimbBaseProvider m_ClimbProvider;

    /// <summary>
    /// The climb provider that performs locomotion while this interactable is selected.
    /// If no climb provider is configured, will attempt to find one.
    /// </summary>
    public ClimbBaseProvider climbProvider
    {
        get => m_ClimbProvider;
        set => m_ClimbProvider = value;
    }

    [SerializeField]
    [Tooltip("Transform that defines the coordinate space for climb locomotion. " +
             "Will use this GameObject's Transform by default.")]
    Transform m_ClimbTransform;

    /// <summary>
    /// Transform that defines the coordinate space for climb locomotion. Will use this GameObject's Transform by default.
    /// </summary>
    public Transform climbTransform
    {
        get
        {
            if (m_ClimbTransform == null)
                m_ClimbTransform = transform;
            return m_ClimbTransform;
        }
        set => m_ClimbTransform = value;
    }

    [SerializeField]
    [Tooltip("Controls whether to apply a distance check when validating hover and select interaction.")]
    bool m_FilterInteractionByDistance = true;

    /// <summary>
    /// Controls whether to apply a distance check when validating hover and select interaction.
    /// </summary>
    /// <seealso cref="maxInteractionDistance"/>
    /// <seealso cref="XRBaseInteractable.distanceCalculationMode"/>
    public bool filterInteractionByDistance
    {
        get => m_FilterInteractionByDistance;
        set => m_FilterInteractionByDistance = value;
    }

    [SerializeField]
    [Tooltip("The maximum distance that an interactor can be from this interactable to begin hover or select.")]
    float m_MaxInteractionDistance = k_DefaultMaxInteractionDistance;

    /// <summary>
    /// The maximum distance that an interactor can be from this interactable to begin hover or select.
    /// Only applies when <see cref="filterInteractionByDistance"/> is <see langword="true"/>.
    /// </summary>
    /// <seealso cref="filterInteractionByDistance"/>
    /// <seealso cref="XRBaseInteractable.distanceCalculationMode"/>
    public float maxInteractionDistance
    {
        get => m_MaxInteractionDistance;
        set => m_MaxInteractionDistance = value;
    }

    [SerializeField]
    [Tooltip("Optional override of locomotion settings specified in the climb provider. " +
             "Only applies as an override if set to Use Value or if the asset reference is set.")]
    ClimbSettingsDatumProperty m_ClimbSettingsOverride;

    /// <summary>
    /// Optional override of climb locomotion settings specified in the climb provider. Only applies as
    /// an override if <see cref="Unity.XR.CoreUtils.Datums.DatumProperty{TValue, TDatum}.Value"/> is not <see langword="null"/>.
    /// </summary>
    public ClimbSettingsDatumProperty climbSettingsOverride
    {
        get => m_ClimbSettingsOverride;
        set => m_ClimbSettingsOverride = value;
    }


    #endregion

    enum FollowState
    {
        No,
        Follow
    }

    enum GrabState
    {
        No,
        Grab
    }

    public enum Climbable
    {
        No,
        Climbable
    }

    public Climbable ClimbableState = Climbable.No;

    [SerializeField] private FollowState followState;

    [SerializeField] private GrabState grabState = GrabState.No;

    [SerializeField] private RopeAttachmentController rope;

    private Rigidbody selfRigidbody;

    private XRDirectInteractor interactor = null;

    private Vector3 grabRopePosition;

    private Vector3 grabRopeDirection;

    private BoolReactiveProperty IsEnableClimbLogic;

    protected override void Awake()
    {
        base.Awake();
        this.followState = FollowState.No;
        this.interactor = null;
        this.selfRigidbody = GetComponent<Rigidbody>();
        this.rope = this.GetComponentInParent<RopeAttachmentController>();
        if(this.rope!=null) this.transform.parent = this.rope?.transform.parent;

        IXRSelectInteractor catcheSelectInteractor = null;
        this.IsEnableClimbLogic = new BoolReactiveProperty(false);
        this.IsEnableClimbLogic.Subscribe(_ =>
        {
            if (_)
            {
                if (m_ClimbProvider != null && this.firstInteractorSelecting.hasSelection)
                {
                    this.followState = FollowState.No;
                    catcheSelectInteractor = this.firstInteractorSelecting;
                    m_ClimbProvider.StartClimbGrab(this, this.firstInteractorSelecting);
                }
                    
            }
            else
            {
                if (m_ClimbProvider != null && catcheSelectInteractor != null)
                {
                    m_ClimbProvider.FinishClimbGrab(catcheSelectInteractor);
                    catcheSelectInteractor = null;
                }
            }
        });
    }

    void Update()
    {
        if (this.grabState == GrabState.Grab && this.ClimbableState == Climbable.Climbable)
        {
            this.IsEnableClimbLogic.Value = true;
        }
        else
        {
            this.IsEnableClimbLogic.Value = false;
        }
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected virtual void OnValidate()
    {
        if (m_ClimbTransform == null)
            m_ClimbTransform = transform;
    }

    /// <inheritdoc />
    protected override void Reset()
    {
        base.Reset();

        selectMode = InteractableSelectMode.Multiple;
        m_ClimbTransform = transform;
    }

    /// <inheritdoc />
    public override bool IsHoverableBy(IXRHoverInteractor interactor)
    {
        return base.IsHoverableBy(interactor) && (!m_FilterInteractionByDistance ||
            GetDistanceSqrToInteractor(interactor) <= m_MaxInteractionDistance * m_MaxInteractionDistance);
    }

    /// <inheritdoc />
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return base.IsSelectableBy(interactor) && (IsSelected(interactor) || !m_FilterInteractionByDistance ||
            GetDistanceSqrToInteractor(interactor) <= m_MaxInteractionDistance * m_MaxInteractionDistance);
    }

    /// <inheritdoc />
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
    }

    /// <inheritdoc />
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
    }


    protected override void Grab()
    {
        this.grabState = GrabState.Grab;
        base.Grab();
        if (firstInteractorSelecting.hasSelection)
        {
            this.rope.AddOrEnableParticleAttachment(this, this.transform);
        }
    }

    protected override void Drop()
    {
        this.grabState = GrabState.No;
        base.Drop();
        this.rope.DisableParticleAttachment(this);
    }

    public void OnObiCollisionEnter(XRDirectInteractor xRDirectInteractor, Vector3 ropePoint, Vector3 ropeDirection)
    {
        if (this.interactor != null) return;
        this.followState = FollowState.Follow;
        this.interactor = xRDirectInteractor;
        SetFollowParameter(ropePoint, ropeDirection);
    }

    public void OnObiCollisionStay(XRDirectInteractor xRDirectInteractor, Vector3 ropePoint, Vector3 ropeDirection)
    {
        SetFollowParameter(ropePoint, ropeDirection);
    }

    public void OnObiCollisionExit(XRDirectInteractor xRDirectInteractor, Vector3 ropePoint, Vector3 ropeDirection)
    {
        this.interactor = null;
        this.followState = FollowState.No;
    }

    private void SetFollowParameter(Vector3 grabRopePosition, Vector3 grabRopeDirection)
    {
        this.grabRopePosition = grabRopePosition;
        this.grabRopeDirection = grabRopeDirection;
    }

    /// <summary>
    /// コントローラーと連動して動かす
    /// </summary>
    private void FixedUpdate()
    {
        if(this.followState == FollowState.Follow && !this.IsEnableClimbLogic.Value)
        {
            this.selfRigidbody.MovePosition(grabRopePosition);
        }
    }

}
