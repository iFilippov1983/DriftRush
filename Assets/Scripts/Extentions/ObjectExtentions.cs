using UnityEngine;

public static class ObjectExtentions
{
    public static void SetActive(this Component obj, bool value)
    {
        obj.gameObject.SetActive(value);
    }

    public static void DoDestroy(this GameObject go)
    {
        if (Application.isPlaying)
            Object.Destroy(go);
        else
            Object.DestroyImmediate(go);
    }
}
