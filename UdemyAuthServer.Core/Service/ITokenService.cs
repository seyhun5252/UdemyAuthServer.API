using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdemyAuthServer.Core.Configuraiton;
using UdemyAuthServer.Core.DTOs;
using UdemyAuthServer.Core.Model;

namespace UdemyAuthServer.Core.Service
{
    //Token oluşturmak için kullanılır.
    public interface ITokenService
    {
        TokenDto CreateToken(UserApp userApp);
        ClientTokenDto CreateTokenByClient(Client client);
    }
}
