namespace Qulaly.Matcher.Selectors
{
    public abstract class Selector : SelectorElement
    {
        public abstract SelectorMatcher GetMatcher();
    }
}