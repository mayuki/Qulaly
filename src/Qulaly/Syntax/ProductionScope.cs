using System;
using System.Collections.Generic;

namespace Qulaly.Syntax
{
    public class ProductionScope : IDisposable
    {
        private Stack<List<Production>> _stackProduction;
        private Stack<List<string>> _stackCaptures;
        private bool _committed;

        public ProductionScope()
        {
            _stackProduction = new Stack<List<Production>>();
            _stackProduction.Push(new List<Production>());
            _stackCaptures = new Stack<List<string>>();
            _stackCaptures.Push(new List<string>());
        }

        public ProductionScope(Stack<List<Production>> stackProduction, Stack<List<string>> stackCaptures)
        {
            _stackProduction = stackProduction;
            _stackProduction.Push(new List<Production>());
            _stackCaptures = stackCaptures;
            _stackCaptures.Push(new List<string>());
        }

        public List<Production> GetProductions() => _stackProduction.Peek();
        public List<string> GetCaptures() => _stackCaptures.Peek();

        public ProductionScope BeginScope()
        {
            return new ProductionScope(_stackProduction, _stackCaptures);
        }

        public void AddProduction(Production value)
        {
            _stackProduction.Peek().Add(value);
        }

        public void AddCapture(string value)
        {
            _stackCaptures.Peek().Add(value);
        }

        public void Commit()
        {
            _committed = true;
        }

        public void Dispose()
        {
            var scopedProductions = _stackProduction.Pop();
            var parentProduction = _stackProduction.Peek();
            var scopedCaptures = _stackCaptures.Pop();
            var parentCaptures = _stackCaptures.Peek();

            if (_committed)
            {
                parentProduction.AddRange(scopedProductions);
                parentCaptures.AddRange(scopedCaptures);
            }
        }
    }
}