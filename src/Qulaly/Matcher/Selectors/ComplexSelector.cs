using System.Linq;

namespace Qulaly.Matcher.Selectors
{
    public class ComplexSelector : SelectorList<SelectorElement>
    {
        public ComplexSelector(params SelectorElement[] selectors)
        {
            foreach (var selector in selectors)
            {
                AddChild(selector);
            }
        }

        public override SelectorMatcher GetMatcher()
        {
            return SelectorCompiler.Compile(this);
        }

        public override string ToSelectorString()
        {
            return string.Concat(Children.Select(x => x.ToSelectorString()));
        }
    }
}
