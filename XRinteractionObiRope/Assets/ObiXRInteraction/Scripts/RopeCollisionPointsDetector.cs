using Obi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.WSA;

[RequireComponent(typeof(ObiRope))]
public class RopeCollisionPointsDetector : MonoBehaviour
{
    public List<(Vector3 pos, int index)> CollisionPointsSortedByParticleIndex {  get; private set; }

    [SerializeField] float detectCollisionDistance = 0.05f;

    private ObiContactEventDispatcher contactEventDispatcher;
    private ObiRope selfRope;

    private HashSet<Vector3> collisionPoints = new HashSet<Vector3>();

    private List<int> collisionParticleIndices = new List<int>();

    private void Awake()
    {
        this.selfRope = GetComponent<ObiRope>();
        this.contactEventDispatcher = FindObjectOfType<ObiContactEventDispatcher>();
    }

    private void OnEnable()
    {
        this.contactEventDispatcher.onContacts.AddListener(SolverContacts);
    }

    private void OnDisable()
    {
        this.contactEventDispatcher.onContacts.RemoveListener(SolverContacts);
    }

    /// <summary>
    /// 折れ線を取得。
    /// </summary>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    public List<Vector3> GetSimplePolylineCollisionPoints(Vector3 endPoint)
    {
        return null;
    }

    public void SolverContacts(ObiSolver sender, Oni.Contact[] contacts)
    {
        var world = ObiColliderWorld.GetInstance();
        this.collisionPoints.Clear();
        foreach (var contact in contacts)
        {
            if (contact.distance < this.detectCollisionDistance)
            {
                var collider = world.colliderHandles[contact.bodyB].owner;
                if (collider.GetComponent<StageObiCollider>()==null)
                {
                    continue;
                }
                int simplexIndex = sender.simplices[contact.bodyA];
                var particleInActor = sender.particleToActor[simplexIndex];
                var rope = (particleInActor.actor as ObiRope);
                if (rope == this.selfRope)
                {
                    this.collisionPoints.Add(contact.pointB);
                }
            }
        }
        var tempIndices = FilterOutRopeRegionPoints(this.collisionPoints, this.detectCollisionDistance);
        this.collisionParticleIndices = tempIndices.OrderBy(x => x).ToList();
        this.CollisionPointsSortedByParticleIndex = this.collisionParticleIndices.Select(x => (this.selfRope.solver.transform.TransformPoint(this.selfRope.solver.positions[x]),x)).ToList();
        FilterPolyLineMethod();
    }

    /// <summary>
    /// 直線上に乗っているポイントを削除する
    /// </summary>
    private void FilterPolyLineMethod()
    {
        if (CollisionPointsSortedByParticleIndex == null || CollisionPointsSortedByParticleIndex.Count == 0) return;

        var startIndex = (this.selfRope.GetParticlePosition(0),0);
        var lastIndex = (this.selfRope.GetParticlePosition(this.selfRope.activeParticleCount - 1), this.selfRope.activeParticleCount - 1);

        //insert start last
        CollisionPointsSortedByParticleIndex.Insert(0, startIndex);
        CollisionPointsSortedByParticleIndex.Insert(CollisionPointsSortedByParticleIndex.Count, lastIndex);

        var first = CollisionPointsSortedByParticleIndex[0];
        Vector3? prePos = null;
        Vector3? preDirection = null;
        for (int i = 0; i < CollisionPointsSortedByParticleIndex.Count; i++)
        {
            var one = this.CollisionPointsSortedByParticleIndex[i];

            if (first == one)
            {
                prePos = one.pos;
                continue;
            }

            var currentDirection = (one.pos - prePos.Value).normalized;
            if (preDirection.HasValue && i < CollisionPointsSortedByParticleIndex.Count)
            {
                var dot = Vector3.Dot(currentDirection, preDirection.Value);
                if (Mathf.Abs(dot) >1f-0.1f)
                {
                    this.CollisionPointsSortedByParticleIndex[i] = (one.pos, -1);
                }
            }
           
            preDirection = currentDirection;
        }
        this.CollisionPointsSortedByParticleIndex = this.CollisionPointsSortedByParticleIndex.Where(v => v.index != -1).ToList();
    }

    private HashSet<int> FilterOutRopeRegionPoints(HashSet<Vector3> collisionPoints, float raduis)
    {
        return collisionPoints.SelectMany(_ => SelectManyFilter(_)).Where(v => IsRopeFilter(v)).Select(p=> convertToRopeParticleSimplexIndex(p)).ToHashSet();

        QueryResult[] SelectManyFilter(Vector3 collisionPoint)
        {
            var queryResults = this.selfRope.solver.SphereCast(this.selfRope.solver.transform, raduis, collisionPoint);
            return queryResults;
        }

        bool IsRopeFilter(QueryResult queryResult)
        {
            if (queryResult.distance < raduis)
            {
                var particleInActor = this.selfRope.solver.particleToActor[queryResult.simplexIndex];
                var rope = (particleInActor.actor as ObiRope);
                if (rope == this.selfRope)
                {
                    return true;
                }
            }
            return false;
        }

        int convertToRopeParticleSimplexIndex(QueryResult queryResult)
        {
            //return this.selfRope.solver.transform.TransformPoint(this.selfRope.solver.positions[queryResult.simplexIndex]);
            return queryResult.simplexIndex;
        }
    }



    private void OnDrawGizmos()
    {
        DrawAllContactPoints(Color.green);
        //DrawContactPoints(new Color(0,0,1,0.2f));
    }

  
    private void DrawAllContactPoints(Color gizmoColor)
    {
        if (CollisionPointsSortedByParticleIndex == null) return;
        var cacheColor = Gizmos.color;
        Gizmos.color = gizmoColor;
        foreach (var contactPoint in CollisionPointsSortedByParticleIndex)
        {
            Gizmos.DrawSphere(contactPoint.pos, 0.1f);
        }
        Gizmos.color = cacheColor;
    }

    private void DrawContactPoints(Color gizmoColor)
    {
        if (CollisionPointsSortedByParticleIndex == null) return;
        var cacheColor = Gizmos.color;
        Gizmos.color = gizmoColor;
        foreach (var contactPoint in collisionPoints)
        {
            Gizmos.DrawSphere(contactPoint, 0.1f);
        }
        Gizmos.color = cacheColor;
    }
}
