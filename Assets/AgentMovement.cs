using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentMovement : MonoBehaviour
{
    private Vector3 starting;
    private Vector3 position1;
    private Vector3 position2;
   // public GameObject[] objects;
    float distanceWanted = 10.0f;
    public int speed;
    public int distance;
    Vector3 neighbor;
    Vector3 diff;

    Vector3 target;
    void Start()
    {
        starting = transform.position;
        position1 = starting;
        position2 = new Vector3(position1.x , position1.y, position1.z-distance);
        transform.position = position1;
        target = position2;
        //objects = GameObject.FindGameObjectsWithTag("Agent");
    }

    void Update()
    {
        //
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);


    }
    void OnTriggerEnter(Collider other)
    {
        
        if (other.tag=="Agent"){
            
            neighbor = other.transform.position;


            diff = transform.position - neighbor;
            diff.y = 0.0f;
            transform.position = neighbor + diff.normalized * distanceWanted;
            other.transform.position = transform.position + (neighbor - transform.position).normalized * distanceWanted;
           // transform.position = neighbor + diff.normalized * distanceWanted;
        }
        /*foreach (GameObject other in objects)
        {
            Vector3 neighbor = other.transform.position;


            Vector3 diff = transform.position - neighbor;
            diff.y = 0.0f;
            transform.position = neighbor + diff.normalized * distanceWanted;
        }*/


       
    }
}
