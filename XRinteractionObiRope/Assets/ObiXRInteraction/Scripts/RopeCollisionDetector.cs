using Obi;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// XRDirectInteractorがあるGameObjectにアタッチするObiRopeの衝突検知
/// </summary>
[RequireComponent(typeof(XRDirectInteractor))]
[RequireComponent(typeof(ObiCollider))]
public class RopeCollisionDetector : MonoBehaviour
{
    public bool IsLeft { get; private set;}

    private ObiContactEventDispatcher contactEventDispatcher;

    private XRDirectInteractor interactor;

    ObiCollider selfCollider;

    private void Awake()
    {
        this.selfCollider = GetComponent<ObiCollider>();
        this.contactEventDispatcher = FindObjectOfType<ObiContactEventDispatcher>();
        var interactionGroup = GetComponentInParent<XRInteractionGroup>();

        //XRInteractionToolKit(ver2.5.2)のStarterAssetsのXR Interaction Setupには、右手、左手を区別できるパラメタが設定されている
        if (interactionGroup.groupName == XRInteractionGroup.GroupNames.k_Left)
        {
            this.IsLeft = true;
        }
        this.interactor = GetComponent<XRDirectInteractor>();
    }

    private void OnEnable()
    {
        this.contactEventDispatcher.onContactEnter.AddListener(SolverContact_OnCollisionEnter);
        this.contactEventDispatcher.onContactStay.AddListener(SolverContact_OnCollisionStay);
        this.contactEventDispatcher.onContactExit.AddListener(SolverContact_OnExit);
    }

    private void OnDisable()
    {
        this.contactEventDispatcher.onContactEnter.RemoveListener(SolverContact_OnCollisionEnter);
        this.contactEventDispatcher.onContactStay.RemoveListener(SolverContact_OnCollisionStay);
        this.contactEventDispatcher.onContactExit.RemoveListener(SolverContact_OnExit);
    }

    public void SolverContact_OnCollisionEnter(ObiSolver sender, Oni.Contact contact)
    {
        AnalyzeContact(sender, contact, (ObiRope obiRope, Vector3 projectPos, Vector3 ropeDirection) =>
        {
            if (this.IsLeft)
            {
                obiRope.GetComponent<RopeAttachmentController>().LeftRopeGrabInteractable.OnObiCollisionEnter(this.interactor, projectPos, ropeDirection);
            }
            else
            {
                obiRope.GetComponent<RopeAttachmentController>().RightRopeGrabInteractable.OnObiCollisionEnter(this.interactor, projectPos, ropeDirection);
            }
        });
    }

    public void SolverContact_OnCollisionStay(ObiSolver sender, Oni.Contact contact)
    {
        AnalyzeContact(sender, contact, (ObiRope obiRope, Vector3 projectPos, Vector3 ropeDirection) =>
        {
            if (this.IsLeft)
            {
                obiRope.GetComponent<RopeAttachmentController>().LeftRopeGrabInteractable.OnObiCollisionStay(this.interactor, projectPos, ropeDirection);
            }
            else
            {
                obiRope.GetComponent<RopeAttachmentController>().RightRopeGrabInteractable.OnObiCollisionStay(this.interactor, projectPos, ropeDirection);
            }
        });
    }

    public void SolverContact_OnExit(ObiSolver sender, Oni.Contact contact)
    {
        AnalyzeContact(sender, contact, (ObiRope obiRope, Vector3 projectPos, Vector3 ropeDirection) =>
        {
            if (this.IsLeft)
            {
                obiRope.GetComponent<RopeAttachmentController>().LeftRopeGrabInteractable.OnObiCollisionExit(this.interactor, projectPos, ropeDirection);
            }
            else
            {
                obiRope.GetComponent<RopeAttachmentController>().RightRopeGrabInteractable.OnObiCollisionExit(this.interactor, projectPos, ropeDirection);
            }
        });
    }

    private void AnalyzeContact(ObiSolver sender, Oni.Contact contact, System.Action<ObiRope, Vector3, Vector3> OnCollisionAction)
    {
        int simplexIndex = sender.simplices[contact.bodyA];
        var particleInActor = sender.particleToActor[simplexIndex];

        var world = ObiColliderWorld.GetInstance();
        var contactCollider = world.colliderHandles[contact.bodyB].owner;

        if ((particleInActor.actor is ObiRope) && contactCollider == selfCollider)
        {
            var obiRope = particleInActor.actor as ObiRope;
            if (obiRope.TryGetNearestParticleIndex(this.transform.position, out var outParticleIndex))
            {
                if (obiRope.TryGetRopeProjectionPosition(this.transform.position, outParticleIndex, sender, out var projectionPosition, out var outRopeDirection))
                {
                    OnCollisionAction?.Invoke(obiRope, projectionPosition, outRopeDirection);
                }
            }
        }
    }
}
