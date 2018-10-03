using System;
using System.Linq.Expressions;
using SniffExplorer.Core.Utils;

namespace SniffExplorer.Core.Attributes
{
    /// <summary>
    /// Use this attribute to indicate that bit count is associated to the property.
    ///
    /// If you want to read bits on a new alignment, add <see cref="ResetBitsAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BitFieldAttribute : Attribute
    {
        private int BitSize { get; }

        public BitFieldAttribute(int bitSize = 1)
        {
            BitSize = bitSize;
        }

        internal Expression GetCallExpression(Expression argumentExpression, Type propertyType)
        {
            return Expression.Convert(BitSize == 1 ?
                Expression.Call(argumentExpression, ExpressionUtils.Bit) :
                Expression.Call(argumentExpression, ExpressionUtils.Bits, Expression.Constant(BitSize)), propertyType);
        }
    }
}
