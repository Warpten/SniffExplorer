using System;
using System.Linq.Expressions;

namespace SniffExplorer.Core.Attributes
{
    public enum ConditionType : byte
    {
        /// <summary>
        /// Source == Value
        /// </summary>
        Equal,          // x == y

        /// <summary>
        /// Source != Value
        /// </summary>
        Different,      // x != y

        /// <summary>
        /// Source > Value
        /// </summary>
        Greater,        // x > y

        /// <summary>
        /// Source < Value
        /// </summary>
        Less,           // x < y

        /// <summary>
        /// Source >= Value
        /// </summary>
        GreaterOrEqual, // x >= y

        /// <summary>
        /// Source &lt;= Value
        /// </summary>
        LessOrEqual,    // x <= y

        /// <summary>
        /// (Source & Value) != 0
        /// </summary>
        And,            // (x & y) != 0

        /// <summary>
        /// (Source & Value) == 0
        /// </summary>
        Nand,

        /// <summary>
        /// (Source | Value) != 0
        /// </summary>
        Or,             // (x | y) != 0

        /// <summary>
        /// (Source | Value) == 0
        /// </summary>
        Nor,

        /// <summary>
        /// (Source ^ Value) != 0
        /// </summary>
        Xor,            // (x ^ y) != 0

        /// <summary>
        /// (Source ^ Value) == 0
        /// </summary>
        Xnor
    }

    /// <summary>
    /// Use this attribute when a property's presence in the
    /// packet should be decided according to a previously read
    /// value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConditionalAttribute : Attribute
    {
        /// <summary>
        /// The conditional operand to apply.
        /// </summary>
        private ConditionType Type { get; }

        /// <summary>
        /// The source of the comparison.
        /// </summary>
        private string ConditionSource { get; }

        /// <summary>
        /// The values to compare against.
        /// </summary>
        private object[] Right { get; }

        public ConditionalAttribute(string propertyName, ConditionType type, params object[] value)
        {
            ConditionSource = propertyName;
            Type = type;
            Right = value;
        }

        public Expression GetLeftHandExpression(Expression root)
        {
            var sourceTokens = ConditionSource.Split('.');
            foreach (var token in sourceTokens)
            {
                var tokenProperty = root.Type.GetProperty(token);

                if (tokenProperty == null)
                    throw new InvalidOperationException(string.Format("Cannot find property '{0}' in path '{1}' for root node of type {2}.", token, ConditionSource, root.Type.ToString()));

                root = Expression.MakeMemberAccess(root, root.Type.GetProperty(token));
            }

            return root;
        }

        public Expression GetComparisonExpression(Expression left)
        {
            Expression ex = GetComparisonExpression(left, Right[0]);
            for (var i = 1; i < Right.Length; ++i)
                ex = Expression.Or(ex, GetComparisonExpression(left, Right[i]));
            return ex;
        }

        private Expression GetComparisonExpression(Expression left, object r)
        {
            var right = Expression.Constant(r);

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
                    return Expression.NotEqual(Expression.And(left, right), Expression.Default(left.Type));
                case ConditionType.Nand:
                    return Expression.Equal(Expression.And(left, right), Expression.Default(left.Type));
                case ConditionType.Or:
                    return Expression.NotEqual(Expression.Or(left, right), Expression.Default(left.Type));
                case ConditionType.Nor:
                    return Expression.Equal(Expression.Or(left, right), Expression.Default(left.Type));
                case ConditionType.Xor:
                    return Expression.NotEqual(Expression.ExclusiveOr(left, right), Expression.Default(left.Type));
                case ConditionType.Xnor:
                    return Expression.Equal(Expression.ExclusiveOr(left, right), Expression.Default(left.Type));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StopIfAttribute : ConditionalAttribute
    {
        public StopIfAttribute(string propertyName, ConditionType type, params object[] value) : base(propertyName, type, value)
        {
        }
    }
}
