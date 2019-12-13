// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.AddXMsEnumToSwaggerJson
{
    using System;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This program will convert a swagger.json file to one that supports "x-ms-enum" for AutoRest
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main program code; Checks input parameters.
        /// </summary>
        /// <param name="args">input filename and output filename</param>
        private static void Main(string[] args)
        {
            // usage
            if (args.Count() != 2)
            {
                Console.Error.WriteLine("usage: ");
                Console.Error.WriteLine("\t" + AppDomain.CurrentDomain.FriendlyName + " <input file> <output file>");
                Environment.Exit(-1);
            }

            // get the input file
            string inputFilename = args[0];
            if (!File.Exists(inputFilename))
            {
                Console.Error.WriteLine("cannot find " + inputFilename);
                Environment.Exit(-1);
            }

            // get the output file
            string outputFilename = args[1];

            // execute
            AddXMsEnumToSwaggerJson(inputFilename, outputFilename);
        }

        /// <summary>
        /// Adds x-ms-enum properties to enum declarations
        /// </summary>
        /// <param name="inputfileName">input swagger json file</param>
        /// <param name="outputFilename">output file</param>
        private static void AddXMsEnumToSwaggerJson(string inputfileName, string outputFilename)
        {
            // read the JSON file
            string jsonText = File.ReadAllText(inputfileName);

            // parse it
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonText);
            foreach (var objSection in jsonObj)
            {
                // handle class definitions; these define request/response data structures which may contain enums that need to be processed
                if (objSection.Name == "definitions")
                {
                    foreach (var objClass in objSection.Value)
                    {
                        foreach (var objDescription in objClass.Value)
                        {
                            if (objDescription.Name == "properties")
                            {
                                foreach (var objField in objDescription)
                                {
                                    foreach (var objFieldDescription in objField)
                                    {
                                        string fieldName = objFieldDescription.Name;
                                        bool isEnum = false;
                                        bool hasXMSEnumProperty = false;
                                        foreach (var objFieldProperties in objFieldDescription)
                                        {
                                            foreach (var objFieldProperty in objFieldProperties)
                                            {
                                                if (objFieldProperty.Name == "enum")
                                                {
                                                    isEnum = true;
                                                }
                                                else if (objFieldProperty.Name == "x-ms-enum")
                                                {
                                                    hasXMSEnumProperty = true;
                                                }
                                            }

                                            if (isEnum && !hasXMSEnumProperty)
                                            {
                                                // insert new x-ms-enum property
                                                var msEnum1 = new JProperty("name", fieldName);
                                                var msEnum2 = new JObject();
                                                msEnum2.Add(msEnum1);
                                                var msEnum3 = new JProperty("x-ms-enum", msEnum2);
                                                objFieldProperties.Add(msEnum3);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // handle API definitions; there might be enums lurking in the route path that need to be processed
                else if (objSection.Name == "paths")
                {
                    // this is every API call
                    foreach (var objCall in objSection.Value)
                    {
                        // this is every HTTP operation on the API call
                        foreach (var objOperation in objCall.Value)
                        {
                            // this is the tag or summary or description, etc.
                            foreach (var objPropertyBag in objOperation)
                            {
                                foreach (var objProperty in objPropertyBag)
                                {
                                    if (objProperty.Name == "parameters")
                                    {
                                        foreach (var objParameter in objProperty)
                                        {
                                            // unnecessary nesting in swagger JSON
                                            foreach (var objParameter2 in objParameter)
                                            {
                                                string fieldName = string.Empty;
                                                bool isEnum = false;
                                                bool hasXMSEnumProperty = false;

                                                // read all the appropriate properties of this parameter
                                                foreach (var objParameterProperty in objParameter2)
                                                {
                                                    if (objParameterProperty.Name == "name")
                                                    {
                                                        fieldName = objParameterProperty.Value;
                                                    }
                                                    else if (objParameterProperty.Name == "enum")
                                                    {
                                                        isEnum = true;
                                                    }
                                                    else if (objParameterProperty.Name == "x-ms-enum")
                                                    {
                                                        hasXMSEnumProperty = true;
                                                    }
                                                }

                                                // do we need to insert x-ms-enum?
                                                if (isEnum && !hasXMSEnumProperty)
                                                {
                                                    // insert new x-ms-enum property
                                                    var msEnum1 = new JProperty("name", fieldName);
                                                    var msEnum2 = new JObject();
                                                    msEnum2.Add(msEnum1);
                                                    var msEnum3 = new JProperty("x-ms-enum", msEnum2);
                                                    objParameter2.Add(msEnum3);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // output
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(outputFilename, output);
        }
    }
}
