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

public enum EBoidState
{
    FollowArmy,
    Idle,
    FindTarget,
    MoveToTarget,
    Attack,
}

public class BaseBoidState
{
    public Boid Owner { get; set; }
    public bool StopMove { get; set; }

    public virtual void Enter()
    {
        
    }
    
    public virtual void Update()
    {
        
    }
}

public class AttackState : BaseBoidState
{
    public Boid target;
    public override async void Enter()
    {
        this.Owner.GetComponent<Animation>().Play();
        await Task.Delay(1);
        target.health -= 20;

        var armyGroup = Owner.armyGroup;
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * Owner.targetOffset;
        
        //如果距离大于这个数，只是打一下就返回自己的位置
        if (Vector3.Distance(armyTargetPosition, target.transform.position) > 1)
        {
            Owner.ChangeState(new ReturnFormationPositionState());
        }
        else
        {
            Enter();
        }
    }
}

public class MoveToTargetState : BaseBoidState
{
    public Boid target;

    public override void Enter()
    {
        Owner.maxSpeed = Owner.armyGroup.config.speed;
    }

    public override void Update()
    {
        this.Owner.UpdateToTarget(target.position);
        if (Vector3.Distance(target.position, Owner.transform.position) < 0.2f)
        {
            this.Owner.ChangeState(new AttackState(){target = target});
        }
    }
}

public class FollowArmyGroupState : BaseBoidState
{
    public override void Enter()
    {
        base.Enter();
        Owner.maxSpeed = Owner.armyGroup.config.speed;
    }

    public override void Update()
    {
        var armyGroup = Owner.armyGroup;
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * Owner.targetOffset;
        Owner.UpdateToTarget(armyTargetPosition);

        var distance = Vector3.Distance(Owner.transform.position, armyTargetPosition);

        if (Owner.armyGroup.State == EArmyGroupState.Attack)
        {
            if (distance < 0.2f)
            {
                Owner.maxSpeed = 2;
                Owner.ChangeState(new FindTargetState());
            }
        }

        if (distance > 1)
        {
            Owner.maxSpeed = armyGroup.config.speed * 2;
        }
        else
        {
            Owner.maxSpeed = armyGroup.config.speed;
        }
    }
}

public class ReturnFormationPositionState : BaseBoidState
{
    public bool _waiting = false;
    public float _waitTime; 
    public override void Enter()
    {
        base.Enter();
        _waitTime = Random.Range(4, 11);
        Owner.maxSpeed = Owner.armyGroup.config.speed;
    }

    public override void Update()
    {
        base.Update();
        
        var armyGroup = Owner.armyGroup;
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * Owner.targetOffset;
        Owner.UpdateToTarget(armyTargetPosition);

        if (_waiting)
        {
            _waitTime -= Time.deltaTime;
            if (_waitTime < 0)
            {
                Owner.ChangeState(new FindTargetState());
            }
        }
        else if (Vector3.Distance(Owner.transform.position, armyTargetPosition) < 0.5f)
        {
            _waiting = true;
        }
    }
}

public class FindTargetState : BaseBoidState
{
    public override async void Enter()
    {
        
    }

    public override void Update()
    {
        var boids = BoidManager.Instance.GetBoidsByArmyGroup(Owner.armyGroup.targetArmyGroup);
        boids = boids.Where(d => d.CanBeAttack()).ToList();
        if (boids.Count == 0)
            return;
        var target = boids.OrderBy((d) => Vector3.Distance(Owner.transform.position, d.position)).First();
        if (target == null)
            return;
        if (Vector3.Distance(target.transform.position, Owner.transform.position) < 0.5f)
        {
            Owner.ChangeState(new AttackState(){target = target});
        }
        else
        {
            Owner.ChangeState(new MoveToTargetState(){target = target});
        }
    }
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