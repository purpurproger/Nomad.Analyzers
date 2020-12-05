using System.Threading.Tasks;

namespace NomadAnalyzers
{
  public class Example
  {
    public void Method()
    {
      var task = Task.Run(() => { }); //Should be TaskUtils.RunSuppressFlow(() => { });

      task.ContinueWith(t => { }); //Should be task.ContinueWithSuppressFlow(() => { });

      task = new Task(() => { }); //Should be TaskUtils.CreateSuppressFlow(() => { });
    }
  }
}