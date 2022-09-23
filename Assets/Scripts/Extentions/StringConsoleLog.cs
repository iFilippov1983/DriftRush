using System;
using UnityEngine;

internal static class StringConsoleLog
{
    public static void Log(this string str, UnityEngine.Object context = null)
        => Debug.Log(str, context);

    public static void Log(this string str, Color color, UnityEngine.Object context = null)
    {
        string newString = $"<color={color}>{str}</color>";
        Debug.Log(newString, context);
    }

    public static void Warning(this string str, UnityEngine.Object context = null)
        => Debug.LogWarning(str, context);

    public static void Error(this string str, UnityEngine.Object context = null)
        => Debug.LogError(str, context);

    public static void Throw(this string str)
        => throw new Exception(str);
}

