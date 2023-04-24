using UnityEngine;

namespace XIV
{
    public static class XIVDebug
    {
        public static void DrawBox(Vector3 pos, Vector3 halfExtend, float duration = 0)
        {
            var bottomLeft = pos - halfExtend;
            var topRight = pos + halfExtend;
            Debug.DrawLine(bottomLeft, topRight, Color.black, duration);
                    
            Debug.DrawLine(bottomLeft, new Vector3(topRight.x, bottomLeft.y), Color.red, duration);
            Debug.DrawLine(new Vector3(topRight.x, bottomLeft.y), topRight, Color.green, duration);
            Debug.DrawLine(topRight, new Vector3(bottomLeft.x, topRight.y), Color.red, duration);
            Debug.DrawLine(new Vector3(bottomLeft.x, topRight.y), bottomLeft, Color.green, duration);
        }
    }
}