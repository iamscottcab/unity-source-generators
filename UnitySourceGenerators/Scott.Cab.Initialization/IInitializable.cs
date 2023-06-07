using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    public interface IInitializable
    {
        Task Initialize();
    }
}
