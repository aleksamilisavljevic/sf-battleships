using System.Collections.ObjectModel;
using System.Fabric.Description;

namespace API
{
  public class IMetricsList : KeyedCollection<string, ServiceLoadMetricDescription>
  {
    protected override string GetKeyForItem(ServiceLoadMetricDescription item)
    {
      return item.Name;
    }
  }
}
