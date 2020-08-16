namespace Qulaly.Matcher
{
    internal static class SelectorCompilerHelper
    {

        public static SelectorMatcher ComposeAnd(SelectorMatcher left, SelectorMatcher right)
        {
            return (in SelectorMatcherContext ctx) => right(ctx) && left(ctx);
        }

        public static SelectorMatcher ComposeOr(SelectorMatcher left, SelectorMatcher right)
        {
            return (in SelectorMatcherContext ctx) => right(ctx) || left(ctx);
        }
    }
}