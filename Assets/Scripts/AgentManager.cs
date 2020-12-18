using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AgentManager : MonoBehaviour
{
    private List<GameObject> agents = new List<GameObject>();
    public Transform entrance;
    private int index = 0; // index of the agent at the front of the queue

    void Start()
    {
        // collects all agents into List
        agents.AddRange(GameObject.FindGameObjectsWithTag("Agent"));

        // sorts agent List by row, to be used - potentially
        // agents = agents.OrderBy(t => t.GetComponent<Agent>().row).ToList();

        // sorts agent List randomly - our control group
        agents = Randomize(agents);
    }

    void Update()
    {
        // sets the destinations of the agents that are still in the queue
        // first agent goes to "entrance", subsequent agents follow the agent in front of them
        Transform tempTarget = entrance;
        for (int i = index; i < agents.Count; i++) 
        {
            agents[i].GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(tempTarget.position);
            tempTarget = agents[i].transform;
        }
        
        GameObject front = null; // agent at the front of the queue
        if (index < agents.Count) front = agents[index]; // IndexOutOfBounds check

        // if front agent has reached entrance, find and go to its corresponding seat
        if (pathComplete(front)) 
        {
            Agent script = front.GetComponent<Agent>();
            GameObject seat = GameObject.Find("Seat " + "(" + script.row + ", " + script.col + ")");
            front.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(seat.transform.position);
            index++;
        }  
        
        // debugging
        foreach (var agent in agents)
        {
            Debug.DrawLine(agent.transform.position, agent.GetComponent<UnityEngine.AI.NavMeshAgent>().destination);
        }
    }

    private bool pathComplete(GameObject a) 
    {
        if (a == null) return false;

        UnityEngine.AI.NavMeshAgent nav = a.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (!nav.pathPending) // is a path in the process of being computed?
        {
            if (nav.remainingDistance <= nav.stoppingDistance) // has the agent reached the target?
            {
                return true;
            }
        }
        return false;
    }

    private List<GameObject> Randomize(List<GameObject> list) // randomizes a given List
    {
        System.Random rand = new System.Random();
        int i = list.Count;
        while (i > 1) 
        {
            i--;
            int r = rand.Next(i + 1);
            GameObject temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
        return list;
    }
}
