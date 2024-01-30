using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public static BoidManager Instance;
    
    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    Boid[] boids;

    public List<Transform> targets;

    public bool dragTarget;
    
    
    void Start ()
    {
        Instance = this;
        boids = FindObjectsOfType<Boid> ();
        foreach (Boid b in boids) {
            b.Initialize (settings, null);
        }
        // Queue<Transform> targetQueue = new(targets);
        // foreach (var group in ArmyGroupManager.Inst.groups)
        // {
        //     group.target = targetQueue.Dequeue();
        // }
    }

    void Update () {
        foreach (var group in ArmyGroupManager.Inst.groups)
        {
            group.Update(settings.maxSpeed);
        }
        if (boids != null) {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++) {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer (numBoids, BoidData.Size);
            boidBuffer.SetData (boidData);

            compute.SetBuffer (0, "boids", boidBuffer);
            compute.SetInt ("numBoids", boids.Length);
            compute.SetFloat ("viewRadius", settings.perceptionRadius);
            compute.SetFloat ("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt (numBoids / (float) threadGroupSize);
            compute.Dispatch (0, threadGroups, 1, 1);

            boidBuffer.GetData (boidData);

            for (int i = 0; i < boids.Length; i++) {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;
                
                boids[i].UpdateBoid ();
                boids[i].drawTarget = dragTarget;
            }

            boidBuffer.Release ();
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var group in ArmyGroupManager.Inst.groups)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(group.center, 1);
        }
    }

    public List<Boid> GetBoidsByArmyGroup(ArmyGroup armyGroup)
    {
        return boids.Where(d => d.armyGroup == armyGroup).ToList();
    }

    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size {
            get {
                return sizeof (float) * 3 * 5 + sizeof (int);
            }
        }
    }
}