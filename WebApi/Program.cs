using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //终结点配置
                    //ASPNETCORE_URLS 环境变量。
                    //--urls 命令行参数。
                    //urls 主机配置键。
                    //UseUrls 扩展方法。
                    //webBuilder.UseUrls("https://localhost:6001,http://localhost:6000");

                    //kestrel 详细配置说明
                    //https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0
                    //webBuilder.UseKestrel((wbContext, opts) =>
                    //{
                    //    //opts.ConfigureEndpointDefaults(opts => { opts.UseConnectionLogging(); });
                    //    //opts.Listen(IPAddress.Any, 3000);
                    //    //opts.Listen(IPAddress.Any, 3000, config =>
                    //    //{
                    //    //    config.UseConnectionLogging();
                    //    //    //    config.KestrelServerOptions.ConfigureHttpsDefaults(congifOpts =>
                    //    //    //    {
                    //    //    //        congifOpts.AllowAnyClientCertificate();
                    //    //    //        congifOpts.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.AllowCertificate;
                    //    //    //        congifOpts.CheckCertificateRevocation = true;
                    //    //    //        congifOpts.ClientCertificateValidation = new Func<System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool>((cer, chain, policyErrs) =>
                    //    //    //        {
                    //    //    //            return true;
                    //    //    //        });
                    //    //    //        congifOpts.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls;
                    //    //    //    });
                    //    //});
                    //});
                    webBuilder.UseStartup<Startup>().UseWebRoot("wwwroot");
                });
    }
}
