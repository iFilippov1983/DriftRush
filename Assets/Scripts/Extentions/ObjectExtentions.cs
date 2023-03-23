using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class ObjectExtentions
{
    public static void SetActive(this Component obj, bool value)
    {
        obj.gameObject.SetActive(value);
    }

    public static T DeepClone<T>(this T obj)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }

    public static void DoDestroy(this GameObject go)
    {
        if (Application.isPlaying)
            Object.Destroy(go);
        else
            Object.DestroyImmediate(go);
    }
}
