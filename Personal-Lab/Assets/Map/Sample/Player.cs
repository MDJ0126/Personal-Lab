using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map.Sample
{
    public class Player : MonoBehaviour
    {
        private Coroutine _moveCo = null;

        private Vector2 _dest;
        private float _progress;

        private void Update()
        {
            this.transform.position = Vector2.Lerp(this.transform.position, _dest, _progress * 2f);
            if (_progress < 1f)
            {
                _progress += Time.deltaTime;
                if (_progress >= 1f)
                    _progress = 1f;
            }
        }

        public void Move(Block block)
        {
            if (_moveCo != null)
                StopCoroutine(_moveCo);
            _moveCo = StartCoroutine(MoveStep(block));
        }

        private IEnumerator MoveStep(Block block)
        {
            while (block.next != null)
            {
                _progress = 0f;
                block = block.next;
                var item = Map.Instance.GetBlockItem(block);
                _dest = item.transform.position;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
