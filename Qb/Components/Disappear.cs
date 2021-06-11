using UnityEngine;

namespace Chen.Qb.Components
{
    internal class Disappear : MonoBehaviour
    {
        private readonly float life = 240f;
        private float age = 0f;

        private void FixedUpdate()
        {
            if (age >= life) Destroy(gameObject);
            age += Time.fixedDeltaTime;
        }
    }
}