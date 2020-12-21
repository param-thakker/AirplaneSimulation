using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public int row;
    public int col;
    public int cRow; // current row
    public bool isSeated;
    public Transform target;

    void Start()
    {
        cRow = 0;
        target = new GameObject().transform;
    }
}

