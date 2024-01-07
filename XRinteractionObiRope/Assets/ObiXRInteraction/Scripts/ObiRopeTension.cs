using Obi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ObiRope))]
[RequireComponent (typeof(RopeCollisionPointsDetector))]
public class ObiRopeTension : MonoBehaviour
{
    [SerializeField] private float strainLimit = 1.1f;

#if UNITY_EDITOR
    [SerializeField] private float debugStrain;

    Vector3 debugViewNextDestination;
#endif

    //HashSet<Transform> grabTransforms = new HashSet<Transform>();

    private Vector3 ropeConnectPos;

    private ObiRope obiRope;

    private RopeCollisionPointsDetector ropeCollisionPointsDetector;

    private Dictionary<Transform, ObiParticleAttachment> grabParticleDict = new Dictionary<Transform, ObiParticleAttachment> ();

    private void Awake()
    {
        this.obiRope = GetComponent<ObiRope>();
        this.ropeCollisionPointsDetector = GetComponent<RopeCollisionPointsDetector>();
    }

    public void AddGrabTransform(Transform grabTransform, ObiParticleAttachment obiParticleAttachment)
    {
        //this.grabTransforms.Add(grabTransform);
        this.grabParticleDict[grabTransform] = obiParticleAttachment;
    }

    public void RemoveGrabTransform(Transform grabTransform)
    {
        //this.grabTransforms.Remove(grabTransform);
        this.grabParticleDict.Remove(grabTransform);
    }

    public void SetRopeConnectPos(Vector3 ropeConnectPos)
    {
        this.ropeConnectPos = ropeConnectPos;
    }

    public bool IsTension(ObiParticleAttachment ropeConnectAttachment)
    {
        if (TryGetGrabPositionNearRopeConnectPos(out var outNearGrabPos))
        {
            var connectIndex = ropeConnectAttachment.particleGroup.GetNearTransformParticleIndex(this.obiRope, ropeConnectAttachment.target.position);
            var connectPosIndex = (ropeConnectAttachment.target.position, connectIndex);
            var nextDestination = GetNextDestination(connectPosIndex).pos;
            var strain = this.obiRope.GetRopeLengthOverDistance(outNearGrabPos.pos, nextDestination);
#if UNITY_EDITOR
            this.debugStrain = strain;
            this.debugViewNextDestination = nextDestination;
#endif
            return strain < strainLimit;
        }
        return false;
    }

    private bool TryGetGrabPositionNearRopeConnectPos(out (Vector3 pos,int index) outNearGrabPos)
    {
        var distance = 10000f;
        (Vector3,int)? nearGrabPos = null;
        foreach (var one in this.grabParticleDict)
        {
            var dist = Vector3.Distance(one.Key.position, this.ropeConnectPos);
            if (dist < distance)
            {
                distance = dist;
                nearGrabPos = (one.Key.position, one.Value.particleGroup.GetNearTransformParticleIndex(this.obiRope, one.Key.position));
            }
        }

        if (nearGrabPos.HasValue)
        {
            outNearGrabPos = nearGrabPos.Value;
            return true;
        }
        outNearGrabPos = (Vector3.zero,0);
        return false;
    }

    private (Vector3 pos, int index) GetNextDestination((Vector3 pos, int index) connectPositionIndex)
    {
        if(this.ropeCollisionPointsDetector.CollisionPointsSortedByParticleIndex.Count == 0)
        {
            return connectPositionIndex;
        }
        var isLastStart = false;
        var last = this.ropeCollisionPointsDetector.CollisionPointsSortedByParticleIndex.Last();
        var first = this.ropeCollisionPointsDetector.CollisionPointsSortedByParticleIndex.First();
        if(Mathf.Abs(last.index - connectPositionIndex.index)<= Mathf.Abs(first.index - connectPositionIndex.index))
        {
            isLastStart = true;
        }

        if(TryGetGrabPositionNearRopeConnectPos(out var outNearGrabPos))
        {
            if (isLastStart)
            {
                this.ropeCollisionPointsDetector.CollisionPointsSortedByParticleIndex.Reverse();
            }
           
            foreach(var one in this.ropeCollisionPointsDetector.CollisionPointsSortedByParticleIndex)
            {
                if(IsInRange(isLastStart, one.index, outNearGrabPos.index))
                {
                    return one;
                }
            }
        }

        return connectPositionIndex;

        bool IsInRange(bool isLastStart, int currentIndex, int grabIndex)
        {
            if (isLastStart)
            {
                if (currentIndex < grabIndex)
                {
                    return true;
                }
            }
            else
            {
                if(currentIndex > grabIndex)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        DrawContactPoints(Color.blue);
    }

    private void DrawContactPoints(Color gizmoColor)
    {
        var cacheColor = Gizmos.color;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(this.debugViewNextDestination, 0.1f);
        Gizmos.color = cacheColor;
    }
}
