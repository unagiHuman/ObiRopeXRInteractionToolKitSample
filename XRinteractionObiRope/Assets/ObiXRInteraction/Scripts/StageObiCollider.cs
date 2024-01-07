using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObiCollider))]
public class StageObiCollider : MonoBehaviour
{

    ObiCollider selfObiCollider;

    private void Awake()
    {
        this.selfObiCollider = GetComponent<ObiCollider>();
        this.selfObiCollider.Filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
    }
}
