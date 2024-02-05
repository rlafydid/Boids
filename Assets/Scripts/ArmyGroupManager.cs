using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum EColor
{
    Blue,
    Red
}

public enum EArmyGroupState
{
    Idle,
    Move,
    Attack,
    Death
}

public enum EArmyFormation
{
    None,
    Square,
    Circular
}

public interface IFormationCollider
{
    bool Intersection(ArmyGroup formation);
}

public class Circle : IFormationCollider
{
    private ArmyGroup _armyGroup;
    public float radius;
    public Circle(ArmyGroup armyGroup)
    {
        this._armyGroup = armyGroup;
        radius = armyGroup.config.spawnRadius;
    }
    public bool Intersection(ArmyGroup armyGroup)
    {
        switch (armyGroup.formationCollider)
        {
            case Rectangle rectangleFormation:
                return Vector3.Distance(armyGroup.center, this._armyGroup.center) <
                       radius + (rectangleFormation.width + rectangleFormation.height) / 2;
            case Circle circleFormation:
                return Vector3.Distance(armyGroup.center, this._armyGroup.center) < circleFormation.radius + radius;
        }

        return false;
    }
}

public class Rectangle : IFormationCollider
{
    public List<Vector3> points = new();
    public float width;
    public float height;

    private ArmyGroup _armyGroup;
    
    public Rectangle(ArmyGroup armyGroup)
    {
        _armyGroup = armyGroup;
        width = armyGroup.config.column * armyGroup.config.spacing;
        height = armyGroup.config.row * armyGroup.config.spacing;
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        //左下，左上，右上，右下
        Vector3 p1 =  new Vector3(-halfWidth, 0, -halfHeight);
        Vector3 p2 =  new Vector3(-halfWidth, 0, halfHeight);
        Vector3 p3 = new Vector3(halfWidth, 0, halfHeight);
        Vector3 p4 = new Vector3(halfWidth, 0, -halfHeight);
        points = new() { p1, p2, p3, p4};
    }

    public bool Intersection(ArmyGroup target)
    {
        switch (target.formationCollider)
        {
            case Rectangle targetRectangle:
                //判断目标矩形的四个点是否在我方矩形内
                foreach (var point in targetRectangle.points)
                {
                    var p = target.center + target.rotation * point;
                    if(IsInRectangle(points, _armyGroup.rotation, _armyGroup.center, p))
                        return true;
                }

                //判断我方矩形的四个点是否在敌方矩形内
                foreach (var point in points)
                {
                    var p = _armyGroup.center + _armyGroup.rotation * point;
                    if (IsInRectangle(targetRectangle.points, target.rotation, target.center, p))
                        return true;
                }
                break;
            case Circle circle:
                return circle.Intersection(_armyGroup);
        }
        return false;
    }
    
    bool IsInRectangle(List<Vector3> rectanglePoints, Quaternion rotation, Vector3 center, Vector3 point)
    {
        point.y = 0;

        for (int i = 1; i <= rectanglePoints.Count; i++)
        {
            Vector3 lastP = center + rotation * rectanglePoints[i-1];
            Vector3 curP = center + rotation * rectanglePoints[i == rectanglePoints.Count ? 0 : i];
            if (Vector3.Dot((curP - lastP).normalized, (point - lastP).normalized) < 0)
            {
                return false;
            }
        }

        return true;
    }
}

public class ArmyGroup
{
    public Vector3 center;

    public Transform target;
    public Quaternion rotation;
    public EColor color;

    public ArmyGroup targetArmyGroup;

    public ArmyGroupConfig config;
    
    public EArmyGroupState State { get; set; }

    public IFormationCollider formationCollider;
    
    public Vector3 TargetPosition
    {
        get => targetArmyGroup.center;
    }

    public void Start()
    {
        switch (config.formation)
        {
            case EArmyFormation.Square:
                formationCollider = new Rectangle(this);
                break;
            case EArmyFormation.Circular:
                formationCollider = new Circle(this);
                break;
        }
    }
    
    public void Update()
    {
        targetArmyGroup = ArmyGroupManager.Inst.GetNearestOpponentArmyGroup(this);
        if (targetArmyGroup == null)
            return;
        
        var dir = targetArmyGroup.center - center;
        
        if(State != EArmyGroupState.Attack)
            center += Time.deltaTime * dir.normalized * config.speed;
        
        if(dir.magnitude > 0.1f)
            rotation = Quaternion.LookRotation(dir, Vector3.up);
        
        if (targetArmyGroup.IsCollision(this))
        {
            State = EArmyGroupState.Attack;
        }
    }

    public bool IsCollision(ArmyGroup group)
    {
        return formationCollider.Intersection(group);
    }
}

public class ArmyGroupManager
{
    public static ArmyGroupManager Inst = new();
    public List<ArmyGroup> groups = new();

    public ArmyGroup GetNearestOpponentArmyGroup(ArmyGroup armyGroup)
    {
        float minDistance = float.MaxValue;
        ArmyGroup nearestArmyGroup = null;
        var opponent = armyGroup.color.GetOpponent();
        foreach (var group in groups)
        {
            var distance = Vector3.Distance(group.center, armyGroup.center);
            if (group.color == opponent && distance < minDistance)
            {
                minDistance = distance;
                nearestArmyGroup = group;
            }
        }

        return nearestArmyGroup;
    }
}

public static class ArmyGroupExtension
{
    public static EColor GetOpponent(this EColor color)
    {
        return color == EColor.Blue ? EColor.Red : EColor.Blue;
    }
}

