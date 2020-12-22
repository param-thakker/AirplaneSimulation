using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    public Text interactionCount;


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
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="Agent"){
            SetText();
        }
    }
    void SetText()
    {
        int newScore = Convert.ToInt32(interactionCount.text) + 1;
        interactionCount.text =  newScore.ToString();
    } 


}

