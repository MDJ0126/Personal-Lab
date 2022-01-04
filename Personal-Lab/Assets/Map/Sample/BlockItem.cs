using System;
using System.Collections;
using UnityEngine;

namespace Map.Sample
{
    public class BlockItem : MonoBehaviour
    {
        public Block block;
        public bool isWall = false;
        private Coroutine _clickEffect = null;

        public void SetBlockInfo(Block block)
        {
            this.block = block;
        }

        public void SetWall()
        {
            isWall = !isWall;
            block.wall = isWall;
            if (_clickEffect != null)
                StopCoroutine(_clickEffect);
            _clickEffect = StartCoroutine(PlayColorEffect(() =>
            {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material.color = isWall ? Color.blue : Color.white;
            }));
        }

        private IEnumerator PlayColorEffect(Action onFinished)
        {
            const float INTERVAL = 0.1f;
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(INTERVAL);
            meshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(INTERVAL);
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(INTERVAL);
            meshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(INTERVAL);
            onFinished?.Invoke();
        }
    }
}
