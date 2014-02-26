/*
 * WDK.API.JsonBridge.Test
 * Test Class to check that JSON Calls works
 * Copyright © 2010, Skitsanos
 * @author skitsanos (info@skitsanos.com)
 * @version 1.0
*/

using System;
using System.Collections.Generic;

namespace WDK.API.JsonBridge
{
    public class Test
    {
        public string execute()
        {
            return "WDK.API.JsonBridge.Text.execute() works!";
        }

        public List<string> executeList()
        {
            var ret = new List<string> { "one", "two", "three", "seven" };
            return ret;
        }

        public string executeWithParam(string param)
        {
            return "Your param: " + param;
        }

        public string executeWithParam(int param)
        {
            return "Your param: " + param;
        }

        public string executeWithParam(string param, string param2)
        {
            return param + param2;
        }

        public int executeWithParam(int param, int param2)
        {
            return param + param2;
        }

        public string executeWithMultipleParam(string param, string param2)
        {
            return "Your param: " + param + " and param2: " + param2;
        }

        public string executeWithComplexParam(string param, TestParamType param2)
        {
            return "Your param: " + param + " and param2.name: " + param2.name + ", param2.status: " + param2.status.ToString();
        }

        public DateTime executeDate(DateTime date)
        {
            return date.AddDays(22);
        }

        public int executeNumber(int data)
        {
            return data + 2000;
        }

        public string executeComplexInput(List<TestComplexParamType> data)
        {
            return "Received " + data.Count + " elements";
        }

        public List<TestComplexParamType> getDatasource(string criteria)
        {
            var result = new List<TestComplexParamType>();
            result.Add(new TestComplexParamType());
            result[0].list.Add(new TestParamType());
                            
            result[0].list[0].name = "Salata";

            return result;
        }
    }

    public class TestParamType
    {
        public string name;
        public int status;
        public EnumsToTest format = EnumsToTest.SOME_SPECIAL_ENUM;
        public DateTime createdOn = DateTime.Now;
    }

    public class TestComplexParamType
    {
        public List<TestParamType> list = new List<TestParamType>();
    }

    public enum EnumsToTest
    {
        ONE_ENUM,
        TWO_ENUM,
        THREE_ENUM,
        SOME_SPECIAL_ENUM = 1234567890
    }
}