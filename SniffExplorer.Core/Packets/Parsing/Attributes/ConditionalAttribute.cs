using System;
using System.Linq.Expressions;

namespace SniffExplorer.Core.Packets.Parsing.Attributes
{
    public enum ConditionType : byte
    {
        Equal,          // x == y
        Different,      // x != y
        Greater,        // x > y
        Less,           // x < y
        GreaterOrEqual, // x >= y
        LessOrEqual,    // x <= y
        And,            // (x & y) != 0
        Or,             // (x | y) != 0
        Xor             // (x ^ y) != 0
    }

    /// <summary>
    /// Use this attribute when a property's presence in the
    /// packet should be decided according to a previously read
    /// value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConditionalAttribute : Attribute
    {
        public ConditionType Type { get; }
        public string PropertyName { get; }
        public object Right { get; }

        public ConditionalAttribute(string propertyName, ConditionType type, object value)
        {
            PropertyName = propertyName;
            Type = type;
            Right = value;
        }

        public Expression GetComparisonExpression(Expression left)
        {
            var right = Expression.Constant(Right);

            switch (Type)
            {
                case ConditionType.Equal:
                    return Expression.Equal(left, right);
                case ConditionType.Different:
                    return Expression.NotEqual(left, right);
                case ConditionType.Greater:
                    return Expression.GreaterThan(left, right);
                case ConditionType.Less:
                    return Expression.LessThan(left, right);
                case ConditionType.GreaterOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);
                case ConditionType.LessOrEqual:
                    return Expression.LessThanOrEqual(left, right);
                case ConditionType.And:
                    return Expression.NotEqual(Expression.And(left, right), Expression.Constant(0));
                case ConditionType.Or:
                    return Expression.NotEqual(Expression.Or(left, right), Expression.Constant(0));
                case ConditionType.Xor:
                    return Expression.NotEqual(Expression.ExclusiveOr(left, right), Expression.Constant(0));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
