using System;
using System.Collections.Generic;

namespace Map.PathFinding
{
    public static class AStar
    {
        /// <summary>
        /// 길찾기
        /// </summary>
        /// <param name="board">맵</param>
        /// <param name="start">시작 좌표</param>
        /// <param name="dest">도착 좌표</param>
        /// <returns>시작점 반환</returns>
        public static Block PathFinding(this Board board, Block start, Block dest)
        {
            if (board.Exists(start) && board.Exists(dest))
            {
                board.CheckClear();

                List<Block> waittingBlocks = new List<Block>();
                List<Block> finishedBlocks = new List<Block>();

                Block current = start;

                while (current != null)
                {
                    // 주변 블럭 가져오기
                    var aroundBlocks = board.GetAroundBlocks(current);

                    for (int i = 0; i < aroundBlocks.Count; i++)
                    {
                        var block = aroundBlocks[i];
                        if (!waittingBlocks.Equals(block) && !block.check)
                            waittingBlocks.Add(block);
                    }

                    // 검사 완료 리스트로 이관
                    current.check = true;
                    if (waittingBlocks.Remove(current))
                        finishedBlocks.Add(current);

                    // 이동 불가 시, 길찾기 실패 처리
                    if (aroundBlocks.Count == 0)
                        return null;
                    else
                    {
                        // 부모 세팅
                        aroundBlocks = aroundBlocks.FindAll(block => !block.check);
                    }

                    // 주변 블럭 가격 계산
                    CalcRating(aroundBlocks, start, current, dest);

                    // 다음 검사 블록 가져오기
                    current = GetNextBlock(aroundBlocks, current);
                    if (current == null)
                    {
                        // 다음 블록 탐색 실패 시, 처음부터 재시작
                        current = GetPriorityBlock(waittingBlocks);

                        // 더이상 검사할 곳이 없다면(길을 찾지 못 한 경우 해당),
                        // 길찾기 종료 및 도착점과 가장 가까운 막힌 곳으로 안내.
                        if (current == null)
                        {
                            Block exceptionBlock = null;
                            for (int i = 0; i < finishedBlocks.Count; i++)
                            {
                                if (exceptionBlock == null || exceptionBlock.H > finishedBlocks[i].H)
                                    exceptionBlock = finishedBlocks[i];
                            }
                            current = exceptionBlock;
                            break;
                        }
                    }
                    else if (current.Equals(dest))
                    {
                        break;
                    }
                }

                // 역으로 길을 만들어준다.
                while (!current.Equals(start))
                {
                    current.prev.next = current;
                    current = current.prev;
                }

                start.prev = null;
                return start;
            }
            return null;
        }

        /// <summary>
        /// 검사 대기 블럭 중에서 가격이 가장 낮은 블럭을 가져오기
        /// </summary>
        /// <param name="waittingBlocks"></param>
        /// <returns></returns>
        private static Block GetPriorityBlock(List<Block> waittingBlocks)
        {
            // 블럭 위치에 따른 가격이 제일 낮은 블럭을 반환다.
            Block block = null;
            var enumerator = waittingBlocks.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (block == null || block.F < current.F)
                {
                    block = current;
                }
            }

            return block;
        }

        /// <summary>
        /// 다음 이동 블록 가져오기
        /// </summary>
        /// <param name="arounds"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private static Block GetNextBlock(List<Block> arounds, Block current)
        {
            Block minValueBlock = null;
            for (int i = 0; i < arounds.Count; i++)
            {
                Block next = arounds[i];
                if (!next.check)
                {
                    // 다음 경로 이동을 해야하니, 시작점으로부터의 가격이 더 높은 블록을 탐색한다.
                    if (minValueBlock == null)
                    {
                        minValueBlock = next;
                    }
                    else if (minValueBlock.H > next.H)
                    {
                        minValueBlock = next;
                    }
                }
            }
            return minValueBlock;
        }

        /// <summary>
        /// 주변 블록 가격 계산하기
        /// </summary>
        /// <param name="arounds">주변 블록 리스트</param>
        /// <param name="start">시작 블록</param>
        /// <param name="current">현재 위치 블록</param>
        /// <param name="dest">도착 블록</param>
        private static void CalcRating(List<Block> arounds, Block start, Block current, Block dest)
        {
            if (arounds != null)
            {
                for (int i = 0; i < arounds.Count; i++)
                {
                    var block = arounds[i];
                    bool isDiagonalBlock = Math.Abs(block.x - current.x) == 1 && Math.Abs(block.y - current.y) == 1;
                    int priceFromDest = (Math.Abs(dest.x - block.x) + Math.Abs(dest.y - block.y)) * 10;
                    if (block.prev == null)
                        block.prev = current;
                    block.SetPrice(current.G + (isDiagonalBlock ? 14 : 10), priceFromDest);
                }
            }
        }
    }
}