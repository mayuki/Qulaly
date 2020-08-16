namespace Qulaly.Matcher.Selectors.Combinators
{
    /// <summary>
    /// 14.2. Child combinator
    /// </summary>
    public class ChildCombinator : Combinator
    {
        public ChildCombinator() : base(">")
        { }

        public override SelectorMatcher GetCombinator(SelectorMatcher left)
        {
            // 14.2. Child combinator (>)
            // https://www.w3.org/TR/selectors-4/#child-combinators
            return (in SelectorMatcherContext ctx) =>
            {
                return ctx.Node.Parent != null && left(ctx.WithSyntaxNode(ctx.Node.Parent));
            };
        }
    }
}