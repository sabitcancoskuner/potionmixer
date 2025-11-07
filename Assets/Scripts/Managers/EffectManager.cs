using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    [SerializeField] private ResolveEffectPool resolveEffectPool;
    [SerializeField] private RocketEffectPool rocketEffectPool;
    [SerializeField] private BombEffectPool bombEffectPool;
    // [SerializeField] private DiscoEffectPool discoEffectPool;

    public void CreateEffectPools()
    {
        resolveEffectPool.CreatePool(10);
        bombEffectPool.CreatePool(10);
        // rocketEffectPool.CreatePool(10);
        // discoEffectPool.CreatePool(10);
    }

    public void PlayResolveEffect(Vector3 position, float duration)
    {
        resolveEffectPool.PlayEffect(position, duration);
    }

    public void PlayBombEffect(Vector3 position, float duration)
    {
        bombEffectPool.PlayEffect(position, duration);
    }

    public void PlayRocketEffect(Vector3 position, Vector2 direction)
    {
        rocketEffectPool.PlayEffect(position, direction);
    }
}
