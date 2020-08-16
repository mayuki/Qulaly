namespace Qulaly.Matcher.Selectors
{
    public abstract class SelectorElement
    {
        public abstract string ToSelectorString();

        public override string ToString()
        {
            return $"{this.GetType().Name}: {ToSelectorString()}";
        }
    }
}