using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;


namespace WDK.XML.LinqSerializer
{

	public static class PropertyCaller<T>
	{
		public delegate void GenSetter(T target, System.Object value);
		public delegate System.Object GenGetter(T target);

		private static Dictionary<Type, Dictionary<Type, Dictionary<string, GenGetter>>> dGets = new Dictionary<Type, Dictionary<Type, Dictionary<string, GenGetter>>>();
		private static Dictionary<Type, Dictionary<Type, Dictionary<string, GenSetter>>> dSets = new Dictionary<Type, Dictionary<Type, Dictionary<string, GenSetter>>>();

		public static GenGetter CreateGetMethod(PropertyInfo pi)
		{
			var classType = typeof(T);
			var propType = pi.PropertyType;
			var propName = pi.Name;

			Dictionary<Type, Dictionary<string, GenGetter>> i1 = null;
			if(dGets.TryGetValue(classType, out i1))
			{
				Dictionary<string, GenGetter> i2 = null;
				if(i1.TryGetValue(propType, out i2))
				{
					GenGetter i3 = null;
					if(i2.TryGetValue(propName, out i3))
					{
						return i3;
					}
				}
			}

			//If there is no getter, return nothing
			var getMethod = pi.GetGetMethod();
			if(getMethod == null)
			{
				return null;
			}

			//Create the dynamic method to wrap the internal get method
			var arguments = new Type[1] { classType };

			var getter = new DynamicMethod(String.Concat("_Get", pi.Name, "_"), typeof(object), new Type[] { typeof(T) }, classType);
			var generator = getter.GetILGenerator();
			generator.DeclareLocal(propType);
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, classType);
			generator.EmitCall(OpCodes.Callvirt, getMethod, null);
			if(!propType.IsClass)
				generator.Emit(OpCodes.Box, propType);
			generator.Emit(OpCodes.Ret);

			//Create the delegate and return it
			GenGetter genGetter = (GenGetter)getter.CreateDelegate(typeof(GenGetter));

			Dictionary<Type, Dictionary<string, GenGetter>> tempDict = null;
			Dictionary<string, GenGetter> tempPropDict = null;
			if(!dGets.ContainsKey(classType))
			{
				tempPropDict = new Dictionary<string, GenGetter>();
				tempPropDict.Add(propName, genGetter);
				tempDict = new Dictionary<Type, Dictionary<string, GenGetter>>();
				tempDict.Add(propType, tempPropDict);
				dGets.Add(classType, tempDict);
			}
			else
			{
				if(!dGets[classType].ContainsKey(propType))
				{
					tempPropDict = new Dictionary<string, GenGetter>();
					tempPropDict.Add(propName, genGetter);
					dGets[classType].Add(propType, tempPropDict);
				}
				else
				{
					if(!dGets[classType][propType].ContainsKey(propName))
					{
						dGets[classType][propType].Add(propName, genGetter);
					}
				}
			}
			return genGetter;
		}

		public static GenSetter CreateSetMethod(PropertyInfo pi)
		{
			Type classType = typeof(T);
			Type propType = pi.PropertyType;
			string propName = pi.Name;

			Dictionary<Type, Dictionary<string, GenSetter>> i1 = null;
			if(dSets.TryGetValue(classType, out i1))
			{
				Dictionary<string, GenSetter> i2 = null;
				if(i1.TryGetValue(propType, out i2))
				{
					GenSetter i3 = null;
					if(i2.TryGetValue(propName, out i3))
					{
						return i3;
					}
				}
			}

			//If there is no setter, return nothing
			MethodInfo setMethod = pi.GetSetMethod();
			if(setMethod == null)
			{
				return null;
			}

			//Create dynamic method
			Type[] arguments = new Type[2] { classType, typeof(object) };

			DynamicMethod setter = new DynamicMethod(String.Concat("_Set", pi.Name, "_"), typeof(void), arguments, classType);
			ILGenerator generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, classType);
			generator.Emit(OpCodes.Ldarg_1);

			if(propType.IsClass)
				generator.Emit(OpCodes.Castclass, propType);
			else
				generator.Emit(OpCodes.Unbox_Any, propType);

			generator.EmitCall(OpCodes.Callvirt, setMethod, null);
			generator.Emit(OpCodes.Ret);

			//Create the delegate and return it
			GenSetter genSetter = (GenSetter)setter.CreateDelegate(typeof(GenSetter));

			Dictionary<Type, Dictionary<string, GenSetter>> tempDict = null;
			Dictionary<string, GenSetter> tempPropDict = null;
			if(!dSets.ContainsKey(classType))
			{
				tempPropDict = new Dictionary<string, GenSetter>();
				tempPropDict.Add(propName, genSetter);
				tempDict = new Dictionary<Type, Dictionary<string, GenSetter>>();
				tempDict.Add(propType, tempPropDict);
				dSets.Add(classType, tempDict);
			}
			else
			{
				if(!dSets[classType].ContainsKey(propType))
				{
					tempPropDict = new Dictionary<string, GenSetter>();
					tempPropDict.Add(propName, genSetter);
					dSets[classType].Add(propType, tempPropDict);
				}
				else
				{
					if(!dSets[classType][propType].ContainsKey(propName))
					{
						dSets[classType][propType].Add(propName, genSetter);
					}
				}
			}

			return genSetter;
		}
	}
}
