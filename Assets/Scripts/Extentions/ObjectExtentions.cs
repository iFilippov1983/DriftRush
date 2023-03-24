using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using ArrayExtensions;
using Newtonsoft.Json;

public static class ObjectExtentions
{
    
    /// <summary>
    /// T and its class field types have to be System.Serializable (doesn't fit for UnityEngine.GameObject or UnityEngine.AnimationCurve)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCloneBinary<T>(this T obj)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }

    /// <summary>
    /// Uses Newtonsoft.Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCloneJson<T>(this T obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings 
        { 
            TypeNameHandling = TypeNameHandling.All 
        };

        string json = JsonConvert.SerializeObject(obj, settings);

        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Not recommended to use in Unity. Can cause crashes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <returns></returns>
    public static T DeepClone<T>(this T original)
    {
        return (T)DeepClone((Object)original);
    }

    /// <summary>
    /// Not recommended to use in Unity. Can cause crashes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <returns></returns>
    public static Object DeepClone(this Object originalObject)
    {
        return InternalClone(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
    }

    public static bool IsPrimitive(this Type type)
    {
        if (type == typeof(String)) return true;
        return (type.IsValueType & type.IsPrimitive);
    }

    public static void DoDestroy(this UnityEngine.GameObject go)
    {
        if (UnityEngine.Application.isPlaying)
            UnityEngine.Object.Destroy(go);
        else
            UnityEngine.Object.DestroyImmediate(go);
    }

    public static void SetActive(this UnityEngine.Component obj, bool value)
    {
        obj.gameObject.SetActive(value);
    }

    #region Private

    private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

    private static Object InternalClone(Object originalObject, IDictionary<Object, Object> visited)
    {
        if (originalObject == null) return null;
        var typeToReflect = originalObject.GetType();
        if (IsPrimitive(typeToReflect)) return originalObject;
        if (visited.ContainsKey(originalObject)) return visited[originalObject];
        if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
        var cloneObject = CloneMethod.Invoke(originalObject, null);
        if (typeToReflect.IsArray)
        {
            var arrayType = typeToReflect.GetElementType();
            if (IsPrimitive(arrayType) == false)
            {
                Array clonedArray = (Array)cloneObject;
                clonedArray.ForEach((array, indices) => array.SetValue(InternalClone(clonedArray.GetValue(indices), visited), indices));
            }

        }
        visited.Add(originalObject, cloneObject);
        CopyFields(originalObject, visited, cloneObject, typeToReflect);
        RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
        return cloneObject;
    }

    private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
    {
        if (typeToReflect.BaseType != null)
        {
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }
    }

    private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
    {
        foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
        {
            if (filter != null && filter(fieldInfo) == false) continue;
            if (IsPrimitive(fieldInfo.FieldType)) continue;
            var originalFieldValue = fieldInfo.GetValue(originalObject);
            var clonedFieldValue = InternalClone(originalFieldValue, visited);
            fieldInfo.SetValue(cloneObject, clonedFieldValue);
        }
    }

    #endregion
}

public class ReferenceEqualityComparer : EqualityComparer<Object>
{
    public override bool Equals(object x, object y)
    {
        return ReferenceEquals(x, y);
    }
    public override int GetHashCode(object obj)
    {
        if (obj == null) return 0;
        return obj.GetHashCode();
    }
}

