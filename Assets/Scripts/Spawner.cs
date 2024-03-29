﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        armyGroup.config = config;
        
        switch (config.formation)
        {
            case EArmyFormation.Square:
                CreateSquareFormation(armyGroup, forward);
                break;
            case EArmyFormation.Circular:
                CreateCircleFormation(armyGroup, forward);
                break;
            case EArmyFormation.None:
                CreateSquareFormation(armyGroup, forward);
                break;
        }

        armyGroup.center = config.transform.position;
        armyGroup.Start();
        ArmyGroupManager.Inst.groups.Add(armyGroup);
    }

    void CreateSquareFormation(ArmyGroup armyGroup, Vector3 forward)
    {
        var config = armyGroup.config;
        var offsetHorizontalCenter = config.spacing * config.column * 0.5f - config.spacing * 0.5f;
        Vector3 right = Vector3.Cross(Vector3.up, -Vector3.forward);

        var offsetVerticalCenter = config.spacing * config.row * 0.5f - config.row * 0.5f;
        
        for (int i = 0; i < config.row; i++)
        {
            for (int j = 0; j < config.column; j++)
            {
                Vector3 offsetForward = -Vector3.forward * i * config.spacing + -Vector3.forward * offsetVerticalCenter;
                Vector3 offsetRight = right * j * config.spacing - right * offsetHorizontalCenter;
                
                // Vector3 offset = new Vector3(config.spacing * j, 0, config.spacing * i);
                Vector3 offset = offsetForward + offsetRight;
                
                Vector3 pos = config.transform.position + Quaternion.LookRotation(forward) * offset;
                Boid boid = Instantiate (prefab);
                boid.transform.position = pos;
                boid.transform.rotation = Quaternion.LookRotation(forward);
                boid.SetColour (colour);
                boid.targetOffset = offset;
                boid.armyGroup = armyGroup;
                boid.maxSpeed = armyGroup.config.speed;
            }
        }
    }

    void CreateNoneFormation(ArmyGroup armyGroup, Vector3 forward)
    {
        for (int i = 0; i < spawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate (prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            boid.SetColour (colour); 
        }
    }
    
    void CreateCircleFormation(ArmyGroup armyGroup, Vector3 forward)
    {
        for (int i = 0; i < armyGroup.config.spawnCount; i++) {
            var circle = Random.insideUnitCircle * armyGroup.config.spawnRadius;
            Vector3 offset = new Vector3(circle.x, 0, circle.y);
            Vector3 pos = armyGroup.config.transform.position + offset;
            Boid boid = Instantiate (prefab);
            boid.transform.position = pos;
            boid.transform.forward = forward;
            boid.armyGroup = armyGroup;
            boid.maxSpeed = armyGroup.config.speed;
            boid.targetOffset = offset;
        }
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