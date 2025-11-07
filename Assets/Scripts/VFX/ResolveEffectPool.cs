using UnityEngine;

public class ResolveEffectPool : ObjectPool<ResolveEffect>
{
    public void PlayEffect(Vector3 position, float duration)
    {
        ResolveEffect effect = GetPooledObject();
        effect.SetDuration(duration);
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
    }
}
