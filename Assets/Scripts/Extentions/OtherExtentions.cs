using System;
using UniRx;

internal static class OtherExtentions
{
    public static void OnNext(this Subject<Unit> s) => s.OnNext(Unit.Default);

    public static T With<T>(this T o, Action<T> a) where T : ICloneable
    {
        var clone = (T)o.Clone();
        a(clone);

        return clone;
    }

}

