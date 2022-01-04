using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map.Sample
{
    public class MapCreater : MonoBehaviour
    {
        [SerializeField]
        private BlockItem block;
        public float interval = 10;
        public Vector2 size;
        public BlockItem[,] blocks;
        public Board board;

        private void Awake()
        {
            Update();
        }

        public void Update()
        {
            Clear();
            board = new Board((int)size.x, (int)size.y);

            blocks = new BlockItem[(int)size.x, (int)size.y];

            int width = (int)((size.x - 1) * interval);
            int height = (int)((size.y - 1) * interval);

            for (int i = 0; i < blocks.GetLength(1); i++)
            {
                for (int j = 0; j < blocks.GetLength(0); j++)
                {
                    var go = Instantiate(block, this.transform);
                    go.transform.localPosition = new Vector3(j * interval - width * 0.5f, i * (-interval) + height * 0.5f, 0f);
                    go.transform.localScale = block.transform.localScale;
                    go.gameObject.SetActive(true);
                    go.GetComponent<BlockItem>().SetBlockInfo(board.blocks[j, i]);
                    blocks[j, i] = go;
                }
            }

            block.gameObject.SetActive(false);
            enabled = false;
        }

        private void Clear()
        {
            for (int i = this.transform.childCount - 1; 0 <= i; i--)
            {
                var ts = this.transform.GetChild(i);
                if (!ts.gameObject.Equals(block))
                {
                    if (Application.isPlaying)
                        Destroy(ts.gameObject);
                    else
                        DestroyImmediate(ts.gameObject);
                }
            }
        }

        public BlockItem GetBlockItem(Vector3 vector3)
        {
            foreach (var item in blocks)
            {
                if ((int)item.transform.localPosition.x == (int)vector3.x && (int)item.transform.localPosition.y == (int)vector3.y)
                    return item;
            }
            return null;
        }
    }
}