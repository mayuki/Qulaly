using System.Linq;

namespace Qulaly.Matcher.Selectors.Combinators
{
    /// <summary>
    /// 14.3. Next-sibling combinator
    /// </summary>
    public class NextSiblingCombinator : Combinator
    {
        public NextSiblingCombinator() : base("+")
        { }

        public override SelectorMatcher GetCombinator(SelectorMatcher left)
        {
            // 14.3. Next-sibling combinator (+)
            // The elements represented by the two compound selectors share the same parent
            // in the document tree and the element represented by the first compound selector
            // immediately precedes the element represented by the second one.
            // https://www.w3.org/TR/selectors-4/#adjacent-sibling-combinators
            return (in SelectorMatcherContext ctx) =>
            {
                var node = ctx.Node;
                if (ctx.Node.Parent == null) return false;

                var beforeNode = ctx.Node.Parent.ChildNodes().TakeWhile(x => x != node).LastOrDefault();
                return beforeNode != null && left(ctx.WithSyntaxNode(beforeNode));
            };
        }
    }
}