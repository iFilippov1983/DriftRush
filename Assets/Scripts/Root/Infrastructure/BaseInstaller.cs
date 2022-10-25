using RaceManager.Root;
using Zenject;

namespace RaceManager.Infrastructure
{
    public class BaseInstaller : MonoInstaller
    {
        protected void Bind<T>(T instance) where T : class
        {
            Container.BindInterfacesAndSelfTo<T>().FromInstance(instance).OnInstantiated<T>((ctx, obj) =>
            {
                Singleton<ResolverService>.Instance.Add(obj);
            });
            Container.QueueForInject(instance);
        }

        protected void Bind<T>() where T : class
        {
            Container.BindInterfacesAndSelfTo<T>().AsSingle().OnInstantiated<T>((ctx, obj) =>
            {
                Singleton<ResolverService>.Instance.Add(obj);
            });
        }
    }
}
