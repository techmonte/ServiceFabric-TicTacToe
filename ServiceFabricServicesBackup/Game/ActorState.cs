using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class ActorState
    {
        [DataMember]
        public int[] Board { get; set; }
        [DataMember]
        public string Winner { get; set; }
        [DataMember]
        public List<Tuple<long, string>> Players { get; set; }
        [DataMember]
        public int NextPlayerIndex { get; set; }
        [DataMember]
        public int NumberOfMoves { get; set; }

    }
}
