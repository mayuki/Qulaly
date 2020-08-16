using System.Linq;

namespace Qulaly.Matcher.Selectors.Combinators
{
    /// <summary>
    /// 14.4. Subsequent-sibling combinator (~)
    /// </summary>
    public class SubsequentSiblingCombinator : Combinator
    {
        public SubsequentSiblingCombinator() : base("~")
        { }

        public override SelectorMatcher GetCombinator(SelectorMatcher left)
        {
            // Subsequent-sibling combinator (~)
            // The elements represented by the two compound selectors share the same parent
            // in the document tree and the element represented by the first compound selector
            // precedes (not necessarily immediately) the element represented by the second one.
            // https://www.w3.org/TR/selectors-4/#general-sibling-combinators
            return (in SelectorMatcherContext ctx) =>
            {
                var node = ctx.Node;
                if (ctx.Node.Parent == null) return false;

                var beforeNodes = ctx.Node.Parent.ChildNodes().TakeWhile(x => x != node).ToList(); // before self.
                var ctx_ = ctx;
                return beforeNodes.Any(x => left(ctx_.WithSyntaxNode(x)));
            };
        }
    }
}