using UnityEngine;

namespace MapGen
{
    interface IIslandShape
    {
        bool IsInside(Vector2 point);
    }
}
