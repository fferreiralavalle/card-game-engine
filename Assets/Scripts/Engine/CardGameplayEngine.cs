using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

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
        }

        public async Task InitializeGame(Game game)
        {
            RegisterMoonSharpTypes();
            LuaIntegration.Initialize();
            this.game = game;
            foreach(Zone z in game.zones)
            {
                foreach (Entity entity in z.entities)
                    await InitializeEntityEffects(entity);
            }

            // Register default play-cost rule for playability checks
            if (PlayabilityManager.Instance == null)
            {
                // Ensure there is a PlayabilityManager in the scene
                var go = new GameObject("PlayabilityManager");
                go.AddComponent<PlayabilityManager>();
            }
            PlayabilityManager.Instance.RegisterRule(new PlayCostRule());

            InitializeGameRules(game);

            onGamePrepared?.Invoke(game);
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

            // 1. Load and prepare all scripts (but do not call Init yet)
            var loadedScripts = new List<(Script script, string path)>();
            foreach (string scriptPath in scriptPaths)
            {
                string luaCode = File.ReadAllText(scriptPath);

                // Initialize the isolated MoonSharp script sandbox
                Script script = new Script();
                RegisterTypesInScript(script);

                // Inject core systems into Lua's global environment
                script.Globals["game"] = game;

                // Inject helper so Lua scripts can register their own playability checks
                script.Globals["RegisterPlayabilityRule"] = (Action<string>)((functionName) =>
                {
                    if (PlayabilityManager.Instance == null) return;
                    PlayabilityManager.Instance.RegisterLuaRule(script, functionName, 0);
                });
                script.Globals["RegisterPlayabilityRuleWithPriority"] = (Action<string, int>)((functionName, priority) =>
                {
                    if (PlayabilityManager.Instance == null) return;
                    PlayabilityManager.Instance.RegisterLuaRule(script, functionName, priority);
                });

                // Optionally expose PlayabilityManager instance for scripts
                script.Globals["PlayabilityManager"] = PlayabilityManager.Instance;

                // Run the script to populate its globals (but don't call Init yet)
                try
                {
                    script.DoString(luaCode);
                }
                catch (ScriptRuntimeException ex)
                {
                    Debug.LogError($"[Lua Error loading initialScript {scriptPath}: {ex.DecoratedMessage}");
                    return;
                }

                loadedScripts.Add((script, scriptPath));
            }

            // 2. Query priority from each script (GetPriority function) and sort
            var prioritized = new List<(Script script, string path, float priority)>();
            foreach (var (script, path) in loadedScripts)
            {
                float priority = 0f;
                try
                {
                    DynValue getPr = script.Globals.Get("GetPriority");
                    if (!getPr.IsNil() && getPr.Type == DataType.Function)
                    {
                        DynValue res = script.Call(getPr);
                        if (res != null && res.Type == DataType.Number)
                        {
                            priority = (float)res.Number;
                        }
                        else
                        {
                            // Non-number returned -> default 0
                            priority = 0f;
                        }
                    }
                    else
                    {
                        // No GetPriority -> default 0
                        priority = 0f;
                    }
                }
                catch (ScriptRuntimeException ex)
                {
                    Debug.LogWarning($"[Lua Warning] GetPriority threw in {path}: {ex.DecoratedMessage}. Defaulting priority=0.");
                    priority = 0f;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Lua Warning] Error evaluating GetPriority in {path}: {ex.Message}. Defaulting priority=0.");
                    priority = 0f;
                }

                prioritized.Add((script, path, priority));
            }

            // Order by priority descending, stable by path to have deterministic ordering
            var ordered = prioritized
                .OrderByDescending(s => s.priority)
                .ThenBy(s => s.path, StringComparer.Ordinal)
                .ToList();

            // 3. Execute Init() on each script in priority order
            foreach (var item in ordered)
            {
                try
                {
                    Debug.Log($"Initializing initial script (priority={item.priority}) in {item.path}");
                    DynValue initFunc = item.script.Globals.Get("Init");
                    if (initFunc.IsNil())
                    {
                        Debug.LogWarning($"[Lua Warning] Init() not found in initial script {item.path}");
                        continue;
                    }
                    item.script.Call(initFunc);
                }
                catch (ScriptRuntimeException ex)
                {
                    Debug.LogError($"[Lua Error on initialScript {item.path}: {ex.DecoratedMessage}");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[C# Error invoking Init in {item.path}: {ex.GetType().Name}: {ex.Message}");
                    return;
                }
            }
        }

        /// <summary>
        /// Kick off execution of a compiled card graph. Finds the trigger and starts the sequence.
        /// </summary>
        public async Task InitializeEntityEffects(Entity sourceEntity)
        {
            RuntimeCardEffectsData cardData = sourceEntity.runtimeCardEffects;
            if (cardData == null || cardData.nodes == null || cardData.nodes.Count == 0)
                return;

            // Map nodes by id for quick lookup
            var nodes = cardData.nodes;
            var nodeById = nodes.ToDictionary(n => n.id);

            // Prepare adjacency and indegree counters
            var adjacency = new Dictionary<string, List<string>>();
            var indegree = new Dictionary<string, int>();
            foreach (var n in nodes)
            {
                adjacency[n.id] = new List<string>();
                indegree[n.id] = 0;
            }

            // Build edges only for connections that represent data dependencies (i.e., feed an input port that is not a Flow)
            foreach (var conn in cardData.connections)
            {
                if (string.IsNullOrEmpty(conn.fromNodeId) || string.IsNullOrEmpty(conn.toNodeId))
                    continue;
                if (!nodeById.ContainsKey(conn.fromNodeId) || !nodeById.ContainsKey(conn.toNodeId))
                    continue;

                var toNode = nodeById[conn.toNodeId];

                // Find input socket index for this connection, if any
                int inputIndex = toNode.inputSocketIds != null ? toNode.inputSocketIds.FindIndex(id => id == conn.toPortId) : -1;

                // If we can find a matching input port, inspect the template input type to skip Flow ports
                bool isFlowPort = false;
                if (inputIndex >= 0 && toNode.template != null && toNode.template.inputs != null && inputIndex < toNode.template.inputs.Count)
                {
                    var portDef = toNode.template.inputs[inputIndex];
                    if (portDef != null && portDef.portType == "Flow")
                        isFlowPort = true;
                }

                // If it's a Flow port, skip as it represents control flow not data dependency
                if (isFlowPort)
                    continue;

                // Otherwise treat as a data dependency edge from -> to
                adjacency[conn.fromNodeId].Add(conn.toNodeId);
                indegree[conn.toNodeId] = indegree[conn.toNodeId] + 1;
            }

            // Kahn's algorithm: start with nodes with indegree 0
            var queue = new Queue<RuntimeNode>();
            foreach (var n in nodes)
            {
                if (indegree[n.id] == 0)
                    queue.Enqueue(n);
            }

            var initialized = new HashSet<string>();

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                await InitializeNode(node, sourceEntity);
                initialized.Add(node.id);

                foreach (var neighborId in adjacency[node.id])
                {
                    indegree[neighborId] = indegree[neighborId] - 1;
                    if (indegree[neighborId] == 0 && nodeById.TryGetValue(neighborId, out var neighborNode))
                    {
                        queue.Enqueue(neighborNode);
                    }
                }
            }

            // If some nodes remain (cycle or unresolved), initialize them and warn
            var remaining = nodes.Where(n => !initialized.Contains(n.id)).ToList();
            if (remaining.Count > 0)
            {
                Debug.LogWarning($"[Engine] Cyclic or unresolved node dependencies detected in '{sourceEntity.entityName}'. Initializing remaining {remaining.Count} nodes.");
                foreach (var node in remaining)
                {
                    await InitializeNode(node, sourceEntity);
                }
            }
        }

        protected void RegisterMoonSharpTypes()
        {
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
            }
        }

        protected void RegisterTypesInScript(Script script)
        {
            // 1. Find all types marked with [MoonSharpUserData]
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(MoonSharpUserDataAttribute), false).Length > 0);

            foreach (var type in types)
            {
                // 1. Dynamically construct typeof(List<Type>) using reflection
                Type listType = typeof(List<>).MakeGenericType(type);

                // 2. Expose the class type to Lua globals for instantiation
                script.Globals[type.Name] = type;

                // 3. Expose the List type to Lua globals (e.g., script.Globals["List_Entity"] or "List_Zone")
                script.Globals[$"List_{type.Name}"] = listType;
            }

            // Optionally register some runtime helper types used by Lua rules
            UserData.RegisterType(typeof(PlayabilityManager));
            script.Globals["PlayabilityManager"] = PlayabilityManager.Instance;
        }

        public async Task InitializeNode(RuntimeNode node, Entity sourceEntity)
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
            script.Globals["game"] = game;
            script.Globals["Source"] = sourceEntity;
            script.Globals["Node"] = node;
            script.Globals["Debug"] = typeof(Debug);
            // Get all types in the assembly with the MoonSharpUserData attribute
            RegisterTypesInScript(script);

            // 2b. Allow node scripts to register playability checks if needed
            script.Globals["RegisterPlayabilityRule"] = (Action<string>)((functionName) =>
            {
                if (PlayabilityManager.Instance == null) return;
                PlayabilityManager.Instance.RegisterLuaRule(script, functionName, 0);
            });
            script.Globals["RegisterPlayabilityRuleWithPriority"] = (Action<string, int>)((functionName, priority) =>
            {
                if (PlayabilityManager.Instance == null) return;
                PlayabilityManager.Instance.RegisterLuaRule(script, functionName, priority);
            });

            await ResolveNodeInputs(node, sourceEntity, script, "Init");

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
            string functionToExecute = nextNode.GetInputPortFromSocketId(wire.toPortId)?.function; // Default function to call on the next node
            string finalFunction = String.IsNullOrEmpty(functionToExecute) ? "Execute" : functionToExecute;
            
            // Execute the action node and wait for it to complete
            await ExecuteNextNodeAsync(nextNode, sourceEntity, finalFunction);
        }

        public async Task ResolveNodeInputs(RuntimeNode node, Entity sourceEntity, Script script, string function = "Execute")
        {
            RuntimeCardEffectsData data = sourceEntity.runtimeCardEffects;
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

                        // Treat null OR empty List (List<object>) as "no value" so we execute the upstream node to populate it
                        bool isEmptyList = output is System.Collections.IList lst && lst.Count == 0;
                        if (output == null || isEmptyList)
                            await ExecuteNextNodeAsync(upstreamNode, sourceEntity, function);
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
                Debug.LogError($"Failed to find node {node.nodeType} while executing {function} inside {sourceEntity.entityName}");
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
        }

        /// <summary>
        /// Configures inputs, runs a Lua node, and pauses execution until its Event finishes.
        /// </summary>
        private async Task ExecuteNextNodeAsync(RuntimeNode node, Entity sourceEntity, string function = "Execute")
        {
            RuntimeCardEffectsData data = sourceEntity.runtimeCardEffects;

            Debug.Log($"[Engine] Executing Node: {node.nodeType} with script {function} inside {sourceEntity.entityName}");

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

            await ResolveNodeInputs(node, sourceEntity, script, function);

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
                DynValue executeFunc = script.Globals.Get(function);
                if (executeFunc.IsNil())
                {
                    Debug.LogError($"[Lua Error]: {function} function is missing/nil in script for node '{node.nodeType}'");
                    eventCompletionSource.TrySetException(new MissingMethodException($"{function} function missing in {node.nodeType}"));
                    return;
                }

                // 2. Safely call the function
                script.Call(executeFunc);
                // Inits dont cann HandleFinish()
                if (function == "Init")
                    eventCompletionSource.SetResult(true);
                Debug.Log($"[Engine] Executed function {function}: {node.nodeType} inside {sourceEntity.entityName}");
            }
            catch (ScriptRuntimeException ex)
            {
                string inner = ex.InnerException != null ? $"\n---> Caused by C# Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}" : "";
                Debug.LogError($"[Lua Script Error in {node.nodeType} function {function}]: {ex.DecoratedMessage}{inner}");
                eventCompletionSource.TrySetException(ex);
            }
            catch (InterpreterException ex)
            {
                Debug.LogError($"[MoonSharp Engine Error in {node.nodeType} {function}]: {ex.Message}");
                eventCompletionSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[C# Host Error in {node.nodeType} {function}]: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
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
                string onTryId = node.GetOutputSocketIdById("onTry");
                HandleFlow(onTryId, node, sourceEntity);
            };
            eve.OnDone += (e) =>
            {
                HandleOutputs(e, node);
                string onDoneId = node.GetOutputSocketIdById("onDone");
                HandleFlow(onDoneId, node, sourceEntity);
            };
        }
    }
}