namespace Qulaly.Matcher.Selectors.Combinators
{
    /// <summary>
    /// 8. Combinators
    /// </summary>
    public abstract class Combinator : SelectorElement
    {
        protected Combinator(string combinatorValue)
        {
            CombinatorValue = combinatorValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public string CombinatorValue { get; }

        public abstract SelectorMatcher GetCombinator(SelectorMatcher left);

        public override string ToString()
        {
            return $"{nameof(Combinator)}: {CombinatorValue}";
        }

        public override string ToSelectorString()
        {
            return $" {CombinatorValue} ";
        }
    }
}
