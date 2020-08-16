using System;

namespace Qulaly.Syntax
{
    public partial class SelectorSyntaxReader
    {
        public bool Expect(params Func<bool>[] nextAlternates)
        {
            var origIndex = Index;
            foreach (var next in nextAlternates)
            {
                using var childScope = _productionScope.BeginScope();
                if (next())
                {
                    childScope.Commit();
                    return true;
                }

                // rewind
                Index = origIndex;
            }
            return false;
        }

        public bool ExpectZeroOrMore(Func<bool> next, int maxCount = int.MaxValue)
        {
            return ExpectZeroOrMore(next, out _, maxCount);
        }

        public bool ExpectZeroOrMore(Func<bool> next, out bool isZero, int maxCount = int.MaxValue)
        {
            var origIndex = Index;
            var isSuccess = true;
            var count = 0;

            while (isSuccess && count++ < maxCount)
            {
                using var childScope = _productionScope.BeginScope();
                
                isSuccess = next();

                if (isSuccess)
                {
                    origIndex = Index;
                    childScope.Commit();
                }
                else
                {
                    // rewind if no more next
                    Index = origIndex;
                }
            }

            isZero = count == 1 && !isSuccess;

            return true;
        }

        public bool ExpectOneOrMore(Func<bool> next, int maxCount = int.MaxValue)
        {
            using var childScope = _productionScope.BeginScope();
            var isSuccess = true;
            var origIndex = Index;
            // One
            isSuccess &= next();

            // or More
            if (isSuccess)
            {
                var count = 1;
                var moreIsSuccess = true;
                while (moreIsSuccess && count++ < maxCount)
                {
                    using var childScope2 = _productionScope.BeginScope();

                    origIndex = Index;
                    moreIsSuccess &= next();

                    if (moreIsSuccess)
                    {
                        childScope2.Commit();
                    }
                    else
                    {
                        // rewind
                        Index = origIndex;
                    }
                }
            }
            else
            {
                // rewind
                Index = origIndex;
            }

            if (isSuccess)
            {
                childScope.Commit();
            }

            return isSuccess;
        }

        public bool ExpectZeroOrOne(Func<bool> next)
        {
            return ExpectZeroOrOne(next, out _);
        }

        public bool ExpectZeroOrOne(Func<bool> next, out bool isZero)
        {
            using var childScope = _productionScope.BeginScope();
            var origIndex = Index;
            isZero = false;
            if (!next())
            {
                // rewind if zero
                Index = origIndex;
                isZero = true;
            }

            if (!isZero)
            {
                childScope.Commit();
            }

            return true;
        }
    }
}
