using System;
using System.Collections.Generic;

namespace Map
{
    public class Board
    {
        public Block[,] blocks;

        public Board(int width, int height)
        {
            blocks = new Block[width, height];
            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                for (int j = 0; j < blocks.GetLength(1); j++)
                {
                    blocks[i, j] = new Block(i, j);
                }
            }
        }

        public void SetBlock(int x, int y, bool wall)
        {
            blocks[x, y].wall = wall;
        }

        public void CheckClear()
        {
            foreach (Block block in blocks)
            {
                block.Clear();
            }
        }

        public bool Exists(Block block)
        {
            return Exists(block.x, block.y);
        }

        public bool Exists(int x, int y)
        {
            foreach (Block block in blocks)
            {
                if (block.x == x && block.y == y)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 주변 블록 가져오기
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public List<Block> GetAroundBlocks(Block target)
        {
            //[-1,-1] [ 0,-1] [ 1,-1]
            //[-1, 0]         [ 1, 0]
            //[-1, 1] [ 0, 1] [ 1, 1]

            List<Block> arounds = new List<Block>();
            if (Exists(target.x - 1, target.y - 1))
            {
                Block block = blocks[target.x - 1, target.y - 1];
                arounds.Add(block);
            }
            if (Exists(target.x, target.y - 1))
            {
                Block block = blocks[target.x, target.y - 1];
                arounds.Add(block);
            }
            if (Exists(target.x + 1, target.y - 1))
            {
                Block block = blocks[target.x + 1, target.y - 1];
                arounds.Add(block);
            }

            if (Exists(target.x - 1, target.y))
            {
                Block block = blocks[target.x - 1, target.y];
                arounds.Add(block);
            }
            if (Exists(target.x + 1, target.y))
            {
                Block block = blocks[target.x + 1, target.y];
                arounds.Add(block);
            }

            if (Exists(target.x - 1, target.y + 1))
            {
                Block block = blocks[target.x - 1, target.y + 1];
                arounds.Add(block);
            }
            if (Exists(target.x, target.y + 1))
            {
                Block block = blocks[target.x, target.y + 1];
                arounds.Add(block);
            }
            if (Exists(target.x + 1, target.y + 1))
            {
                Block block = blocks[target.x + 1, target.y + 1];
                arounds.Add(block);
            }

            // 대각선 블록인 경우 정방향블록이 벽이면 제외한다.
            for (int i = arounds.Count - 1; i >= 0 ; i--)
            {
                Block block = arounds[i];
                bool isDiagonalBlock = Math.Abs(block.x - target.x) == 1 && Math.Abs(block.y - target.y) == 1;
                if (isDiagonalBlock)
                {
                    // 가로 블록 벽인지 확인
                    Block blockX = arounds.Find(b => b.x == block.x && b.y == target.y);
                    if (blockX.wall)
                        arounds.Remove(block);

                    // 세로 블록 벽인지 확인
                    Block blockY = arounds.Find(b => b.x == target.x && b.y == block.y);
                    if (blockY.wall)
                        arounds.Remove(block);
                }
            }

            // 벽 블록 제거
            arounds.RemoveAll(b => b.wall);

            return arounds;
        }
    }
}
