using RaceManager.Root;
using UnityEngine;
using Zenject;

namespace RaceManager.Infrastructure
{
    public class BaseInstaller : MonoInstaller
    {
        protected void Bind<T>(T instance) where T : class
        {
            Container.BindInterfacesAndSelfTo<T>().FromInstance(instance);
            Singleton<Resolver>.Instance.Add(instance);

            //Container.BindInterfacesAndSelfTo<T>().FromInstance(instance).OnInstantiated<T>((ctx, obj) =>
            //{
            //    Singleton<Resolver>.Instance.Add(obj);
            //});

            Container.QueueForInject(instance);
        }

        protected void Bind<T>() where T : class
        {
            Container.BindInterfacesAndSelfTo<T>().AsSingle().OnInstantiated<T>((ctx, obj) =>
            {
                Singleton<Resolver>.Instance.Add(obj);
            });
        }
    }
}
