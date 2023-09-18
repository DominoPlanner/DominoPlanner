namespace DominoPlanner.Usage
{
    public class BlockData : ModelBase
    {
        public BlockData(int blockSize, bool useBlock)
        {
            BlockSize = blockSize;
            UseBlock = useBlock;
        }

        private int _BlockSize;
        public int BlockSize
        {
            get
            {
                return _BlockSize;
            }
            set
            {
                if(_BlockSize != value)
                {
                    _BlockSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _UseBlock;
        public bool UseBlock
        {
            get
            {
                return _UseBlock;
            }
            set
            {
                if(_UseBlock != value)
                {
                    _UseBlock = value;
                    RaisePropertyChanged();
                }
            }
        }

        public BlockData Clone()
        {
            return new BlockData(BlockSize, UseBlock);
        }
    }
}
