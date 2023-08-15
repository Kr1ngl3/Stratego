using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.Models
{
    class AnimationEventArgs : EventArgs
    {
        public enum Type
        {
            Vanish,
            Appear,
            Attack,
            Move
        }

        public AnimationEventArgs(Type type, Vector2? target = null)
        {
            switch (type)
            {
                case Type.Vanish:
                    IsVanishing = true;
                    break;
                case Type.Appear:
                    IsAppearing = true;
                    break;
                case Type.Attack:
                    Target = target;
                    IsAttacking = true;
                    break;
                case Type.Move:
                    Target = target;
                    break;

            }
        }

        public bool IsAttacking { get; init; }
        public bool IsVanishing { get; init; }
        public bool IsAppearing { get; init; }
        public Vector2? Target { get; init; }
        public int AnimationTime => Target is null ? 550 : 1050;
    }
}
