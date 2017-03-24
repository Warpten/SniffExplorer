using System;
using System.Linq.Expressions;
using SniffExplorer.Core.Utils;

namespace SniffExplorer.Core.Packets.Parsing.Attributes
{
    /// <summary>
    /// Use this attribute to indicate that the associated property is
    /// stored on bits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BitFieldAttribute : Attribute
    {
        public int BitSize { get; }

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
