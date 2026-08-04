using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MoonSharp.Interpreter;
using System.Reflection;
using System.Linq;
using Unity.VisualScripting;

namespace RuntimeCardEngine
{
    public class CardGameplayEngine : MonoBehaviour
    {
        public static CardGameplayEngine Instance;

        public string initialScriptsPath = "InitialScripts";

        [Header("Engine Setup")]
        [SerializeField] public Game game;


        public Action<Game> onGamePrepared;

        private void Awake()
        {
            Instance = this;
            LuaIntegration.Initialize();
        }

        public void InitializeGame(Game game)
        {
            this.game = game;
            foreach(Zone z in game.zones)
            {
                foreach (Entity entity in z.entities)
                    InitializeEntityEffects(entity);
            }
            onGamePrepared?.Invoke(game);
            InitializeGameRules(game);
        }

        public void InitializeGameRules(Game game)
        {
            string baseScriptsPath = Path.Combine(Application.streamingAssetsPath, initialScriptsPath);
            string[] scriptPaths;
            try
            {
                scriptPaths = Directory.GetFiles(baseScriptsPath, "*.lua", SearchOption.AllDirectories);
                if (scriptPaths.Length == 0)
                {
                    Debug.LogError($"No LUA script found for lua inside {baseScriptsPath}");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }
            foreach(string scriptPath in scriptPaths)
            {
                string luaCode = File.ReadAllText(scriptPath);

                // 1. Initialize the isolated MoonSharp script sandbox
                Script script = new Script();
                RegisterAllMoonSharpTypes(script);

                // 2. Inject core systems into Lua's global environment
                script.Globals["Game"] = game;

                // 3. Run Lua Code
                script.DoString(luaCode);
                try
                {
                    Debug.Log($"Initializing initial script in {scriptPath}");
                    script.Call(script.Globals["Init"]);
                }
                catch (ScriptRuntimeException ex)
                {
                    Debug.LogError($"[Lua Error on initialScript {scriptPath}: {ex.DecoratedMessage}");
                    return;
                }
            }
        }

        /// <summary>
        /// Kick off execution of a compiled card graph. Finds the trigger and starts the sequence.
        /// </summary>
        public void InitializeEntityEffects(Entity sourceEntity)
        {
            RuntimeCardEffectsData cardData = sourceEntity.runtimeCardEffects;
            // Initialize all nodes, which assigns their runtime lua script to them
            List<RuntimeNode> nodes = cardData.nodes;
            foreach (RuntimeNode node in nodes)
            {
                InitializeNode(node, sourceEntity);
            }
        }

        protected void RegisterAllMoonSharpTypes(Script script)
        {
            // 1. Find all types marked with [MoonSharpUserData]
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(MoonSharpUserDataAttribute), false).Length > 0);

            foreach (var type in types)
            {
                // 2. Register the base class/type with MoonSharp
                UserData.RegisterType(type);

                // 3. Dynamically construct typeof(List<Type>) using reflection
                Type listType = typeof(List<>).MakeGenericType(type);

                // 4. Register the generic List type with MoonSharp
                UserData.RegisterType(listType);

                // 5. Expose the class type to Lua globals for instantiation
                script.Globals[type.Name] = type;

                // 6. Expose the List type to Lua globals (e.g., script.Globals["List_Entity"] or "List_Zone")
                // Note: Avoiding '<' and '>' in global names makes accessing them cleaner in Lua!
                script.Globals[$"List_{type.Name}"] = listType;
            }
        }

        public void InitializeNode(RuntimeNode node, Entity sourceEntity)
        {
            string baseScriptsPath = Path.Combine(Application.streamingAssetsPath, NodeTemplateManager.Instance.templatesPath);
            string scriptPath = "";
            try
            {
                string[] paths = Directory.GetFiles(baseScriptsPath, $"{node.nodeType}.lua", SearchOption.AllDirectories);
                if (paths.Length > 0)
                {
                    scriptPath = paths[0];
                }
                else
                {
                    Debug.LogError($"No LUA script found for {node.nodeType} inside ({sourceEntity.entityName})");
                    return;
                }
            } catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }

            string luaCode = File.ReadAllText(scriptPath);

            // 1. Initialize the isolated MoonSharp script sandbox
            Script script = new Script();

            // 2. Inject core systems into Lua's global environment
            script.Globals["Game"] = game;
            script.Globals["Source"] = sourceEntity;
            script.Globals["Node"] = node;
            script.Globals["Debug"] = typeof(Debug);
            // Get all types in the assembly with the MoonSharpUserData attribute
            RegisterAllMoonSharpTypes(script);

            // 3. Inject the custom field values from the Node Editor (e.g., "Amount" -> 5)
            Table fieldsTable = new Table(script);
            foreach (var kvp in node.fieldValues)
            {
                fieldsTable[kvp.Key] = kvp.Value;
            }
            script.Globals["Fields"] = fieldsTable;

            // 4. Handle helper functions
            // Lua script reference will be needed later to handle Execute() function
            node.luaScript = script;
            // This gives the LUA script control over when to call the flow
            script.Globals["HandleFlow"] = (Action<string>)(async output =>
            {
                string outputId = node.GetOutputSocketIdById(output);
                await HandleFlow(outputId, node, sourceEntity);
            });
            // Gives a convenient way for the script to set the events outputs
            script.Globals["HandleOutputs"] = (Action<Event>)(eve =>
            {
                HandleOutputs(eve, node);
            });
            // 5. Initialize the Lua script
            try
            {
                script.DoString(luaCode);
                Debug.Log($"Initializing {node.nodeType} script in {sourceEntity.entityName}");
                script.Call(script.Globals["Init"]);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.LogError($"[Lua Error in {node.nodeType} on Entity: {sourceEntity.runtimeId} with name {sourceEntity.entityName}]: {ex.DecoratedMessage}");
                return;
            }
        }

        public async Task HandleFlow(string outputId, RuntimeNode node, Entity entity)
        {
            await ExecuteNextFlowStepAsync(node, outputId, entity);
        }

        public void HandleOutputs(Event eve, RuntimeNode node)
        {
            foreach(var output in eve.output)
            {
                node.SetOutputValue(output.Key, output.Value);
            }
        }

    /// <summary>
    /// Moves the flow sequentially down the graph by tracing a target connection wire.
    /// </summary>
    private async Task ExecuteNextFlowStepAsync(RuntimeNode currentNode, string outputId, Entity sourceEntity)
        {
            RuntimeCardEffectsData data = sourceEntity.runtimeCardEffects;
            // Find the connection wire plugged into our output flow slot
            RuntimeConnection wire = data.connections.Find(c =>
                c.fromNodeId == currentNode.id &&
                c.fromPortId == outputId
            );

            if (wire == null)
            {
                Debug.Log($"[Engine] Sequence flow ended for node {currentNode.nodeType} inside ${sourceEntity.entityName}.");
                return;
            }

            // Find the downstream node
            RuntimeNode nextNode = data.nodes.Find(n => n.id == wire.toNodeId);
            if (nextNode == null) return;

            // Execute the action node and wait for it to complete
            await ExecuteNextNodeAsync(nextNode, sourceEntity);
        }

        /// <summary>
        /// Configures inputs, runs a Lua node, and pauses execution until its Event finishes.
        /// </summary>
        private async Task ExecuteNextNodeAsync(RuntimeNode node, Entity sourceEntity)
        {
            RuntimeCardEffectsData data = sourceEntity.runtimeCardEffects;

            Debug.Log($"[Engine] Executing Node: {node.nodeType} inside {sourceEntity.entityName}");

            // 1. Prepare an isolated, clean MoonSharp context
            Script script = node.luaScript;

            var eventCompletionSource = new TaskCompletionSource<bool>();

            // Used for constants, utility and targeting nodes to notify when they are finished
            script.Globals["HandleFinish"] = (Action)(() =>
            {
                Debug.Log($"[Engine] Finished executing Node: {node.nodeType} inside {sourceEntity.entityName}");
                eventCompletionSource.SetResult(true);
            });

            // Easy way to setup Actions that only send 1 event
            script.Globals["HandleEventSetup"] = (Action<Event>)(e =>
            {
                HandleEventSetup(e, node, sourceEntity);
            });

            // Used for Action Nodes to trigger their onDone and onTry flows. This is AFTER handleFinish is called
            script.Globals["HandleFlow"] = (Action<string>)(output =>
            {
                string outputId = node.GetOutputSocketIdById(output);
                _ = HandleFlow(outputId, node, sourceEntity);
            });
            // 2. Resolve all parameters. Recursively looks upstream if connections exist.
            Dictionary<string, object> resolvedInputs = new Dictionary<string, object>();

            // Note: In production, find templates via your CardCreatorEditor registry:
            NodeTemplate template = NodeTemplateManager.Instance.GetNodeTemplate(node.nodeType);

            if (template != null)
            {
                foreach (var port in template.inputs)
                {
                    if (port.portType == "Flow") continue; // Exclude execution lines

                    // Search for an upstream data wire feeding this parameter port
                    RuntimeConnection dataWire = data.connections.Find(c =>
                        c.toNodeId == node.id &&
                        c.toPortId == node.GetInputSocketIdById(port.id)
                    );

                    if (dataWire != null)
                    {
                        // Upstream Connection: Backtrack recursively to compute its value
                        RuntimeNode upstreamNode = data.nodes.Find(n => n.id == dataWire.fromNodeId);
                        object output = upstreamNode.GetOutputValue<object>(dataWire.fromPortId);
                        if (output == null)
                            await ExecuteNextNodeAsync(upstreamNode, sourceEntity);
                        output = upstreamNode.GetOutputValue<object>(dataWire.fromPortId);
                        resolvedInputs[port.id] = output;
                    }
                    else
                    {
                        // Direct Input: Fallback to static text fields typed into the Node UI
                        node.fieldValues.TryGetValue(port.portName, out object valueText);
                        resolvedInputs[port.id] = valueText ?? "0";
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to find node {node.nodeType} while executing {sourceEntity.entityName}");
            }
            // Inside ExecuteNextNodeAsync in C#:
            Table inputsTable = new Table(script);
            // Assign inputs to node
            foreach (var port in resolvedInputs)
            {
                node.SetInputValue(port.Key, port.Value);
                inputsTable[port.Key] = port.Value;
            }
            script.Globals["Inputs"] = inputsTable;

            // Run Execute script, should call HandleFinish when done
            try
            {
                if (script == null)
                {
                    Debug.LogError($"[Engine Error]: Script instance is null for node '{node.nodeType}'");
                    eventCompletionSource.TrySetException(new NullReferenceException("Script instance is null"));
                    return;
                }

                // 1. Explicitly verify function existence
                DynValue executeFunc = script.Globals.Get("Execute");
                if (executeFunc.IsNil())
                {
                    Debug.LogError($"[Lua Error]: 'Execute' function is missing/nil in script for node '{node.nodeType}'");
                    eventCompletionSource.TrySetException(new MissingMethodException($"Execute function missing in {node.nodeType}"));
                    return;
                }

                // 2. Safely call the function
                script.Call(executeFunc);
                Debug.Log($"[Engine] Executed: {node.nodeType} inside {sourceEntity.entityName}");
            }
            catch (ScriptRuntimeException ex)
            {
                string inner = ex.InnerException != null ? $"\n---> Caused by C# Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}" : "";
                Debug.LogError($"[Lua Script Error in {node.nodeType}]: {ex.DecoratedMessage}{inner}");
                eventCompletionSource.TrySetException(ex);
            }
            catch (InterpreterException ex)
            {
                Debug.LogError($"[MoonSharp Engine Error in {node.nodeType}]: {ex.Message}");
                eventCompletionSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[C# Host Error in {node.nodeType}]: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                eventCompletionSource.TrySetException(ex);
            }
            await eventCompletionSource.Task;
        }

        public void HandleEventSetup(Event eve, RuntimeNode node, Entity sourceEntity)
        {
            eve.entitySource = sourceEntity;
            eve.OnTry += (e) =>
            {
                HandleOutputs(e, node);
                HandleFlow("onTry", node, sourceEntity);
            };
            eve.OnDone += (e) =>
            {
                HandleOutputs(e, node);
                HandleFlow("onDone", node, sourceEntity);
            };
        }
    }
}