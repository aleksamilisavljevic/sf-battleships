using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
  [DataContract]
  public class BotState
  {

    [DataMember]
    public BattleshipGrid guessGrid {  get; set; }

    [DataMember]
    public long hunting {  get; set; }

    [DataMember]
    public long[] moveOrder { get; set; }

    [DataMember]
    public long lastRow {  get; set; }

    [DataMember]
    public long lastCol { get; set; }

    [DataMember]
    public int curStep { get; set; }

  }
}
