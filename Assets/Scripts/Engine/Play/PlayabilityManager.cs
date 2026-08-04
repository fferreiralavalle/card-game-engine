using MoonSharp.Interpreter;
using RuntimeCardEngine;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Playability manager supports ordered/priority rules (higher priority runs first).
/// Rules may be registered from mods; they may be C# objects, delegates or Lua functions.
/// </summary>
[MoonSharpUserData]
public class PlayabilityManager : MonoBehaviour
{
    public static PlayabilityManager Instance { get; private set; }

    // Keep an ordered list. We store pairs to support priority and stable ordering.
    private readonly List<(IPlayabilityRule rule, int priority, Guid id)> _rules =
        new ();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // Register with priority (higher runs earlier). Returns an id you can use to unregister.
    public Guid RegisterRule(IPlayabilityRule rule, int priority = 0)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        var id = Guid.NewGuid();
        _rules.Add((rule, priority, id));
        // Keep sorted by priority desc (higher first), then insertion order stable since we use List
        _rules.Sort((a, b) => b.priority.CompareTo(a.priority));
        return id;
    }

    public Guid RegisterRule(Func<Game, Entity, PlayabilityResult> func, int priority = 0)
    {
        var wrapper = new DelegatePlayabilityRule(func, priority);
        return RegisterRule(wrapper, priority);
    }

    // Unregister by rule instance
    public bool UnregisterRule(IPlayabilityRule rule)
    {
        var item = _rules.Find(t => t.rule == rule);
        if (item.rule == null) return false;
        return _rules.Remove(item);
    }

    // Unregister by registration id
    public bool UnregisterRule(Guid registrationId)
    {
        var item = _rules.Find(t => t.id == registrationId);
        if (item.rule == null) return false;
        return _rules.Remove(item);
    }

    public void ClearRules() => _rules.Clear();

    /// <summary>
    /// Evaluate all rules. By default collects all failures. Set shortCircuitOnFail = true to stop at first fail.
    /// </summary>
    public PlayabilityResult CheckCanPlay(Game game, Entity card, bool shortCircuitOnFail = false)
    {
        if (game == null) throw new ArgumentNullException(nameof(game));
        if (card == null) throw new ArgumentNullException(nameof(card));

        var failures = new List<string>();

        foreach (var (rule, _, id) in _rules)
        {
            try
            {
                PlayabilityResult res = rule.Check(game, card) ?? PlayabilityResult.Success();
                if (!res.IsPlayable)
                {
                    if (res.Reasons?.Count > 0)
                        failures.AddRange(res.Reasons);
                    else
                        failures.Add($"Rule failed (id={id})");
                    if (shortCircuitOnFail)
                        return PlayabilityResult.Fail(failures);
                }
            }
            catch (Exception ex)
            {
                failures.Add($"Rule exception: {ex.GetType().Name}: {ex.Message}");
                if (shortCircuitOnFail) return PlayabilityResult.Fail(failures);
            }
        }

        return failures.Count == 0 ? PlayabilityResult.Success() : PlayabilityResult.Fail(failures);
    }

    #region Lua helpers

    public Guid RegisterLuaRule(Script luaScript, string globalName, int priority = 0)
    {
        if (luaScript == null) throw new ArgumentNullException(nameof(luaScript));
        if (string.IsNullOrEmpty(globalName)) throw new ArgumentNullException(nameof(globalName));
        DynValue func = luaScript.Globals.Get(globalName);
        if (func.IsNil() || func.Type != DataType.Function)
        {
            Debug.LogWarning($"Lua global '{globalName}' not found or not a function.");
            return Guid.Empty;
        }

        Func<Game, Entity, PlayabilityResult> wrapper = (game, card) =>
        {
            try
            {
                DynValue result = luaScript.Call(func, game, card);
                if (result.IsNil()) return PlayabilityResult.Success();
                if (result.Type == DataType.Boolean) return result.Boolean ? PlayabilityResult.Success() : PlayabilityResult.Fail("Lua rule returned false");
                if (result.Type == DataType.Table)
                {
                    Table t = result.Table;
                    DynValue playableVal = t.Get("playable");
                    bool playable = playableVal.IsNil() ? true : playableVal.Boolean;
                    DynValue reasonVal = t.Get("reason");
                    string reason = reasonVal.IsNil() ? null : reasonVal.String;
                    return playable ? PlayabilityResult.Success() : (reason != null ? PlayabilityResult.Fail(reason) : PlayabilityResult.Fail("Lua rule failed"));
                }
                return PlayabilityResult.Success();
            }
            catch (ScriptRuntimeException ex)
            {
                return PlayabilityResult.Fail($"Lua error: {ex.DecoratedMessage}");
            }
            catch (Exception ex)
            {
                return PlayabilityResult.Fail($"Lua-hosted rule exception: {ex.GetType().Name}: {ex.Message}");
            }
        };

        return RegisterRule(wrapper, priority);
    }
}

#endregion