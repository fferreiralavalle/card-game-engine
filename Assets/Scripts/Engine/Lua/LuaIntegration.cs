using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCardEngine
{
    public static class LuaIntegration
    {
        public static void Initialize()
        {
            // Register your core game types so Lua can read and modify them safely
            UserData.RegisterAssembly();
            UserData.RegisterType<Entity>();
            UserData.RegisterType<Player>();
            UserData.RegisterType<Game>();
            UserData.RegisterType<RuntimeNode>();
            // In C# script setup:
            UserData.RegisterType<Debug>();

            // Register Lists
            UserData.RegisterType<List<Zone>>();
            UserData.RegisterType<List<Entity>>();
            UserData.RegisterType<List<Resource>>();

            // ==========================================
            // STRING CONVERTERS
            // ==========================================
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<List<string>>(
                (script, list) => {
                    var table = new Table(script);
                    foreach (var item in list) table.Append(DynValue.NewString(item));
                    return DynValue.NewTable(table);
                }
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table, typeof(List<string>),
                dynVal => {
                    List<string> list = new List<string>();
                    foreach (var pair in dynVal.Table.Pairs)
                    {
                        if (pair.Value.Type == DataType.String)
                            list.Add(pair.Value.String);
                    }
                    return list;
                }
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Tuple, typeof(List<string>),
                dynVal => {
                    List<string> list = new List<string>();
                    foreach (var pair in dynVal.Tuple)
                    {
                        if (pair.Type == DataType.String)
                            list.Add(pair.String);
                    }
                    return list;
                }
            );

            // ==========================================
            // OBJECT & GENERIC LIST CONVERTERS (Fixes Cross-Script Table Errors)
            // ==========================================

            // 1. Convert Lua Table -> C# List<object> (Strips Lua script ownership)
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table, typeof(List<object>),
                dynVal => {
                    var list = new List<object>();
                    foreach (var pair in dynVal.Table.Pairs)
                    {
                        list.Add(pair.Value.ToObject());
                    }
                    return list;
                }
            );

            // Convert C# List<object> -> Lua Table
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<List<object>>(
                (script, list) => {
                    var table = new Table(script);
                    foreach (var item in list)
                    {
                        table.Append(DynValue.FromObject(script, item));
                    }
                    return DynValue.NewTable(table);
                }
            );

            // 2. Convert Lua Table -> C# List<Entity>
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table, typeof(List<Entity>),
                dynVal => {
                    var list = new List<Entity>();
                    foreach (var pair in dynVal.Table.Pairs)
                    {
                        var obj = pair.Value.ToObject();
                        if (obj is Entity entity)
                            list.Add(entity);
                    }
                    return list;
                }
            );

            // 3. Convert Lua Table -> C# List<Zone>
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table, typeof(List<Zone>),
                dynVal => {
                    var list = new List<Zone>();
                    foreach (var pair in dynVal.Table.Pairs)
                    {
                        var obj = pair.Value.ToObject();
                        if (obj is Zone zone)
                            list.Add(zone);
                    }
                    return list;
                }
            );

            // ==========================================
            // DELEGATE CONVERTERS
            // ==========================================

            // Register converter for Action<Event, Trigger>
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Function,
                typeof(Action<Event, Trigger>),
                v =>
                {
                    var func = v.Function;
                    return (Action<Event, Trigger>)((ev, trigger) =>
                    {
                        // Call the Lua function with the arguments
                        func.Call(DynValue.FromObject(null, ev), DynValue.FromObject(null, trigger));
                    });
                }
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Function,
                typeof(Action<Event>),
                v =>
                {
                    var func = v.Function;
                    return (Action<Event>)((ev) =>
                    {
                        // Call the Lua function with the arguments
                        func.Call(DynValue.FromObject(null, ev));
                    });
                }
            );
        }
    }
}