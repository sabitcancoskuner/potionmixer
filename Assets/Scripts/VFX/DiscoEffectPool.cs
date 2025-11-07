using UnityEngine;

public class DiscoEffectPool : ObjectPool<DiscoEffect>
{
    public void PlayEffect(Vector3 position, float duration, Vector2Int orientation)
    {
        DiscoEffect effect = GetPooledObject();
        effect.SetDuration(duration);
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
    }
}
