using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObiSolverExtension
{
    public static QueryResult[] SphereCast(this ObiSolver solver, Transform targetTransform, float radius, Vector3 centerPoint)
    {
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        var query = new QueryShape(QueryShape.QueryType.Sphere, centerPoint, Vector3.zero, 0, radius, filter);
        var xform = new AffineTransform(targetTransform.position, targetTransform.rotation, targetTransform.localScale);
        return solver.SpatialQuery(query, xform);
    }
}
