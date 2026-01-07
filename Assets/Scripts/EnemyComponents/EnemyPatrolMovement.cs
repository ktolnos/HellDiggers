using System;
using System.Collections;
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
    public bool disableDigging = true;
    public float startPointDelay = 0f;
    private bool initalized = false;
    private bool isInitializing = false;

    private IEnumerator Initialize()
    {
        isInitializing = true;
        if (startPointDelay > 0f)
        {
            yield return new WaitForSeconds(startPointDelay);
        }
        startPoint = transform.position;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            patrolPoints[i] += startPoint;
        }
        initalized = true;
        isInitializing = false;
    }

    public override void Move(Vector3 targetPos, DigCallback digCallback)
    {
        if (!initalized && !isInitializing)
        {
            StartCoroutine(Initialize());
        }
        if (!initalized)
        {
            return;
        }
        Vector3 patrolTarget = patrolPoints[currentPointIndex];

        if (Vector2.Distance(transform.position, patrolTarget) < pointTolerance)
        {
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
            baseMovement.Move(patrolTarget, disableDigging ? _ => { } : digCallback);
        }
    }
    
    public override void Stop()
    {
        baseMovement.Stop();
    }

    public override bool IsCurrentFacingDirectionRight()
    {
        return baseMovement.IsCurrentFacingDirectionRight();
    }
}