/*
** JSON Bridge Http Handler for WDK Content Router
**
** @author skitsanos (info@skitsanos.com)
** @version 1.6
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace WDK.API.JsonBridge
{
	public class HttpHandler : IHttpHandler, IRequiresSessionState
	{
		#region " Variables and Properties "

		public string classpath;
		public string method;

		#endregion

		#region " ProcessRequest "

		public void ProcessRequest(HttpContext context)
		{
			var Response = context.Response;
			var Request = context.Request;

			var execTimeNow = new TimeSpan(DateTime.Now.Ticks);

			if (bool.Parse(getWebConfigValue("jsonbridge.AuthEnabled").ToString()) && String.IsNullOrEmpty(Request.Headers["Authorization"]))
			{
				Response.Write(JsonConvert.SerializeObject(new InvokationErrorType { message = "Authorization failed. No Authorization HTTP headers sent", execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds) }));
				return;
			}

			if (bool.Parse(getWebConfigValue("jsonbridge.AuthEnabled").ToString()) && !String.IsNullOrEmpty(Request.Headers["Authorization"]))
			{
				var authHeader = Request.Headers["Authorization"];
				var authMethod = Request.Headers["Authorization"].Split(' ')[0].ToLower();

				var assembly = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "bin\\JBAP." + authMethod + ".dll");
				var type = assembly.GetType("JBAP." + authHeader.Split(' ')[0]);
				if (type == null)
				{
					Response.Write(JsonConvert.SerializeObject(new InvokationErrorType
					{
							message = "Authorization failed. Wrong Authorization handler name",
							execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds)
						}));
					return;
				}
				var obj = Activator.CreateInstance(type);
				var jbapClassName = authHeader.Split(' ')[0];
				var validateArgs = authHeader.Substring(jbapClassName.Length + 1);
				var validateResult = (bool)type.InvokeMember("validate", BindingFlags.Default | BindingFlags.InvokeMethod, null, obj, new object[] { validateArgs });
				if (validateResult) return;
				Response.Write(JsonConvert.SerializeObject(new InvokationErrorType
				{
					message = "Authorization failed",
					execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds)
				}));
				return;
			}
			if (context.Application["__assemblies"] == null)
			{
				context.Application["__assemblies"] = browseAssemblies();
			}

			var loaddedAssemblies = (Dictionary<string, Assembly>)context.Application["__assemblies"];

			switch (classpath.ToLower())
			{
				case "jquery.jsonbridge.js":
					Response.ContentType = ContentTypes.JAVASCRIPT;
					Response.Write(getEmbeddedContent("jquery.jsonbridge.js"));
					break;

				case "_browse":
					var listOfAssemblies = loaddedAssemblies.Select(assembly => assembly.Value.FullName.Split(',')[0]).ToList();

					Response.ContentType = ContentTypes.JSON;
					Response.Write(JsonConvert.SerializeObject(new InvokationResultType()
					{
						result = listOfAssemblies,
						execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds)
					}));
					break;

				case "_browseclasses":
					var listOfClasses = new List<string>();

					try
					{
						foreach (var assembly in loaddedAssemblies)
						{
							foreach (var type in assembly.Value.GetTypes())
							{
								listOfClasses.Add(type.FullName);
							}
						}
					}
					catch (Exception)
					{
						//Response.Write(ex.ToString() + "<hr/>");
					}

					Response.ContentType = ContentTypes.JSON;
					Response.Write(JsonConvert.SerializeObject(new InvokationResultType
					{
						result = listOfClasses,
						execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds)
					}));
					break;

				default:
					if (!String.IsNullOrEmpty(method))
					{
						if (isValidClass(classpath))
						{
							if (method.ToLower() == "_methods")
							{
								// Browse for all methods within a class
								Response.Write(JsonConvert.SerializeObject(browseClassMethods(classpath)));
							}
							else
							{
								// Class path is valid, check if method is valid

								var possibleMethods = new List<MethodInfo>();

								possibleMethods = getOverloadsByName(getClassAssemblyQualifiedName(classpath), method);

								if (possibleMethods.Count == 0)
								{
									Response.Write(JsonConvert.SerializeObject(new InvokationErrorType()
																				{
																					execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds),
																					message = classpath + " does not contain any method " + method
																				}));
									return;
								}

								if (Request.RequestType == "POST")
								{
									// Check if user actually sent any params by analizyng inout stream
									if (Request.InputStream.Length == 0)
									{
										Response.Write("Request body missing");
									}
									else
									{
										var stream = new StreamReader(Request.InputStream);
										var requestBody = stream.ReadToEnd();

										var jsonSettings = new JsonSerializerSettings
											{
												DateTimeZoneHandling = DateTimeZoneHandling.Utc,
												DateFormatHandling = DateFormatHandling.IsoDateFormat,
												DateFormatString = "{0:s}",
												DateParseHandling = DateParseHandling.DateTime
											};

										var invokeParams = JsonConvert.DeserializeObject<object[]>(requestBody, jsonSettings);

										for (var index = possibleMethods.Count - 1; index >= 0; index--)
										{
											if (possibleMethods[index].GetParameters().Length != invokeParams.Length)
											{
												possibleMethods.RemoveAt(index);
											}
										}
										switch (possibleMethods.Count)
										{
											case 0:
												Response.Write(JsonConvert.SerializeObject(new InvokationErrorType()
													{
														execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds),
														message = "There is no overload of the method with the specified number of params"
													}));
												return;
											case 1:
												{
													var methodInfo = possibleMethods[0];

													var correctParams = new List<object>();

													for (var index = 0; index < invokeParams.Length; index++)
													{
														if (invokeParams[index].GetType().FullName != "Newtonsoft.Json.Linq.JObject")
														{
															correctParams.Add(invokeParams[index]);
														}
														else
														{
															var paramType = methodInfo.GetParameters()[index].ParameterType;

															var serializer = new JsonSerializer();

															var o = serializer.Deserialize(new JTokenReader((JObject)invokeParams[index]), paramType);

															correctParams.Add(o);
														}
													}

													var invokeResult = invokeAssemblyMethod(classpath, methodInfo, correctParams.ToArray());

													Response.ContentType = ContentTypes.JSON;

													//Response.Write(JsonConvert.SerializeObject(invokeResult, new JavaScriptDateTimeConverter()));
													var converters = new JsonConverterCollection
							                            {
							                                new StringEnumConverter(),
							                                new IsoDateTimeConverter()
							                            };
													Response.Write(JsonConvert.SerializeObject(invokeResult, converters.ToArray()));

												}
												break;
											default:
												{
													//Response.Write("Before anything else");
													//return;
													//trying to build the possibly correct argument list for each overload
													var correctFinalParams = new List<object>();
													var debug = "";
													for (var index = possibleMethods.Count - 1; index >= 0; index--)
													{
														var correctParams = new List<object>();
														var validParams = true;
														//try to deserialize each possible type. if it fucks up, it's clearly not the overload we're looking for
														//Response.Write("Before the loop");
														//return;

														for (var index2 = 0; index2 < invokeParams.Length; index2++)
														{
															debug += "Front of the loop; ";
															//return;
															if (invokeParams[index2].GetType().FullName != "Newtonsoft.Json.Linq.JObject")
															{
																correctParams.Add(invokeParams[index2]);
																debug += "Added normal type to param arr; ";
															}
															else
															{
																var paramType = possibleMethods[index].GetParameters()[index2].ParameterType;
																debug += "Proceeding to deserialize from " + paramType.ToString() + "; ";
																var serializer = new JsonSerializer();
																// Response.Write(debug);
																// return;
																var o = serializer.Deserialize(new JTokenReader((JObject)invokeParams[index2]), paramType);
																debug += "Done serializing; ";
																if (o == null)
																{
																	//couldn't deserialize to this type, so it's not the good overload
																	validParams = false;
																	debug += "Failed serializing; ";
																	break;
																}
																correctParams.Add(o);


															}
															debug += "End of the loop; ";
														}
														//Response.Write(debug);
														//return;
														if (!validParams)
														{
															possibleMethods.RemoveAt(index);
															debug += "Not a valid overload; ";
															continue;
														}

														var curMethodParamInfoArr = possibleMethods[index].GetParameters();
														var curMethodTypeArr = curMethodParamInfoArr.Select(param => param.ParameterType).ToList();

														//since all complex types are ok now, we've gotta check whether all types are matching. If they do, it's a valid overload
														for (var index2 = 0; index2 < curMethodTypeArr.Count; index2++)
														{
															debug += "Checking " + correctParams[index2].GetType() + " and " + curMethodTypeArr[index2] + ": ";
															if (correctParams[index2].GetType() == curMethodTypeArr[index2])
															{
																debug += "equal; ";
															}
															else if ((correctParams[index2].GetType() == Type.GetType("System.Int64")) && isValidInt32(Int64.Parse(correctParams[index2].ToString())) && (curMethodTypeArr[index2] == Type.GetType("System.Int32")))
															{
																debug += "Could cast from Int64 to Int32;";
															}
															else
															{

																debug += "different; ";
																validParams = false;
																break;
															}
														}

														if (!validParams)
														{
															possibleMethods.RemoveAt(index);
															debug += "Not a valid overload; ";
															continue;
														}
														correctFinalParams = correctParams;

													}
													//Response.Write(debug);
													//return;
													if (possibleMethods.Count == 0)
													{
														Response.Write("No valid overload afterall.");
													}
													else if (possibleMethods.Count >= 2)
													{
														Response.Write(JsonConvert.SerializeObject(new InvokationErrorType()
															{
																execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds),
																message = "Ambiguous call: " + possibleMethods.Count + " valid overloads"
															}));
													}
													else
													{
														var invokeResult = invokeAssemblyMethod(classpath, possibleMethods[0], correctFinalParams.ToArray());

														Response.ContentType = ContentTypes.JSON;

														var converters = new JsonConverterCollection
							                                {
							                                    new StringEnumConverter(),
							                                    new IsoDateTimeConverter()
							                                };
														Response.Write(JsonConvert.SerializeObject(invokeResult, converters.ToArray()));
													}
												}
												break;
										}
									}
								}
								else
								{
									var methodInfo = possibleMethods[0];
									if (methodInfo.GetParameters().Length > 0)
									{
										Response.Write(JsonConvert.SerializeObject(new InvokationErrorType
										{
												execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds),
												message = "This method requires " + methodInfo.GetParameters().Length + " parameter(s)"
											}));
									}
									else
									{
										var invokeResult = invokeAssemblyMethod(classpath, methodInfo, null);

										var converters = new JsonConverterCollection
							                {
							                    new StringEnumConverter(),
							                    new JavaScriptDateTimeConverter()
							                };

										Response.Write(JsonConvert.SerializeObject(invokeResult, converters.ToArray()));
									}
								}
							}
						}
						else
						{
							// User specified wrong class path, should be valid type name
							Response.Write(JsonConvert.SerializeObject(new InvokationErrorType
							{
								execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds),
								message = "Invalid class path: " + classpath
							}));
						}
					}
					else
					{
						// Class specified but method is missing
						//Response.Write("--not implemented, " + classpath);
						Response.Write(JsonConvert.SerializeObject(browseClassMethods(classpath)));
					}
					break;
			}
		}

		#endregion

		#region " IsReusable "

		public virtual bool IsReusable
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region " BrowseAssemblies "

		private static Dictionary<string, Assembly> browseAssemblies()
		{
			var ret = new Dictionary<string, Assembly>();

			var dirinfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\bin");

			foreach (var file in dirinfo.GetFiles("*.dll"))
			{
				try
				{
					var assembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension((file.Name)));

					var assemblyName = assembly.FullName.Split(',')[0];

					ret.Add(assemblyName, assembly);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}

			//foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
			//{
			//    try
			//    {
			//        var assemblyName = assembly.FullName.Split(',')[0];
			//        //AppDomain.CurrentDomain.Load(assemblyName);
			//        ret.Add(assemblyName, assembly);
			//    }
			//    catch(Exception ex)
			//    {
			//        System.Diagnostics.Debug.WriteLine(ex.ToString());
			//    }
			//}

			return ret;
		}

		#endregion

		#region " IsValidClass "

		private static bool isValidClass(string name)
		{
			var result = false;

			var loaddedAssemblies = browseAssemblies();

			try
			{
				foreach (var assembly in loaddedAssemblies)
				{
					foreach (var type in assembly.Value.GetTypes())
					{
						if (type.FullName.ToLower() == name.ToLower())
						{
							result = true;
						}
					}
				}
			}
			catch (Exception)
			{
			}

			return result;
		}

		#endregion

		#region " BrowseClassMethods "

		private static List<string> browseClassMethods(string classpath)
		{
			var result = new List<string>();

			var loaddedAssemblies = browseAssemblies();

			try
			{
				foreach (var assembly in loaddedAssemblies)
				{
					foreach (var type in assembly.Value.GetTypes())
					{
						if (type.FullName.ToLower() == classpath.ToLower())
						{
							foreach (var m in type.GetMethods())
							{
								result.Add(m.Name);
							}
						}
					}

				}
			}
			catch (Exception)
			{
			}

			return result;
		}
		#endregion

		#region " AssemblyContainsMethod "

		private static bool assemblyContainsMethod(string classpath, string method)
		{
			var loaddedAssemblies = browseAssemblies();

			try
			{
				foreach (var assembly in loaddedAssemblies)
				{
					foreach (var type in assembly.Value.GetTypes())
					{
						if (String.Equals(type.FullName, classpath, StringComparison.CurrentCultureIgnoreCase))
						{
							var methodInfo = type.GetMethod(method);

							if (methodInfo != null)
							{
								return true;
							}
						}
					}

				}
			}
			catch (Exception)
			{
			}

			return false;
		}

		#endregion

		#region " GetOverloadsByName "

		List<MethodInfo> getOverloadsByName(string classpath, string methodName)
		{
			var result = new List<MethodInfo>();

			foreach (var method in Type.GetType(classpath).GetMethods())
			{
				if (method.Name == methodName)
				{
					result.Add(method);
				}
			}

			return result;
		}

		#endregion

		#region " GetMethodInfo "

		/// <summary>
		/// Gets Method Information by classpath and method name
		/// </summary>
		/// <param name="classpath"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		private static MethodInfo getMethodInfo(string classpath, string method)
		{
			var loaddedAssemblies = browseAssemblies();

			try
			{
				foreach (var assembly in loaddedAssemblies)
				{
					foreach (var type in assembly.Value.GetTypes())
					{
						if (type.FullName.ToLower() == classpath.ToLower())
						{
							var methodInfo = type.GetMethod(method);

							if (methodInfo != null)
							{
								return methodInfo;
							}
						}
					}

				}
			}
			catch (Exception)
			{
			}

			return null;
		}

		#endregion

		#region " InvokeAssemblyMethod "

		/// <summary>
		/// Invokes Method on class with arguments or without
		/// </summary>
		/// <param name="classpath"></param>
		/// <param name="method"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static object invokeAssemblyMethod(string classpath, MethodInfo methodInfo, object[] args)
		{
			var loaddedAssemblies = browseAssemblies();
			var execTimeNow = new TimeSpan(DateTime.Now.Ticks);

			try
			{
				var result = new InvokationResultType();

				foreach (var assembly in loaddedAssemblies)
				{
					var assemblyName = assembly.Value.FullName.Split(',')[0];

					foreach (var type in assembly.Value.GetTypes())
					{
						if (type.FullName.ToLower() != classpath.ToLower()) continue;

						if (methodInfo == null)
						{
							return "Did not find the method.";
						}
						//return "method: " + methodInfo.ToString();
						//if method is static it could fuck
						//if(methodInfo.IsStatic)
						//{
						//    result= methodInfo.Invoke(null, args);
						//    return result;
						//}
						//else
						//{
						//    var mgr = Activator.CreateInstance(Type.GetType(classpath + ", " + assemblyName, true, true));
						//    result = methodInfo.Invoke(mgr, args);
						//    return result;
						//}

						//make sure all param works fine
						if ((args != null) && (args.Length > 0))
						{
							for (var index = 0; index < args.Length; index++)
							{
								var paramType = methodInfo.GetParameters()[index];
								switch (paramType.ParameterType.FullName)
								{
									case "System.DateTime":
										args[index] = DateTime.Parse(args[index].ToString());
										break;

									case "System.Int32":
										args[index] = Int32.Parse(args[index].ToString());
										break;

									case "System.Int64":
										args[index] = Int64.Parse(args[index].ToString());
										break;

									case "System.Decimal":
										args[index] = Decimal.Parse(args[index].ToString());
										break;

									default:
										if (paramType.ParameterType.Namespace == "System.Collections.Generic" && paramType.ParameterType.FullName.Contains("System.Collections.Generic.List"))
										{
											var serializer = new JsonSerializer();
											var o = serializer.Deserialize(new JTokenReader((JToken)args[index]), paramType.ParameterType);

											args[index] = o;
										}
										break;
								}
							}
						}

						if (methodInfo.IsStatic)
						{
							try
							{
								result.result = methodInfo.Invoke(null, args);
							}
							catch (Exception e)
							{
								result.result = e.Message;
							}
						}
						else
						{
							var mgr = Activator.CreateInstance(Type.GetType(classpath + ", " + assemblyName, true, true));
							try
							{
								result.result = methodInfo.Invoke(mgr, args);
							}
							catch (TargetInvocationException targetInvocationException)
							{

								var errresult = new InvokationErrorType
									{
										message = targetInvocationException.InnerException.Message,
										stackTrace = targetInvocationException.InnerException.StackTrace,
										execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds)
									};
								return errresult;
							}
						}

						result.execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds);

						return result;
					}

				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
				var result = new InvokationErrorType
				{
					message = ex.Message,
					stackTrace = ex.StackTrace,
					execTime = (new TimeSpan(DateTime.Now.Ticks - execTimeNow.Ticks).TotalSeconds)
				};
				return result;
			}

			return null;
		}

		#endregion

		#region " Helper Functions "

		private static String getClassAssemblyQualifiedName(string classpath)
		{
			var loaddedAssemblies = browseAssemblies();

			try
			{
				foreach (var assembly in loaddedAssemblies)
				{
					foreach (var type in assembly.Value.GetTypes())
					{
						if (type.FullName.ToLower() == classpath.ToLower())
						{
							return type.AssemblyQualifiedName;
						}
					}
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
		{
			return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
		}

		private static bool isValidInt32(Int64 x)
		{
			return x >= Int32.MinValue && x <= Int32.MaxValue;
		}

		#endregion

		#region " DynamicCast "

		public static object DynamicCast(object obj, Type targetType)
		{
			// First, it might be just a simple situation
			if (targetType.IsAssignableFrom(obj.GetType()))
			{
				return obj;
			}

			// If not, we need to find a cast operator. The operator
			// may be explicit or implicit and may be included in
			// either of the two types...
			const BindingFlags pubStatBinding = BindingFlags.Public | BindingFlags.Static;

			var originType = obj.GetType();

			String[] names = { "op_Implicit", "op_Explicit" };

			var castMethod = targetType.GetMethods(pubStatBinding).Union(originType.GetMethods(pubStatBinding)).FirstOrDefault(itm => itm.ReturnType.Equals(targetType) && itm.GetParameters().Length == 1 && itm.GetParameters()[0].ParameterType.IsAssignableFrom(originType) && names.Contains(itm.Name));

			if (null != castMethod)
			{
				return castMethod.Invoke(null, new object[] { obj });
			}
			else
			{
				throw new InvalidOperationException(String.Format("No matching cast operator found from {0} to {1}.", originType.Name, targetType.Name));
			}
		}

		#endregion

		#region " GetEmbeddedContent "

		private static string getEmbeddedContent(string resource)
		{
			var assembly = Assembly.GetExecutingAssembly();

			const string ns = "WDK.API.JsonBridge";

			try
			{
				var resStream = assembly.GetManifestResourceStream(ns + "." + resource);

				if (resStream != null)
				{
					var result = "";

					var sr = new StreamReader(resStream);

					result = sr.ReadToEnd();

					sr.Close();

					return result;
				}
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}

			return "";
		}

		#endregion

		#region " GetWebConfigValue "

		private static object getWebConfigValue(string Key)
		{
			var conf = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

			return conf.AppSettings.Settings[Key] == null ? null : conf.AppSettings.Settings[Key].Value;
		}

		#endregion
	}

	public class InvokationResultType
	{
		public string type = "result";
		public object result;
		public double execTime;
	}

	public class InvokationErrorType
	{
		public string type = "error";
		public string message;
		public string stackTrace;
		public double execTime;
	}
}
