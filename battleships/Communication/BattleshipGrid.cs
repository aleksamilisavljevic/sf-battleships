using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
  [DataContract]
  public class BattleshipGrid
  {
    [DataMember]
    public long[][] grid { get; set; }
  }
}
