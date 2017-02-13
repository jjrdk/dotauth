﻿#region copyright
// Copyright 2017 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class ReflectionHelper
    {
        private static Dictionary<int, OpCode> _mappingIndiceToOpCode = new Dictionary<int, OpCode>
        {
            { 0, OpCodes.Ldarg_1 },
            { 1, OpCodes.Ldarg_2 },
            { 2, OpCodes.Ldarg_3 }
        };

        public static TypeInfo CreateNewAnonymousType<TSource>(IEnumerable<string> fieldNames)
        {
            return CreateNewAnonymousType(typeof(TSource), fieldNames);
        }

        public static TypeInfo CreateNewAnonymousType(Type sourceType, IEnumerable<string> fieldNames)
        {
            // 1. Declare the type.
            var objType = Type.GetType("System.Object");
            var objCtor = objType.GetConstructor(new Type[0]);
            var dynamicAssemblyName = new AssemblyName("TempAssm");
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssm");
            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("TempCl", TypeAttributes.Public);
            var properties = fieldNames.Select(f => new { Prop = sourceType.GetProperty(f), Name = f });
            var builder = dynamicAnonymousType.DefineConstructor(MethodAttributes.Assembly, CallingConventions.Standard, properties.Select(p => p.Prop.PropertyType).ToArray());
            var constructorIl = builder.GetILGenerator();
            var equalsMethod = dynamicAnonymousType.DefineMethod("Equals", 
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot
                | MethodAttributes.Virtual| MethodAttributes.Final,CallingConventions.HasThis, typeof(bool), new[] { typeof(object) });
            var getHashCodeMethod = dynamicAnonymousType.DefineMethod("GetHashCode",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot
                | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis, typeof(int), Type.EmptyTypes);
            var ilEquals = equalsMethod.GetILGenerator();
            var ilHashCode = getHashCodeMethod.GetILGenerator();
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, objCtor);
            /*
            ilEquals.Emit(OpCodes.Ldarg_1);
            ilEquals.Emit(OpCodes.Castclass, dynamicAnonymousType.AsType());
            ilEquals.Emit(OpCodes.Stloc_0);*/
            Label inequality = ilEquals.DefineLabel();
            Label end = ilEquals.DefineLabel();
            int i = 0;
            // 2. Create the constructor & toString method.
            foreach (var property in properties)
            {
                OpCode opCode;
                var kvp = _mappingIndiceToOpCode.FirstOrDefault(m => m.Key == i);
                if (kvp.Equals(default(KeyValuePair<int, OpCode>)))
                {
                    opCode = OpCodes.Ldarg_S;
                }
                else
                {
                    opCode = kvp.Value;
                }

                var field = dynamicAnonymousType.DefineField(property.Name, property.Prop.DeclaringType, FieldAttributes.Public);
                var getProperty = dynamicAnonymousType.DefineMethod("get_" + property.Name, MethodAttributes.Assembly, property.Prop.PropertyType, null);
                var propertyIl = getProperty.GetILGenerator();
                // 2.1 Build constructor
                constructorIl.Emit(OpCodes.Ldarg_0);
                constructorIl.Emit(opCode);
                constructorIl.Emit(OpCodes.Stfld, field);
                // 2.2. Build property
                propertyIl.Emit(OpCodes.Ldarg_0);
                propertyIl.Emit(OpCodes.Ldfld, field);
                propertyIl.Emit(OpCodes.Stloc_0);
                propertyIl.Emit(OpCodes.Ldloc_0);
                propertyIl.Emit(OpCodes.Ret);
                // 2.3 Build the equals method
                /*
                ilEquals.Emit(OpCodes.Ldarg_0);
                ilEquals.Emit(OpCodes.Call, getProperty);
                ilEquals.Emit(OpCodes.Ldloc_0);
                ilEquals.Emit(OpCodes.Callvirt, getProperty);
                ilEquals.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                ilEquals.Emit(OpCodes.Brfalse_S, inequality);
                */
                i++;
            }

            constructorIl.Emit(OpCodes.Ret);
            /*
            ilEquals.Emit(OpCodes.Br_S, end);
            ilEquals.MarkLabel(inequality);
            ilEquals.Emit(OpCodes.Ldc_I4_0);
            ilEquals.Emit(OpCodes.Br_S, end);
            ilEquals.MarkLabel(end);
            ilEquals.Emit(OpCodes.Nop);
            ilEquals.Emit(OpCodes.Stloc_1);
            ilEquals.Emit(OpCodes.Ldloc_1);
            ilEquals.Emit(OpCodes.Ret);
            */

            ilHashCode.Emit(OpCodes.Nop);
            ilHashCode.Emit(OpCodes.Ldc_I4_1);
            ilHashCode.Emit(OpCodes.Stloc_0);
            ilHashCode.Emit(OpCodes.Ldloc_0);
            ilHashCode.Emit(OpCodes.Ret);

            ilEquals.Emit(OpCodes.Nop);
            ilEquals.Emit(OpCodes.Ldc_I4_1);
            ilEquals.Emit(OpCodes.Stloc_0);
            ilEquals.Emit(OpCodes.Ldloc_0);
            ilEquals.Emit(OpCodes.Ret);
            dynamicAnonymousType.DefineMethodOverride(equalsMethod, typeof(object).GetMethod("Equals", new[] { typeof(object) }));
            dynamicAnonymousType.DefineMethodOverride(getHashCodeMethod, typeof(object).GetMethod("GetHashCode"));
            return dynamicAnonymousType.CreateTypeInfo();
        }

        public static TypeInfo CreateNewAnonymousType(Type sourceType, IEnumerable<string> fieldNames, IEnumerable<Type> types)
        {
            var dynamicAssemblyName = new AssemblyName("TempAssm");
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssm");
            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("TempCl", TypeAttributes.Public);
            ConstructorBuilder constructor = dynamicAnonymousType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types.ToArray());
            foreach (var fieldName in fieldNames)
            {
                var property = sourceType.GetProperty(fieldName);
                dynamicAnonymousType.DefineField(fieldName, property.PropertyType, FieldAttributes.Public);
            }

            return dynamicAnonymousType.CreateTypeInfo();
        }
    }
}
