using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.library.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdemyAuthServer.Core;
using UdemyAuthServer.Core.Configuraiton;
using UdemyAuthServer.Core.DTOs;
using UdemyAuthServer.Core.Model;
using UdemyAuthServer.Core.Repository;
using UdemyAuthServer.Core.Service;

namespace UdemyAuthServer.Service.Service
{
    public class AuthenticationService : IAuthhenticationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenService;

        public AuthenticationService(IOptions<List<Client>> clients, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenService)
        {
            _clients = clients.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenService = userRefreshTokenService;
        }

        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto == null) new ArgumentNullException(nameof(loginDto));

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Response<TokenDto>.Fail("Email veya password yanlış", 400, true);

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Response<TokenDto>.Fail("Email veya password yanlış", 400, true);
            }
            var token = _tokenService.CreateToken(user);

            var userRefreshToken = await _userRefreshTokenService.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken == null)
            {
                await _userRefreshTokenService.AddAsync(new UserRefreshToken
                {
                    UserId = user.Id,
                    Code = token.RefreshToken,
                    Expiration = token.RefreshTokenExpiration
                });
            }
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.ClientId == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);

            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", 404, true);

            }
            var token = _tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);

        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenService.
                Where(x => x.Code == refreshToken)
                .SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail("Refresh Token not found", 404, true);
            }
            else
            {
                var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);
                if (user == null)
                {
                    return Response<TokenDto>.Fail("User Id not found", 404, true);

                }

                var tokenDto = _tokenService.CreateToken(user);

                existRefreshToken.Code = tokenDto.RefreshToken;
                existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;

                await _unitOfWork.CommitAsync();

                return Response<TokenDto>.Success(tokenDto, 200);

            }

        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefeshToken = await _userRefreshTokenService
                .Where(x => x.Code == refreshToken)
                .SingleOrDefaultAsync();

            if (existRefeshToken == null)
            {
                return Response<NoDataDto>.Fail("Refresh Token not found", 404, true);
            }

            _userRefreshTokenService.Remove(existRefeshToken);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);



        }
    }
}
