using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SniffExplorer.Enums;

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
            var hashCode = 0xDEADBEEF;
            if (_left != null)
                hashCode ^= (uint)_left.GetHashCode();
            if (_right != null)
                hashCode ^= (uint)_right.GetHashCode();

            return (int)hashCode;
        }

        public override string ToString()
        {
            if (Side == Status.Right)
                return _right.ToString();
            return _left.ToString();
        }
    }
}
