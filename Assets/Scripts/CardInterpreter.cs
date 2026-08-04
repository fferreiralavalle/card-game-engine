using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter; // Our Lua engine!
using static UnityEngine.EventSystems.EventTrigger;

namespace RuntimeCardEngine
{
    public class CardInterpreter : MonoBehaviour
    {
        private string luaScriptsPath;
    }
}