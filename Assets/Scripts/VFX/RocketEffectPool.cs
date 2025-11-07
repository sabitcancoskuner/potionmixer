using System.Collections;
using UnityEngine;

public class RocketEffectPool : ObjectPool<RocketEffect>
{
    public void PlayEffect(Vector3 position, Vector2 direction)
    {
        RocketEffect effect = GetPooledObject();
        effect.transform.position = position;
        effect.SetDirection(direction);
        effect.SetOrientation(direction);
        effect.gameObject.SetActive(true);
    }
}
