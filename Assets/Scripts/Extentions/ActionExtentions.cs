﻿using System;

public static class ActionExtentions
{

	public static void SafeInvoke (this Action action)
	{
		if (action != null)
		{
			action.Invoke ();
		}
	}

	public static void SafeInvoke<P> (this Action<P> action, P p)
	{
		if (action != null)
		{
			action.Invoke (p);
		}
	}

	public static void SafeInvoke<P1, P2> (this Action<P1, P2> action, P1 p1, P2 p2)
	{
		if (action != null)
		{
			action.Invoke (p1, p2);
		}
	}

    public static void SafeInvoke<P1, P2, P3>(this Action<P1, P2, P3> action, P1 p1, P2 p2, P3 p3)
    {
        if (action != null)
        {
            action.Invoke(p1, p2, p3);
        }
    }
}
