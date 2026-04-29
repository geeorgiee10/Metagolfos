using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Helpers.Linq
{
	public static class LinqUtil
	{
		public static int FirstIndex<T>(this IEnumerable<T> enumerable, T element)
		{
			try
			{
				return enumerable.Select((el, i) => (el, i)).First(e => e.el.Equals(element)).i;
			}
			catch
			{
				return -1;
			}
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, System.Action<T> action)
		{
			foreach (T t in enumerable) action.Invoke(t);
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, System.Action<T, int> action)
		{
			int i = 0;
			foreach (T t in enumerable) action.Invoke(t, i++);
		}
	}
}