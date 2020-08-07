using System.Threading.Tasks;

namespace Sample
{
	public interface RPCCallback
	{
		Task<string> getByCity(int city, string start, string end);
	}
}