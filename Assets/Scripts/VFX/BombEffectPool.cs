using UnityEngine;

public class BombEffectPool : ObjectPool<BombEffect>
{
    public void PlayEffect(Vector3 position, float duration)
    {
        BombEffect effect = GetPooledObject();
        effect.SetDuration(duration);
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
    }
}
