
using System.Net;

namespace StreamingUrlFetcherRestServer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            string ipAddress;
            using (WebClient wc = new())
            {
                ipAddress = wc.DownloadString("http://icanhazip.com/");
            }

            if (!string.IsNullOrEmpty(ipAddress))
            {
                Console.WriteLine(ipAddress);
            }

            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://*:444");

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
