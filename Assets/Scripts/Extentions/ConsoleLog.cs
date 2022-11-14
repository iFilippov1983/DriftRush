using System;
using UnityEngine;
using Zenject;

public static class ConsoleLog
{
    public static void Log(this string str, UnityEngine.Object context = null)
        => Debug.Log(str, context);

    public static void Log(this string str, string color, UnityEngine.Object context = null)
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

    public static string Colored(this string str, string color)
    {
        return $"<color={color}>{str}</color>";
    }

    public class Color
    { 
        public const string Red = "red";
        public const string Green = "green";
        public const string Blue = "blue";
        public const string Yellow = "yellow";
    }
}

