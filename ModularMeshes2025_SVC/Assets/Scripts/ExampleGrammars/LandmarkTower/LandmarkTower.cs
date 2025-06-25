using UnityEngine;

namespace Demo
{
    // Very simple grammar that stacks prefabs to create a landmark skyscraper.
    // The tower consists of an optional base, a repeating middle section and a top element.
    public class LandmarkTower : Shape
    {
        public GameObject basePrefab;
        public GameObject middlePrefab;
        public GameObject topPrefab;
        public int middleLevels = 20;

        protected override void Execute()
        {
            float y = 0f;
            if (basePrefab != null)
            {
                SpawnPrefab(basePrefab, new Vector3(0, y, 0));
                y += 1f;
            }

            for (int i = 0; i < middleLevels; i++)
            {
                if (middlePrefab != null)
                {
                    SpawnPrefab(middlePrefab, new Vector3(0, y, 0));
                }
                y += 1f;
            }

            if (topPrefab != null)
            {
                SpawnPrefab(topPrefab, new Vector3(0, y, 0));
            }
        }
    }
}
