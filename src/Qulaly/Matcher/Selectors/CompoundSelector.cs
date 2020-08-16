using System;
using System.Linq;

namespace Qulaly.Matcher.Selectors
{
    public class CompoundSelector : SelectorList<Selector>
    {
        public CompoundSelector(params Selector[] selectors)
        {
            foreach (var selector in selectors)
            {
                if (
                    (selector is TypeSelector) ||
                    (selector is PropertySelector) ||
                    (selector is PseudoElementSelector) ||
                    (selector is PseudoClassSelector)
                )
                {
                    AddChild(selector);
                }
                else
                {
                    throw new ArgumentException($"An element of SimpleSelectorSequence must be TypeSelector, PropertySelector, PseudoElementSelector, PseudoClassSelector. ({selector.ToString()})");
                }
            }
        }

        public override SelectorMatcher GetMatcher()
        {
            SelectorMatcher composedMatcher = (in SelectorMatcherContext _) => true;

            for (var i = Children.Count - 1; i >= 0; i--)
            {
                var child = Children[i];
                var matcher = child.GetMatcher();
                composedMatcher = SelectorCompilerHelper.ComposeAnd(composedMatcher, matcher);
            }

            return composedMatcher;
        }

        public override string ToSelectorString()
        {
            return string.Concat(Children.Select(x => x.ToSelectorString()));
        }
    }
}