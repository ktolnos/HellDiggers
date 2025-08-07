using UnityEngine;

public class GM: MonoBehaviour
{

    public static GM I;
    
    public int money = 0;
    public bool isFree = false;
    
    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

}