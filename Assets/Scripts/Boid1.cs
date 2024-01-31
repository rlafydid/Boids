using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public int health = 100;
    
    private Vector3 targetPosition;

    public float maxSpeed;

    public BaseBoidState state;

    public string currentState;
    
    public void UpdateBoid ()
    {
        state.Update();
        switch (version)
        {
            case EVersion.Legacy:
                UpdateBoidLegacy();
                break;
        }
    }

    private void Start()
    {
        ChangeState(new FollowArmyGroupState());
        this.GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>(this.armyGroup.color.ToString());
    }
    
    public void UpdateToTarget(Vector3 targetPosition)
    {
        Vector3 acceleration = Vector3.zero;

        // float maxSpeed = settings.maxSpeed;
        float maxSpeed = this.maxSpeed;
        Vector3 offsetToTarget = (targetPosition - position);
       
        var centerToTarget = armyGroup.TargetPosition - armyGroup.center;
        // if (Vector3.Dot(centerToTarget.normalized, offsetToTarget.normalized) < -0.9f)
        // {
        //     maxSpeed = 1f;
        // }

        var addWeight = offsetToTarget.magnitude;
        
        acceleration = SteerTowards (offsetToTarget) * (settings.targetWeight + addWeight);
        
        float minSpeed = this.maxSpeed * 0.5f;

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

        if (armyGroup.State == EArmyGroupState.Attack)
        {
            transform.forward = armyGroup.rotation * Vector3.forward;
        }
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

    public void ChangeState(BaseBoidState state)
    {
        this.state = state;
        state.Owner = this; 
        state.Enter();
        currentState = state.GetType().Name;
    }

    public bool CanBeAttack()
    {
        if (state is AttackState || state is FollowArmyGroupState)
            return true;
        return false;
    }
}