using UnityEngine;
using System.Collections.Generic;

namespace ProceduralPoints {
    public static class PerlinNoise {
        public static float ProceduralPoints(int xCoord, int yCoord, float seed) {
            float point = new float();
            point = Mathf.PerlinNoise(xCoord + seed, yCoord + seed);
            return point;
        }
    }
}