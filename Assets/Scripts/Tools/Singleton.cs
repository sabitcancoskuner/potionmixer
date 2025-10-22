using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Singleton instance of " + typeof(T) + " is null.");
            }

            return instance;
        }
    }

    protected void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            Init();
        }
        else
        {
            Debug.LogWarning("An instance of " + typeof(T) + " already exists. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    protected virtual void Init()
    {
        // Optional initialization code for derived classes
    }

    protected void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    
}
