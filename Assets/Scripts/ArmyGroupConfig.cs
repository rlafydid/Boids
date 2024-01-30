using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyGroupConfig : MonoBehaviour
{
    /*方阵数据*/
    public int row = 4;
    public int column = 4;
    public float spacing = 1;

    /*无阵型数据*/
    public int spawnCount = 10;
    public float spawnRadius = 10;

    public EColor color;

    public EArmyFormation formation = EArmyFormation.Square;

    public float speed = 2;
}
