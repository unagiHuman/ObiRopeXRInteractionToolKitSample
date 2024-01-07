
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public interface IClimbGrabbable
{
    public Transform climbTransform { get; }

    public ClimbSettingsDatumProperty climbSettingsOverride { get; set; }
}
