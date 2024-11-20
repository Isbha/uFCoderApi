using System.Threading.Tasks;
using global::SamsoWebhost.Saflok.Models;
namespace uFCoderApi.Repository.Interface
{


    public interface ISaflok
    {

        Task<dynamic> CreateKey(KeyCardRequest cardOperation, string Username, string password, string url);

        Task<dynamic> AdditionalKey(KeyCardRequest cardOperation, string Username, string password, string url);
    }
}

