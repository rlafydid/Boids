using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyGroup
{
    public Vector3 center;
    public Transform target;
    public float speed = 1;
    public Quaternion rotation;
    public void Update()
    {
        var dir = target.position - center;
        center += Time.deltaTime * dir.normalized * speed;
        if(dir.magnitude > 0.1f)
        rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}

public class ArmyGroupManager
{
    public static ArmyGroupManager Inst = new();
    public List<ArmyGroup> groups = new();
}

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, SelectedOnly, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;
    public GizmoType showSpawnRegion;

    public int row = 4;
    public int column = 10;

    public float spacing = 1;
    
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
        CreateArmyGroup(Vector3.zero);
        CreateArmyGroup(Vector3.forward * 10);
    }

    void CreateArmyGroup(Vector3 center)
    {
        ArmyGroup armyGroup = new ArmyGroup();
        for (int i = -row/2; i < row/2; i++)
        {
            for (int j = -column/2; j < column/2; j++)
            {
                Vector3 offset = new Vector3(spacing * j, 0, spacing * i);
                Vector3 pos = center + offset;
                Boid boid = Instantiate (prefab);
                boid.transform.position = pos;
                boid.transform.forward = Vector3.forward;
                boid.SetColour (colour);
                boid.targetOffset = offset;
                boid.armyGroup = armyGroup;
            }
        }

        armyGroup.center = center;
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