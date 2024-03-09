using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

[DataContract]
internal class RequestData
{
    [DataMember(Name = "title")]
    public string Title { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
    [DataMember(Name = "button")]
    public int Button { get; set; }
    [DataMember(Name = "icon")]
    public int Icon { get; set; }
}
