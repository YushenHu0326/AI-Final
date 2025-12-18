using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionController : MonoBehaviour
{
    AIAgent agent;

    public TMP_Text algo, time;
    // Start is called before the first frame update
    void Start()
    {
        agent = FindObjectOfType<AIAgent>();
    }

    void Update()
    {
        string mode = agent.agentMode == AIAgent.AgentMode.AStar ? "A*" : "RTT";
        algo.text = "Current Algorithm: " + mode;
        time.text = "Time Used: " + agent.timeUsed.ToString() + " ms";
    }

    public void FindPath()
    {
        agent.PathPlanning();
    }

    public void SwitchAlgo()
    {
        if (agent.agentMode == AIAgent.AgentMode.AStar)
            agent.agentMode = AIAgent.AgentMode.RTT;
        else
            agent.agentMode = AIAgent.AgentMode.AStar;
    }

    public void LoadSimpleScene()
    {
        Application.LoadLevel("SimpleScene");
    }

    public void LoadComplexScene()
    {
        Application.LoadLevel("ComplexScene");
    }

    public void LoadNoiseScene()
    {
        Application.LoadLevel("Noise");
    }
}
