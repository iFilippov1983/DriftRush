using UnityEngine;

public static class StringExtentions
{
    public static string ToStringTime (this float time)
	{
		time = Mathf.Abs (time);
		int minutes = (int)time / 60;
		int seconds = (int)time - 60 * minutes;
		int milliseconds = (int)(100 * (time - minutes * 60 - seconds));
		return string.Format ("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
	}

	public static string ToStringTime (this float time, string format)
	{
		int minutes = (int)time / 60;
		int seconds = (int)time - 60 * minutes;
		int milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
		return string.Format (format, minutes, seconds, milliseconds);
	}

	public static string SplitByUppercaseWith(this string str, string separator)
	{
        string result = string.Empty;
        string substring = string.Empty;

        foreach (char c in str)
        {
            if (char.IsUpper(c))
            {
                result = result.Equals(string.Empty)
                    ? substring
                    : result + separator + substring;

                substring = string.Empty;
                substring += c;
            }
            else
            {
                substring += c;
            }
        }

        result += separator + substring;

        return result;
    }
}
