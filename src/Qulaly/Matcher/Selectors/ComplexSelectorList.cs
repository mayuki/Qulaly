using System.Linq;

namespace Qulaly.Matcher.Selectors
{
    public class ComplexSelectorList : SelectorList<Selector>
    {
        public ComplexSelectorList(params Selector[] selectors)
        {
            foreach (var selector in selectors)
            {
                AddChild(selector);
            }
        }

        public override SelectorMatcher GetMatcher()
        {
            SelectorMatcher matcher = (in SelectorMatcherContext ctx) => false;

            foreach (var selector in Children)
            {
                matcher = SelectorCompilerHelper.ComposeOr(SelectorCompiler.Compile(selector), matcher);
            }

            return matcher;
        }

        public override string ToSelectorString()
        {
            return string.Join(", ", Children.Select(x => x.ToSelectorString()));
        }
    }
}
