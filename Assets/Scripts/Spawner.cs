using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyGroup
{
    public Vector3 center;
}

public class ArmyGroupManager
{
    public ArmyGroup group;
}

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, SelectedOnly, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;
    public GizmoType showSpawnRegion;

    public int row = 4;
    public int column = 4;

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
        Vector3 center = Vector3.zero;
        for (int i = -row/2; i < row/2; i++)
        {
            for (int j = -column/2; j < column/2; j++)
            {
                Vector3 offset = new Vector3(spacing * i, 0, spacing * j);
                Vector3 pos = transform.position + offset;
                Boid boid = Instantiate (prefab);
                boid.transform.position = pos;
                boid.transform.forward = Vector3.forward;
                boid.SetColour (colour);
                boid.targetOffset = offset;
            }
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