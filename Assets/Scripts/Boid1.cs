using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVersion
{
    Legacy,
    One,
    Two,
    Three
}

public partial class Boid : MonoBehaviour {

    public Vector3 targetOffset;

    public ArmyGroup armyGroup;

    public bool drawTarget;

    public EVersion version;
    

    private Vector3 targetPosition;
    public void UpdateBoid ()
    {
        switch (version)
        {
            case EVersion.Legacy:
                UpdateBoidLegacy();
                break;
            case EVersion.One:
                UpdateBoid1();
                break;
            case EVersion.Two:
                UpdateBoid2();
                break;
        }
    }

    public void UpdateBoid1()
    {
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * targetOffset;

        target = armyGroup.target;
        
        Vector3 acceleration = Vector3.zero;

        // float maxSpeed = offsetToTarget.magnitude;
        float maxSpeed = settings.maxSpeed;
        if (target != null) {
            targetPosition = target.position;
            Vector3 offsetToTarget = (targetPosition - position);
           

            // var centerToTarget = armyGroup.target.position - armyGroup.center;
            // if (Vector3.Dot(centerToTarget.normalized, offsetToTarget.normalized) < -0.9f)
            // {
            //     maxSpeed = 0.3f;
            // }
            
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight * offsetToTarget.magnitude;
        }
        float minSpeed = 0;

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            // Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);
            Vector3 offsetToFlockmatesCentre = armyTargetPosition;

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        float testSpeed = maxSpeed;
        
        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp (speed, minSpeed, maxSpeed);
        velocity = dir * speed;
        // velocity = dir * testSpeed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }
    
    
    public void UpdateBoid2()
    {
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * targetOffset;

        // target = armyGroup.target;
        
        Vector3 acceleration = Vector3.zero;

        // float maxSpeed = offsetToTarget.magnitude;
        float maxSpeed = settings.maxSpeed;
            targetPosition = armyTargetPosition;
            Vector3 offsetToTarget = (targetPosition - position);
           
            var centerToTarget = armyGroup.target.position - armyGroup.center;
            if (Vector3.Dot(centerToTarget.normalized, offsetToTarget.normalized) < -0.9f)
            {
                maxSpeed = 1f;
            }
            
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight * offsetToTarget.magnitude;
        float minSpeed = 0;

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp (speed, minSpeed, maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }
    
    
    public void UpdateBoid3()
    {
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * targetOffset;

        target = armyGroup.target;
        
        Vector3 acceleration = Vector3.zero;

        // float maxSpeed = offsetToTarget.magnitude;
        float maxSpeed = settings.maxSpeed;
        if (target != null) {
            targetPosition = target.position;
            Vector3 offsetToTarget = (targetPosition - position);
           

            // var centerToTarget = armyGroup.target.position - armyGroup.center;
            // if (Vector3.Dot(centerToTarget.normalized, offsetToTarget.normalized) < -0.9f)
            // {
            //     maxSpeed = 0.3f;
            // }
            
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight * offsetToTarget.magnitude;
        }
        float minSpeed = 0;

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            // Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);
            Vector3 offsetToFlockmatesCentre = armyTargetPosition;

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        float testSpeed = maxSpeed;
        
        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp (speed, minSpeed, maxSpeed);
        velocity = dir * speed;
        // velocity = dir * testSpeed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }
    
    private void OnDrawGizmos()
    {
        if(drawTarget)
            Gizmos.DrawSphere(targetPosition, 0.5f);
    }
}