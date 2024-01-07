using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObiParticleGroupExtension
{
    public static int GetNearTransformParticleIndex(this ObiParticleGroup obiParticleGroup, ObiRope obiRope, Vector3 targetPosition)
    {
        var distance = 100000f;
        var outParticleIndex = 0;
        foreach (var particleIndex in obiParticleGroup.particleIndices)
        {
            var particlePosition = obiRope.GetParticleWorldPosition(particleIndex);
            var currentDistance = Vector3.Distance(targetPosition, particlePosition);
            if (currentDistance <= distance)
            {
                distance = currentDistance;
                outParticleIndex = particleIndex;
            }
        }

        return outParticleIndex;
    }
}
