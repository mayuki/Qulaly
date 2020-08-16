using System;
using System.Collections.Generic;

namespace Qulaly.Matcher.Selectors
{
    public abstract class SelectorList<T> : Selector
        where T: SelectorElement
    {
        private readonly List<T> _children = new List<T>();

        public IReadOnlyList<T> Children => _children;

        public void AddChild(T child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            _children.Add(child);
        }
    }
}