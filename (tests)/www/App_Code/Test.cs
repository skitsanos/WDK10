/*
 * WDK.API.JsonBridge.Test
 * Test Class to check that JSON Calls works
 * Copyright © 2010, Skitsanos
 * @author skitsanos (info@skitsanos.com)
 * @version 1.0
*/

using System;
using System.Collections.Generic;

public class Test
{
	public string execute()
	{
		return "WDK.API.JsonBridge.Text.execute() works!";
	}
	
	public string executeWithParam(string param)
	{
		return "Your param: " + param;
	}

	public string executeWithMultipleParam(string param, string param2)
	{
		return "Your param: " + param + " and param2: " + param2;
	}

	public string executeWithComplexParam(string param, TestParamType param2)
	{
		return "Your param: " + param + " and param2.name: " + param2.name + ", param2.status: " + param2.status.ToString();
	}

   
}

public class TestParamType
{
	public string name;
	public int status;
}