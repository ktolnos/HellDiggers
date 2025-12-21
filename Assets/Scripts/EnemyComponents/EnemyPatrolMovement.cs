using System;
using UnityEngine;

public class EnemyPatrolMovement : EnemyMovement
{
    public EnemyMovement baseMovement;
    
    public Vector3[] patrolPoints;
    public float pointTolerance = 0.5f;
    private int currentPointIndex = 0;
    
    private Vector3 startPoint;
    public float waitTime = 0f;
    private float pointReachedTime = 0f;

    private void Start()
    {
        startPoint = transform.position;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            patrolPoints[i] += startPoint;
        }
    }

    public override void Move(Vector3 targetPos, DigCallback digCallback)
    {
        Vector3 patrolTarget = patrolPoints[currentPointIndex];

        if (Vector2.Distance(transform.position, patrolTarget) < pointTolerance)
        {
            baseMovement.Stop();
            if (pointReachedTime < 0f)
            {
                pointReachedTime = Time.time;
            }
            if (Time.time - pointReachedTime >= waitTime)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                pointReachedTime = -100f;
            }
        }
        else
        {
            baseMovement.Move(patrolTarget, digCallback);
        }
    }
    
    public override void Stop()
    {
        baseMovement.Stop();
    }
        
}