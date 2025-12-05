using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class ParameterToConstantReplacer : ExpressionVisitor
    {
        private readonly Dictionary<string, double> _values;

        public ParameterToConstantReplacer(Dictionary<string, double> values)
        {
            _values = values;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_values.TryGetValue(node.Name, out double val))
                return Expression.Constant(val);

            return base.VisitParameter(node);
        }
    }
}
