using System;

using DidasUtils.Numerics;

namespace MCH.Render
{
    public interface IUpdatable
    {
        void Update(Vector2i offset);
    }
}
