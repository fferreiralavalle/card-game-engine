using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCardEngine
{
    public class PlayabilityResult
    {
        public bool IsPlayable { get; }
        public List<string> Reasons { get; }

        public PlayabilityResult(bool isPlayable, IEnumerable<string> reasons = null)
        {
            IsPlayable = isPlayable;
            Reasons = reasons != null ? new List<string>(reasons) : new List<string>();
        }

        public static PlayabilityResult Success() => new PlayabilityResult(true);
        public static PlayabilityResult Fail(string reason) => new PlayabilityResult(false, new[] { reason });
        public static PlayabilityResult Fail(IEnumerable<string> reasons) => new PlayabilityResult(false, reasons);
    }

    public interface IPlayabilityRule
    {
        PlayabilityResult Check(Game game, Entity card);
    }

    public class DelegatePlayabilityRule : IPlayabilityRule
    {
        private readonly Func<Game, Entity, PlayabilityResult> _func;
        public int Priority { get; }

        public DelegatePlayabilityRule(Func<Game, Entity, PlayabilityResult> func, int priority = 0)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
            Priority = priority;
        }

        public PlayabilityResult Check(Game game, Entity card) => _func(game, card);
    }
}
