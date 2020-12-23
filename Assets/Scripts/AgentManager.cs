using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AgentManager : MonoBehaviour
{
    private List<GameObject> agents = new List<GameObject>();
    private int index = 0; // index of the agent at the front of the queue, separates agents inside/outside of the airplane
    private int selection = -1; //determines whether or not a type of boarding has been selected
    private int interactions = 0; //counter for interactions
    private bool[] isRowOccupied = new bool[11];
    private Vector3 ahead = Vector3.forward * 1.5f;

    public Transform entrance;
    public Material matGreen;
    public Material matRed;
    public Button random;
    public Button boarding;
    public Button efficient;
    public GameObject full;
    public GameObject alternate;
    public GameObject cross;

    void Start()
    {
        random.onClick.AddListener(FullEnabler);
        boarding.onClick.AddListener(AltEnabler);
        efficient.onClick.AddListener(CrossEnabler);
    }

    void Update()
    {
        // sets the targets of the agents that are still in the queue (outside of the plane)
        // first agent goes to "entrance", subsequent agents follow the agent in front of them
        if (selection != -1)
        {
            Transform tempTarget = entrance;
            for (int i = index; i < agents.Count; i++)
            {
                agents[i].GetComponent<Agent>().target = tempTarget;
                tempTarget = agents[i].transform;
            }

            UpdateDestination();

            GameObject front = null; // agent at the front of the queue
            if (index < agents.Count) front = agents[index]; // IndexOutOfBounds check

            if (pathComplete(front) && isRowOccupied[0] == false) 
            {
                front.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 1;
                index++;
            }

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
                    script.status = agents[i].name + " is heading to row " + script.row;
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
                    // Debug.Log(agents[i].name + ": " + script.target.position);
                    nav.SetDestination(script.target.position);
                }

                // agent has arrived at respective row
                if (currentRow == script.row && pathComplete(agents[i]) && !script.isSeated)
                {
                    script.status = agents[i].name + " is stowing their luggage";
                    StartCoroutine(StowAndSit(agents[i], script, currentRow));
                    script.isSeated = true; // to prevent repeated StowAndSit calls
                }
            }
            UpdateDestination();
        }
    }

    private IEnumerator StowAndSit(GameObject a, Agent script, int currentRow)
    {
        a.GetComponent<Renderer>().material = matRed; // stowing indicator

        yield return new WaitForSeconds(3f); // stowing duration of 3 seconds
        
        a.GetComponent<Renderer>().material = matGreen;

        GameObject seat = GameObject.Find("Seat " + "(" + script.row + ", " + script.col + ")"); // move agent to seat
        Vector3 ray = seat.transform.position - a.transform.position;
        ray.y = 0;
        float distance = ray.magnitude;
        RaycastHit[] hits = Physics.RaycastAll(a.transform.position, ray, distance);
        List<GameObject> otherAgents = new List<GameObject>();
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.tag.Equals("Agent"))
            {
                otherAgents.Add(hit.collider.gameObject);
                Debug.Log(a.name + " has found " + hit.collider.gameObject.name + " in their way");
                Debug.DrawLine(a.transform.position, hit.collider.gameObject.transform.position, Color.red);
            }
        }

        script.status = a.name + " is preparing to sit";

        if (otherAgents.Count != 0) //when there are other agents in this agent's way
        {
            GameObject tempPosition = new GameObject("Temp Position of " + a.name);
            tempPosition.transform.position = a.transform.position + ahead;
            script.target = tempPosition.transform;
            foreach (var ag in otherAgents)
            {
                GameObject row = GameObject.Find("Row " + script.row);
                ag.GetComponent<Agent>().target = row.transform;
            }
            script.status = a.name + " is waiting for other passengers, so " + a.name + " can get to their seat";
            yield return new WaitForSeconds(2f); // Allow seated passengers to move out of the way
            script.target = seat.transform;
            Destroy(tempPosition);

            yield return new WaitForSeconds(1.5f); //Wait for new passenger to get to seat
            foreach (var ag in otherAgents)
            {
                seat = GameObject.Find("Seat " + "(" + ag.GetComponent<Agent>().row + ", " + ag.GetComponent<Agent>().col + ")");
                ag.GetComponent<Agent>().target = seat.transform;
            }
            isRowOccupied[currentRow] = false;
        } else
        {
            isRowOccupied[currentRow] = false;
            script.target = seat.transform;
        }

        script.status = a.name + " is seated";

        
    }

    private void FullEnabler()
    {
        Selector();
    }

    private void AltEnabler()
    {
        full.SetActive(false);
        alternate.SetActive(true);
        cross.SetActive(false);
        Selector();
    }

    private void CrossEnabler()
    {
        full.SetActive(false);
        alternate.SetActive(false);
        cross.SetActive(true);
        Selector();
    }

    private void Disabler()
    {
        random.gameObject.SetActive(false);
        boarding.gameObject.SetActive(false);
        efficient.gameObject.SetActive(false);
    }

    private void Selector()
    {
        random.onClick.RemoveListener(FullEnabler);
        boarding.onClick.RemoveListener(AltEnabler);
        efficient.onClick.RemoveListener(CrossEnabler);
        random.GetComponentInChildren<Text>().text = "Random";
        boarding.GetComponentInChildren<Text>().text = "Boarding Groups";
        efficient.GetComponentInChildren<Text>().text = "Efficient";
        random.onClick.AddListener(RandoSelection);
        boarding.onClick.AddListener(BoardingSelection);
        efficient.onClick.AddListener(EfficientSelection);
    }

    private void RandoSelection()
    {
        selection = 0;
        RandomizeQueue();
        Disabler();
        SetRandomStoppingDistance(); 
        StartingPositions();
    }

    private void BoardingSelection()
    {
        selection = 1;
        BoardingGroupQueue();
        Disabler();
        SetRandomStoppingDistance();
        StartingPositions();
    }

    private void EfficientSelection()
    {
        selection = 2;
        EfficientQueue();
        Disabler();
        SetRandomStoppingDistance(); 
        StartingPositions();
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

    private void SetRandomStoppingDistance() 
    {
        System.Random rand = new System.Random();
        foreach (var agent in agents)
        {
            UnityEngine.AI.NavMeshAgent nav = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();

            // (1, x) CHANGE x to adjust stopping distance randomization
            nav.stoppingDistance = rand.Next(1, 5); // distance [1, x)
        }
    }

    private void StartingPositions() // sets the starting positions for the agents based on queue
    {
        int a = 0;
        int x = 0;
        int z = 0;
        for (int i = 0; i < 6; i++) // rows
        {
            z = i * 4;
            if (i % 2 == 0) // alternating columns
            {
                for (int j = 0; j < 10; j++) // right to left columns
                {
                    x = j * 3;
                    agents[a].transform.position = new Vector3(x, 1.25f, z);
                    a++;
                }
            }
            else
            {
                for (int j = 9; j >= 0; j--) // left to right columns
                {
                    x = j * 3;
                    agents[a].transform.position = new Vector3(x, 1.25f, z);
                    a++;
                }
            }
        }
    }

    private void RandomizeQueue() // randomizes a given List
    {
        agents.AddRange(GameObject.FindGameObjectsWithTag("Agent"));

        System.Random rand = new System.Random();
        int i = agents.Count;

        while (i > 1) 
        {
            i--;
            int r = rand.Next(i + 1);
            GameObject temp = agents[r];
            agents[r] = agents[i];
            agents[i] = temp;
        }
    }

    // assumes every row is fully filled
    private void BoardingGroupQueue()
    {
        // collects all agents into List
        agents.AddRange(GameObject.FindGameObjectsWithTag("Agent"));
        // sorts agent List by row
        agents = agents.OrderByDescending(t => t.GetComponent<Agent>().row).ToList();

        int numRows = agents.Count / 6;

        for (int j = numRows; j > 0; j -= 2)
        {
            System.Random rand = new System.Random();

            int i = 0;
            if (j == 1) i = 6;
            else i = 12;
            int lowerBound = (j * 6) - i;
            while (i > 1) 
            {
                i--;
                int r = rand.Next(lowerBound, lowerBound + i);
                GameObject temp = agents[r];
                agents[r] = agents[lowerBound + i];
                agents[lowerBound + i] = temp;
            }
        }
    }

    // assumes every row is fully filled
    private void EfficientQueue()
    {
        int n = GameObject.FindGameObjectsWithTag("Agent").Length;
        int row = n / 6;
        int[] col = new int[] {1, 6, 1, 6, 2, 5, 2, 5, 3, 4, 3, 4};

        for (int i = 0; i < 12; i++) 
        {
            row = (n / 6) - ((i % 4) / 2);
            for (int j = row; j > 0; j -= 2)
            {
                // Debug.Log("Agent (" + j + ", " + col[i] + ")");
                agents.Add(GameObject.Find("Agent (" + j + ", " + col[i] + ")"));
            }
        }
    }
}