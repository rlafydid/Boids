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

public class ArmyGroup
{
    public Vector3 center;
    public Transform target;
    public Quaternion rotation;
    public EColor color;

    public ArmyGroup targetArmyGroup;

    public EArmyGroupState State { get; set; }

    public Vector3 TargetPosition
    {
        get => targetArmyGroup.center;
    }

    public void Update(float speed)
    {
        targetArmyGroup = ArmyGroupManager.Inst.GetNearestOpponentArmyGroup(this);
        if (targetArmyGroup == null)
            return;
        
        var dir = targetArmyGroup.center - center;

        if (dir.magnitude < 1)
        {
            this.State = EArmyGroupState.Attack;
            return;
        }
        
        center += Time.deltaTime * dir.normalized * speed;
        if(dir.magnitude > 0.1f)
            rotation = Quaternion.LookRotation(dir, Vector3.up);

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

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, SelectedOnly, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;
    public GizmoType showSpawnRegion;

  
    public Transform blueGroups;
    public Transform redGroups;
    
    void Awake () {
        /*
        for (int i = 0; i < spawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate (prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            boid.SetColour (colour); 
        }
        */
        foreach (var group in blueGroups.GetComponentsInChildren<ArmyGroupConfig>())
        {
            group.color = EColor.Blue;
            CreateArmyGroup(group, Vector3.forward);
        }

        foreach (var group in redGroups.GetComponentsInChildren<ArmyGroupConfig>())
        {
            group.color = EColor.Red;
            CreateArmyGroup(group, Vector3.back);
        }
    }

    void CreateArmyGroup(ArmyGroupConfig config, Vector3 forward)
    {
        ArmyGroup armyGroup = new ArmyGroup();
        armyGroup.color = config.color;

        var offsetHorizontalCenter = config.spacing * config.column * 0.5f - config.spacing * 0.5f;
        Vector3 right = Vector3.Cross(Vector3.up, -Vector3.forward);

        for (int i = 0; i < config.row; i++)
        {
            for (int j = 0; j < config.column; j++)
            {
                Vector3 offsetForward = -Vector3.forward * i;
                Vector3 offsetRight = right * j - right * offsetHorizontalCenter;
                
                // Vector3 offset = new Vector3(config.spacing * j, 0, config.spacing * i);
                Vector3 offset = offsetForward + offsetRight;
                
                Vector3 pos = config.transform.position + Quaternion.LookRotation(forward) * offset;
                Boid boid = Instantiate (prefab);
                boid.transform.position = pos;
                boid.transform.rotation = Quaternion.LookRotation(forward);
                boid.SetColour (colour);
                boid.targetOffset = offset;
                boid.armyGroup = armyGroup;
            }
        }

        armyGroup.center = config.transform.position;
        ArmyGroupManager.Inst.groups.Add(armyGroup);
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {

        Gizmos.color = new Color (colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }

}