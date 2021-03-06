﻿using MLAgents;
using UnityEngine;

public class GradientObservation2DAgent : Agent {

    public GameObject Marker;
    public GameObject Target;
    public Vector2 WidthMinMax = new Vector2(-5, 5);
    public float MarkerSpeed = 1f;
    public float TotalPenaltyOverMaxSteps = -10f;
    public float SuccessReward = 2f;
    public AnimationCurve RewardCurve = AnimationCurve.Linear(0, 1, 1, 0); // Use this to calculate normalized score
    public float RewardCurveScale = 1f;

    float TotalWidth = 10f;
    float winDistance = 0.01f;
    public bool debug = false;

    public override void InitializeAgent()
    {
        TotalWidth = WidthMinMax.y - WidthMinMax.x;
    }

    // How to reinitialize when the game is reset. The Start() of an ML Agent
    public override void AgentReset()
    {
        Marker.transform.position = Marker.transform.parent.position + new Vector3(Random.Range(WidthMinMax.x, WidthMinMax.y), Marker.transform.position.y, 0);
        Target.transform.position = Marker.transform.parent.position + new Vector3(Random.Range(WidthMinMax.x, WidthMinMax.y), Marker.transform.position.y, 0);
    }

    public override void AgentAction(float[] vectorAction)
    {
        var action = Mathf.Clamp(vectorAction[0], -1f, 1f);
        if (debug) Debug.Log("action: " + action);

        var startingDistance = Vector3.Distance(Marker.transform.position, Target.transform.position) / TotalWidth;
        Marker.transform.Translate(action * Time.deltaTime, 0, 0);
        var dist = Vector3.Distance(Marker.transform.position, Target.transform.position) / TotalWidth;
        var progress = startingDistance - dist;

        if (dist < winDistance) // Did they win?
        {
            AddReward(SuccessReward);
            Done();
        }
        else
        {
            var reward = 0f;
            if (progress > 0)
            {
                reward = RewardCurve.Evaluate(progress) * RewardCurveScale; // Abs of Action means reward size is relative to distance traveled
            }
            else
            {
                reward = RewardCurve.Evaluate(Mathf.Abs(progress)) * RewardCurveScale * -1.5f;
                //reward = TotalPenaltyOverMaxSteps / agentParameters.maxStep; // Went the wrong way? It's a penalty, not a reward
            }

            if (debug) Debug.Log("reward: " + reward);
            AddReward(reward);
        }
    }
}
