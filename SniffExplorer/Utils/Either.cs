using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace SniffExplorer.Utils
{
    public sealed class Either<Left, Right> where Left : IComparable where Right : IComparable
    {
        public enum Status
        {
            Right,
            Left,
        }
        
        public Status Side { get; private set; }
        private Left _left;
        private Right _right;

        public Either()
        {
        }

        public override bool Equals(object obj)
        {
            var eObj = obj as Either<Left, Right>;
            if (eObj != null)
                return Equals(eObj);
            return false;
        }

        public bool Equals(Either<Left, Right> other)
        {
            if (other.Side != Side)
                return false;

            switch (Side)
            {
                case Status.Right:
                    return other.RightValue.Equals(RightValue);
                case Status.Left:
                    return other.LeftValue.Equals(LeftValue);
            }
            return false; // Dead code, never happens
        }

        public static bool operator == (Either<Left, Right> left, Either<Left, Right> right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            return left.Equals(right);
        }

        public static bool operator !=(Either<Left, Right> left, Either<Left, Right> right)
        {
            return !(left == right);
        }

        public static implicit operator Left(Either<Left, Right> v)
        {
            return v.LeftValue;
        }

        public static implicit operator Right(Either<Left, Right> v)
        {
            return v.RightValue;
        }

        public Either(Right right)
        {
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            _right = right;
            Side = Status.Right;
        }

        public Either(Left left)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            _left = left;
            Side = Status.Left;
        }

        public Right RightValue
        {
            get
            {
                if (Side == Status.Left)
                    throw new InvalidOperationException();
                return _right;
            }
            set
            {
                Side = Status.Right;
                _right = value;
            }
        }

        public Left LeftValue
        {
            get
            {
                if (Side == Status.Right)
                    throw new InvalidOperationException();
                return _left;
            }
            set
            {
                Side = Status.Left;
                _left = value;
            }
        }

        public override int GetHashCode()
        {
            switch (Side)
            {
                case Status.Right:
                    return _right.GetHashCode();
                case Status.Left:
                    return _left.GetHashCode();
            }

            return 0xDEADBEE;
        }

        public override string ToString()
        {
            if (Side == Status.Right)
                return _right.ToString();
            return _left.ToString();
        }

        public class EqualityComparer : IEqualityComparer<Either<Left, Right>>
        {
            public bool Equals(Either<Left, Right> x, Either<Left, Right> y)
            {
                return x == y;
            }

            public int GetHashCode(Either<Left, Right> obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
