using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    public delegate void DigCallback(Vector3 target);
    
    public abstract void Move(Vector3 target, DigCallback digCallback);

    public abstract void Stop();
}
