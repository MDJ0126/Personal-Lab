namespace Map
{
    public class Block
    {
        /// <summary>
        /// 좌표
        /// </summary>
        public int x, y;

        /// <summary>
        /// 닫힌 블럭인지 여부
        /// </summary>
        public bool wall;

        //===========================================================================================================
        // 아래의 변수와 메소드는 계산때만 활용. (상속이나 다른 처리가 필요할 듯.)
        //===========================================================================================================

        /// <summary>
        /// 블럭 위치에 따른 가격 (거리 가중치)
        /// </summary>
        public int F => G + H;

        /// <summary>
        /// 시작점과의 거리
        /// </summary>
        public int G { get; private set; } = 0;

        /// <summary>
        /// 도착점과의 거리
        /// </summary>
        public int H { get; private set; } = 0;

        /// <summary>
        /// 검사 여부
        /// </summary>
        public bool check = false;

        /// <summary>
        /// 이전 블럭
        /// </summary>
        public Block prev = null;

        /// <summary>
        /// 다음 블럭
        /// </summary>
        public Block next = null;

        public Block(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 가격 세팅
        /// </summary>
        /// <param name="g"></param>
        /// <param name="h"></param>
        public void SetPrice(int g, int h)
        {
            this.G = g;
            this.H = h;
        }

        public void Clear()
        {
            check = false;
            G = 0;
            H = 0;
            prev = null;
            next = null;
        }
    }
}
