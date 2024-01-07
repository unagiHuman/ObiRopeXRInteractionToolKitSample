using UnityEngine;
using UniRx;
using UnityEngine.XR.Interaction.Toolkit;
using Obi;

[RequireComponent (typeof(ObiRopeTension))]
[RequireComponent(typeof(RopeAttachmentController))]
public class RopeClimbableStateController : MonoBehaviour
{
    public enum ClimbableState
    {
        None,
        Climbable,
    }

    [SerializeField] private ReactiveProperty<ClimbableState> state;

    [SerializeField] private ObiParticleAttachment connectAttachment;

    private RopeAttachmentController ropeAttachmentController;

    private ObiRopeTension ropeTension;

    private IntReactiveProperty grabCount;

    private void Awake()
    {
        this.ropeAttachmentController = GetComponent<RopeAttachmentController> ();
        this.ropeTension = GetComponent<ObiRopeTension> ();
        this.state = new ReactiveProperty<ClimbableState>(ClimbableState.None);
        this.state.Subscribe(_ =>
        {
            if(_ == ClimbableState.Climbable)
            {
                this.ropeAttachmentController.RightRopeGrabInteractable.ClimbableState = RopeGrabInteractable.Climbable.Climbable;
                this.ropeAttachmentController.LeftRopeGrabInteractable.ClimbableState = RopeGrabInteractable.Climbable.Climbable;
            }
            else
            {
                this.ropeAttachmentController.RightRopeGrabInteractable.ClimbableState = RopeGrabInteractable.Climbable.No;
                this.ropeAttachmentController.LeftRopeGrabInteractable.ClimbableState = RopeGrabInteractable.Climbable.No;
            }
        });
        this.grabCount = new IntReactiveProperty(0);
        this.grabCount.Subscribe(_ =>
        {
            if(_ == 0)
            {
                this.state.Value = ClimbableState.None;
            }
        });
    }

    private void OnEnable()
    {
        this.ropeAttachmentController.RightRopeGrabInteractable.selectEntered.AddListener(OnSelectEnteredRight);
        this.ropeAttachmentController.RightRopeGrabInteractable.selectExited.AddListener(OnSelectExitedRight);
        this.ropeAttachmentController.LeftRopeGrabInteractable.selectEntered.AddListener(OnSelectEnteredLeft);
        this.ropeAttachmentController.LeftRopeGrabInteractable.selectExited.AddListener(OnSelectExitedLeft);
    }
    private void OnDisable()
    {
        this.ropeAttachmentController.RightRopeGrabInteractable.selectEntered.RemoveListener(OnSelectEnteredRight);
        this.ropeAttachmentController.RightRopeGrabInteractable.selectExited.RemoveListener(OnSelectExitedRight);
        this.ropeAttachmentController.LeftRopeGrabInteractable.selectEntered.RemoveListener(OnSelectEnteredLeft);
        this.ropeAttachmentController.LeftRopeGrabInteractable.selectExited.RemoveListener(OnSelectExitedLeft);
    }

    private void Update()
    {
        if(this.grabCount.Value > 0)
        {
            UpdateRopeState();
        }
    }

    private void OnSelectEnteredRight(SelectEnterEventArgs args)
    {
        if(this.ropeAttachmentController.TryGetObiParticleAttachment(this.ropeAttachmentController.RightRopeGrabInteractable, out var obiParticleAttachment))
        {
            this.ropeTension.AddGrabTransform(this.ropeAttachmentController.RightRopeGrabInteractable.transform, obiParticleAttachment);
            this.grabCount.Value++;
        }
    }

    private void OnSelectExitedRight(SelectExitEventArgs args)
    {
        this.ropeTension.RemoveGrabTransform(this.ropeAttachmentController.RightRopeGrabInteractable.transform);
        this.grabCount.Value--;
    }

    private void OnSelectEnteredLeft(SelectEnterEventArgs args)
    {
        if (this.ropeAttachmentController.TryGetObiParticleAttachment(this.ropeAttachmentController.LeftRopeGrabInteractable, out var obiParticleAttachment))
        {
            this.ropeTension.AddGrabTransform(this.ropeAttachmentController.LeftRopeGrabInteractable.transform, obiParticleAttachment);
            this.grabCount.Value++;
        }
    }

    private void OnSelectExitedLeft(SelectExitEventArgs args)
    {
        this.ropeTension.RemoveGrabTransform(this.ropeAttachmentController.LeftRopeGrabInteractable.transform);
        this.grabCount.Value--;
    }

    private void UpdateRopeState()
    {
        if (this.ropeTension.IsTension(connectAttachment))
        {
            this.state.Value = ClimbableState.Climbable;
        }
        else
        {
            this.state.Value = ClimbableState.None;
        }
    }

}
