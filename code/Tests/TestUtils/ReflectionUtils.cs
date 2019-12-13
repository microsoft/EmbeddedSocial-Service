// <copyright file="ReflectionUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Utility methods for Reflection to find Test classes and methods
    /// </summary>
    public class ReflectionUtils
    {
        /// <summary>
        /// Creates a class instance from a given class name for a specific assembly and specic namespace.
        /// Takes in a list of parameters and calls the first constructor found that matches the number and types of the parameters.
        /// </summary>
        /// <param name="assembly">assembly where the class is found</param>
        /// <param name="nameSpace">namespace where the class if found</param>
        /// <param name="className">name of the class</param>
        /// <param name="parameters">list of parameters for the class constructor</param>
        /// <returns>an instance of the class if found; null otherwise</returns>
        public static object CreateClassInstance(Assembly assembly, string nameSpace, string className, params object[] parameters)
        {
            var classType = assembly.GetTypes().Where(t => t.IsClass && t.Namespace == nameSpace && t.Name == className).First();

            // if no class found with this name, return null
            if (classType == null)
            {
                return null;
            }

            // find the right constructor for the passed in parameters
            foreach (var constructor in classType.GetConstructors())
            {
                var constructorPars = constructor.GetParameters();
                if (constructorPars.Count() == parameters.Count())
                {
                    bool doesConstructorSignatureMatchParameters = true;
                    for (int i = 0; i < constructorPars.Count(); i += 1)
                    {
                        Type constructorParamType = constructorPars[i].ParameterType;
                        Type paramType = parameters[i].GetType();

                        // Here we check whether the parameter type is inheritable from the constructor parameter type
                        if (!constructorParamType.IsAssignableFrom(paramType))
                        {
                            doesConstructorSignatureMatchParameters = false;
                            break;
                        }
                    }

                    // if constructor signature matches, create class
                    if (doesConstructorSignatureMatchParameters)
                    {
                        return Activator.CreateInstance(classType, parameters);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retruns all methods within a class labelled with attribute tAttribute
        /// </summary>
        /// <param name="type">class</param>
        /// <param name="tAttribute">attribute type t</param>
        /// <returns>list of methods</returns>
        public static IEnumerable<MethodInfo> GetMethodsWithTypeTAttribute(Type type, Type tAttribute)
        {
            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                if (methodInfo.IsDefined(tAttribute))
                {
                    yield return methodInfo;
                }
            }
        }

        /// <summary>
        /// Puts together all methods (and its corresponding classes) that should be invoked
        /// </summary>
        /// <param name="passingTests">List of test names to include.
        /// Methods with names not on this list will be excluded.</param>
        /// <returns>enumerable interfaces of test method infos</returns>
        public static IEnumerable<Tuple<object, MethodInfo>> GetClassMethodTests(List<string> passingTests)
        {
            List<Tuple<object, MethodInfo>> tests = new List<Tuple<object, MethodInfo>>();

            // Loop through each class (i.e., type) and check whether it is a test class
            foreach (Type t in GetClassesWithTypeTAttribute(Assembly.GetCallingAssembly(), typeof(TestClassAttribute), "SocialPlus.EndToEndTests"))
            {
                // instantiate the class
                object classInstance = Activator.CreateInstance(t, null);

                // for each method, invoke it if it's a TestMethod
                foreach (MethodInfo methodInfo in GetMethodsWithTypeTAttribute(t, typeof(TestMethodAttribute)))
                {
                    // Check whether test appears in list of passing tests. Skip otherwise
                    if (!passingTests.Contains(methodInfo.Name))
                    {
                        continue;
                    }

                    tests.Add(new Tuple<object, MethodInfo>(classInstance, methodInfo));
                }
            }

            return tests;
        }

        /// <summary>
        /// Returns all classes in assembly labelled with attribute of type tAttribute
        /// within a namespace
        /// </summary>
        /// <param name="assembly">assembly to peak into</param>
        /// <param name="tAttribute">attribute type t</param>
        /// <param name="nameSpace">assembly name space</param>
        /// <returns>list of classes</returns>
        public static IEnumerable<Type> GetClassesWithTypeTAttribute(Assembly assembly, Type tAttribute, string nameSpace)
        {
            foreach (Type type in assembly.GetTypes().Where(t => t.IsClass && t.Namespace == nameSpace))
            {
                if (type.GetCustomAttributes(tAttribute, true).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
}
