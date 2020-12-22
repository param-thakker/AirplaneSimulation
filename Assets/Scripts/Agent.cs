using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
    public int row;
    public int col;
    public int cRow; // current row
    public bool isSeated;
    public Transform target;
    public string status;
    public Material matOrange;
    public Material whiteSeat;
    public Text statusUpdater;


    void Start()
    {
        cRow = 0;
        target = new GameObject().transform;
        status = "Agent is heading to airplane to get seat (" + row + ", " + col + ")";
    }

    void OnMouseOver()
    {
        GameObject.Find("Seat " + "(" + row + ", " + col + ")").GetComponent<Renderer>().material = matOrange;
        statusUpdater.text = status;
    }

    void OnMouseExit()
    {
        GameObject.Find("Seat " + "(" + row + ", " + col + ")").GetComponent<Renderer>().material = whiteSeat;
    }
}

