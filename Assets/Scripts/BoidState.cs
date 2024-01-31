using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BaseBoidState
{
    public Boid Owner { get; set; }

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
            Owner.ChangeState(new FollowArmyGroupState());
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
    public bool _waiting = false;
    public float _waitTime; 
    public override void Enter()
    {
        base.Enter();
        Owner.maxSpeed = Owner.armyGroup.config.speed;
        _waitTime = Random.Range(5, 20);
    }

    public override void Update()
    {
        var armyGroup = Owner.armyGroup;
        var armyTargetPosition = armyGroup.center + armyGroup.rotation * Owner.targetOffset;
        Owner.UpdateToTarget(armyTargetPosition);

        var distance = Vector3.Distance(Owner.transform.position, armyTargetPosition);

        if (Owner.armyGroup.State == EArmyGroupState.Attack)
        {
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
        _waitTime = Random.Range(7, 15);
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

