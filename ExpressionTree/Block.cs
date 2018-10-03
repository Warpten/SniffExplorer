using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionTree
{
    public interface IExpression
    {
        Type Type { get; }
        Expression GenerateExpression();
    }

    public class Block : IExpression
    {
        public Type Type => Nodes.Last().Type;

        private List<IExpression> Variables { get; } = new List<IExpression>();
        private List<IExpression> Nodes { get; } = new List<IExpression>();

        public Expression GenerateExpression() {
            return Expression.Block(Variables.Select(v => v.GenerateExpression()).Cast<ParameterExpression>(), Nodes.Select(node => node.GenerateExpression()));
        }
    }

    public class Variable<T> : IExpression
    {
        public Type Type { get; } = typeof(T);
        public string Name { get; set; }

        public Expression GenerateExpression() {
            return Expression.Variable(Type, Name);
        }
    }

    public class Parameter<T> : IExpression
    {
        public string Name { get; set; }
        public Type Type { get; } = typeof(T);

        public Expression GenerateExpression() {
            return Expression.Parameter(Type, Name);
        }
    }

    public class IfThen : IExpression
    {
        public Type Type { get; } = typeof(void);
        public IExpression Condition { get; set; }
        public IExpression Action { get; set; }

        public Expression GenerateExpression() {
            return Expression.IfThen(Condition.GenerateExpression(), Action.GenerateExpression());
        }
    }

    public class IfThenElse : IExpression
    {
        public Type Type { get; } = typeof(void);
        public IExpression Condition { get; set; }
        public IExpression ActionTrue { get; set; }
        public IExpression ActionFalse { get; set; }

        public Expression GenerateExpression() {
            return Expression.IfThenElse(Condition.GenerateExpression(), ActionTrue.GenerateExpression(), ActionFalse.GenerateExpression());
        }
    }

    public enum ComparisonOperand
    {
        And,
        Or,
        Xor,
        Xnor,
        Nand,
        Nor,

        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }

    public abstract class BinaryComparison<T> : IExpression
    {
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }
        private ComparisonOperand Operand { get; set; }

        public Type Type { get; } = typeof(T);

        public BinaryComparison(ComparisonOperand op)
        {
            Operand = op;
        }

        public Expression GenerateExpression()
        {
            if (!Type.IsAssignableFrom(Left.Type))
                throw new InvalidOperationException();

            if (!Type.IsAssignableFrom(Right.Type))
                throw new InvalidOperationException();

            switch (Operand)
            {
                case ComparisonOperand.And:
                    return Expression.And(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.Or:
                    return Expression.Or(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.Xor:
                    return Expression.ExclusiveOr(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.Xnor:
                    return Expression.ExclusiveOr(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.Nand:
                    return Expression.And(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.Nor:
                    return Expression.Or(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.Equal:
                    return Expression.Equal(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.NotEqual:
                    return Expression.NotEqual(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.GreaterThan:
                    return Expression.GreaterThan(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.LessThan:
                    return Expression.LessThan(Left.GenerateExpression(), Right.GenerateExpression());
                case ComparisonOperand.LessThanOrEqual:
                    return Expression.LessThanOrEqual(Left.GenerateExpression(), Right.GenerateExpression());
            }

            throw new InvalidOperationException("Unreachable code");
        }
    }

    public class And<T> : BinaryComparison<T>
    {
        public And() : base(ComparisonOperand.And) { }
    }
}
