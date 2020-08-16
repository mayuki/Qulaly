namespace Qulaly.Matcher.Selectors.Combinators
{
    /// <summary>
    /// 14.1. Descendant combinator
    /// </summary>
    public class DescendantCombinator : Combinator
    {
        public DescendantCombinator() : base(" ")
        { }

        public override SelectorMatcher GetCombinator(SelectorMatcher left)
        {
            // 14.1. Descendant combinator ( )
            // https://www.w3.org/TR/selectors-4/#descendant-combinators
            return (in SelectorMatcherContext ctx) =>
            {
                var parent = ctx.Node.Parent;
                while (parent != null)
                {
                    if (left(ctx.WithSyntaxNode(parent)))
                    {
                        return true;
                    }
                    parent = parent.Parent;
                }
                return false;
            };
        }

        public override string ToSelectorString()
        {
            return " ";
        }
    }
}