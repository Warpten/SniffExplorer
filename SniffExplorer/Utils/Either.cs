using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SniffExplorer.Enums;

namespace SniffExplorer.Utils
{
    public sealed class Either<Left, Right>
    {
        public enum Status
        {
            Right,
            Left,
        }

        private Status _status;
        private Left _left;
        private Right _right;

        public Either()
        {
        }

        public Either(Right right)
        {
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            _right = right;
            _status = Status.Right;
        }

        public Either(Left left)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            _left = left;
            _status = Status.Left;
        }

        [Pure]
        public Right RightValue
        {
            get
            {
                if (_status == Status.Left)
                    throw new InvalidOperationException();
                return _right;
            }
            set
            {
                _status = Status.Right;
                _right = value;
            }
        }

        [Pure]
        public Left LeftValue
        {
            get
            {
                if (_status == Status.Right)
                    throw new InvalidOperationException();
                return _left;
            }
            set
            {
                _status = Status.Left;
                _left = value;
            }
        }

        [Pure]
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
            if (_status == Status.Right)
                return _right.ToString();
            return _left.ToString();
        }
    }
}
