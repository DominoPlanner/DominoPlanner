using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public struct ProtocolTransfer
    {
        List<List<Tuple<int, int>>>[] _dominoes;
        public List<List<Tuple<int, int>>>[] dominoes
        {
            get
            {
                return _dominoes;
            }
            set
            {
                _dominoes = value;

            }
        } // twodimensional array of lists of tuples ^^
        public Orientation orientation { get; set; }
        public int cellcount
        {
            get
            {
                return dominoes.Min(row => row.Sum(block => block.Count));
            }
        }

        public ColorRepository colors { get; set; }

        public int[] counts { get; set; }

        public int columns;
        public int rows;

    }
}
