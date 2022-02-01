using System;
using System.ComponentModel;
using System.Reflection;

public static class Utils
{
	/// <summary>
	/// Enum 확장메소드, Description 읽어오기
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static string ToDescription(this Enum source)
	{
		FieldInfo fi = source.GetType().GetField(source.ToString());
		var att = (DescriptionAttribute)fi.GetCustomAttribute(typeof(DescriptionAttribute));
		if (att != null)
		{
			return att.Description;
		}
		else
		{
			return source.ToString();
		}
	}
}