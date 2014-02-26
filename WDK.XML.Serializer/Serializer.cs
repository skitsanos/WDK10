using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace WDK.XML.LinqSerializer
{

	public static class Serializer<T> where T : class, new()
	{
		private static Dictionary<Type, PropertyInfo[]> props = new Dictionary<Type, PropertyInfo[]>();

		public static string Serialize(T instance)
		{
			var t = typeof(T);
			
			var xd = new XElement(t.Name);

			foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(t))
			{
				xd.Add(descriptor.Name);
			}
			
			return xd.ToString();
		}
	}
}
