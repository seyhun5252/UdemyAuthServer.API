using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.library.Dto
{
    //Bu da verileri getirmek ya da hata döndürmek için
    public class Response<T> where T : class
    {
        public T Data { get; private set; }
        public int StatusCode { get; private set; }

        [JsonIgnore] // bu bize jsona döndürğünde yok sayılacak
        public bool IsSuccessful { get; private set; }
        public ErrorDto Error { get; private set; }

        // Add için data vermye gerek var 
        public static Response<T> Success(T data, int statusCode)
        {
            return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessful = true };
        }

        //Delete ve update için data dönmeye gerek o yüzden data default değer verdik
        public static Response<T> Success(int statusCode)
        {
            return new Response<T> { Data = default, StatusCode = statusCode, IsSuccessful = true };
        }

        // Bu failse liste halinde error dönebiilir
        public static Response<T> Fail(ErrorDto errorDto, int statusCode)
        {
            return new Response<T> { Error = errorDto, StatusCode = statusCode, IsSuccessful = false };
        }
        // Bu failde tek bir error dönebiilir
        public static Response<T> Fail(string errorMessage, int statusCode, bool isShow)
        {
            var errorDto = new ErrorDto(errorMessage, isShow);
            return new Response<T> { Error = errorDto, StatusCode = statusCode, IsSuccessful = false };
        }
    }
}
