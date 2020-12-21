using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AgentManager : MonoBehaviour
{
    private List<GameObject> agents = new List<GameObject>();
    private int index = 0; // index of the agent at the front of the queue, separates agents inside/outside of the airplane
    private bool[] isRowOccupied = new bool[8];

    public Transform entrance;
    public Material matGreen;
    public Material matRed;

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
        // sets the targets of the agents that are still in the queue (outside of the plane)
        // first agent goes to "entrance", subsequent agents follow the agent in front of them
        Transform tempTarget = entrance;
        for (int i = index; i < agents.Count; i++) 
        {
            agents[i].GetComponent<Agent>().target = tempTarget;
            tempTarget = agents[i].transform;
        }
        
        UpdateDestination();

        GameObject front = null; // agent at the front of the queue
        if (index < agents.Count) front = agents[index]; // IndexOutOfBounds check

        if (pathComplete(front) && isRowOccupied[0] == false) index++;

        // sets the targets of the agents that waiting to stow and sit down (inside of the plane)
        for (int i = 0; i < index; i++) 
        {
            UnityEngine.AI.NavMeshAgent nav = agents[i].GetComponent<UnityEngine.AI.NavMeshAgent>();
            Agent script = agents[i].GetComponent<Agent>();

            if (script.isSeated || !pathComplete(agents[i])) continue;

            int currentRow = script.cRow;
            int previousRow = currentRow;

            // agent is in line to get to respective row
            if (currentRow < script.row)
            {
                // determines the furthest unblocked/unoccupied row 
                for (int j = currentRow + 1; j <= script.row; j++)
                {
                    if (isRowOccupied[j] == true) 
                    {
                        currentRow = j - 1;
                        break;
                    }
                    else currentRow = j;
                }
                
                GameObject row = GameObject.Find("Row " + currentRow);
                script.target = row.transform;

                isRowOccupied[previousRow] = false;
                isRowOccupied[currentRow] = true;

                script.cRow = currentRow;
                nav.SetDestination(script.target.position);
            }
            
            // agent has arrived at respective row
            if (currentRow == script.row && pathComplete(agents[i]) && !script.isSeated) 
            {
                StartCoroutine(StowAndSit(agents[i], script, currentRow));
                script.isSeated = true; // to prevent repeated StowAndSit calls
            }
        }
        
        UpdateDestination();
    }

    private IEnumerator StowAndSit(GameObject a, Agent script, int currentRow)
    {
        a.GetComponent<Renderer>().material = matRed; // stowing indicator

        yield return new WaitForSeconds(3f); // stowing duration of 3 seconds
        
        isRowOccupied[currentRow] = false;
        a.GetComponent<Renderer>().material = matGreen;

        GameObject seat = GameObject.Find("Seat " + "(" + script.row + ", " + script.col + ")"); // move agent to seat
        script.target = seat.transform;
    }

    private void UpdateDestination() // sets the destination for all agents at once
    {
        foreach (var agent in agents)
        {
            UnityEngine.AI.NavMeshAgent nav = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();
            Agent script = agent.GetComponent<Agent>();
            nav.SetDestination(script.target.position);

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
