using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
  [DataContract]
  public class User
  {
    [DataMember]
    public string username {  get; set; }

    [DataMember]
    public string password { get; set; }

    [DataMember]
    public string firstName { get; set; }

    [DataMember]
    public string lastName { get; set; }

  }
}
