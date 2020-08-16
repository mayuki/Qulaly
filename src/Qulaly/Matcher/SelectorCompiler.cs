using Qulaly.Matcher.Selectors;
using Qulaly.Matcher.Selectors.Combinators;

namespace Qulaly.Matcher
{
    public class SelectorCompiler
    {
        public static SelectorMatcher Compile(SelectorElement selector)
        {
            SelectorMatcher composedMatcher = (in SelectorMatcherContext _) => true;

            if (selector is SelectorList<SelectorElement> selectorList)
            {
                for (var i = 0; i < selectorList.Children.Count; i++)
                {
                    var child = selectorList.Children[i];
                    if (child is Combinator combinator)
                    {
                        var matcher = combinator.GetCombinator(composedMatcher);
                        composedMatcher = SelectorCompilerHelper.ComposeAnd(matcher, Compile(selectorList.Children[i + 1]));
                    }
                    else if (child is Selector childSelector)
                    {
                        var matcher = childSelector.GetMatcher();
                        composedMatcher = SelectorCompilerHelper.ComposeAnd(composedMatcher, matcher);
                    }
                }
            }

            return composedMatcher;
        }
    }
}