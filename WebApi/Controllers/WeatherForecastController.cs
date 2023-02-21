using AutoMapper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetCoreTemp.WebApi.QuartzJobScheduler;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using NetCoreTemp.WebApi.Services;
using NPOI.XWPF.UserModel;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System.Xml.Linq;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using GemBox.Document;
using GemBox.Pdf;
using PdfDocument = GemBox.Pdf.PdfDocument;
using Spire.Pdf;

namespace NetCoreTemp.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly RedisHelp.RedisHelper _redisHelper;

        private readonly MyMapper<User, UserDto> _myMapper;
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly RoleService _roleService;
        UserService _userService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            RedisHelp.RedisHelper redisHelper,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _redisHelper = redisHelper;
            var mapper = serviceProvider.GetService<IMapper>();
            if (mapper != null)
                _mapper = mapper;

            var myMapper = serviceProvider.GetService<MyMapper<User, UserDto>>();
            if (myMapper != null)
                _myMapper = myMapper;

            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            _roleService = serviceProvider.GetRequiredService<RoleService>();
            _userService = serviceProvider.GetRequiredService<UserService>();

            ////测试 订阅key过期事件
            //_redisHelper.SubscribeKeyExpire();

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("开始测试");

            #region Fody-MethodDecorator&PropertyChanged测试

            await _userService.getUserRoles("0");
            //Services.SampleA sample = new Services.SampleA
            //{
            //    A = "A",
            //    AA = "AA",
            //    B = 1,
            //    BB = 2,
            //    C = 1.123m,
            //    CC = 1.223m,
            //    D = true,
            //    DD = false
            //};
            //sample.PropertyChanged += (obj, args) =>
            //{
            //    _logger.LogInformation("sample.PropertyChanged:{0}", args.PropertyName);
            //};
            //sample.A += "_";
            //sample.B += 10;
            //sample.C += 10.999m;
            //sample.D = false;
            //sample.MethodA();
            //sample.MethodTime("TTTTTime");

            //Services.SampleB sampleB = new Services.SampleB
            //{
            //    A = "B",
            //    AA = "BB",
            //    B = 2,
            //    BB = 22,
            //    C = 2.123m,
            //    CC = 2.223m,
            //    D = true,
            //    DD = false
            //};
            //sampleB.PropertyChanged += (obj, args) =>
            //{
            //    _logger.LogInformation("sample.PropertyChanged:{0}", args.PropertyName);
            //};
            //sampleB.A += "_";
            //sampleB.B += 20;
            //sampleB.C += 20.999m;
            //sampleB.D = false;
            //sampleB.MethodB();
            //sampleB.MethodTimeB("BBBTime");

            #endregion

            var rng = new Random();
            var Arr = await Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList());

            #region autoMapper测试

            //dynamic foo = new ExpandoObject();
            //foo.Type = "TestType111";
            //foo.Name = "TestName111";
            //foo.Sort = 1;
            //foo.Remark = "Test_Remark111";
            //foo.CreateUserId = "CreateUserId111";
            //foo.CreateUserName = "CreateUserName111";
            //var exp = new ExpandoObject();
            //var x = exp as IDictionary<string, Object>;
            //x.Add("Name", "Menu_Name");
            //foo.ArrMenu = new List<ExpandoObject> { exp };

            //var ss = new Dictionary<string, Object>
            //{
            //    {"Type", "TestType222" },
            //    {"Name", "TestName222"},
            //    {"Sort",  2},
            //    {"Remark",  "Test_Remark222"},
            //    { "CreateUserId", "CreateUserId222" },
            //    { "CreateUserName", "CreateUserName222"},
            //    { "ArrMenu",new List<Dictionary<string, object>>{new Dictionary<string, object>{ { "Name", "Menu_Name222" } } } }
            //};
            //var vb = new Role
            //{
            //    //Type = "TestType111",
            //    Name = "TestName111",
            //    Sort = 1,
            //    Remark = "Test_Remark111"
            //};
            //var dt0 = vb.ToDto();
            //var aa = _mapper.Map(ss, vb);
            //var bb = _mapper.Map(foo, vb);

            //UserDto userDto = new UserDto
            //{
            //    _ID = Guid.NewGuid().ToString(),
            //    _Name = "Test",
            //    _Roles = new List<string> { "A", "B", "C" }
            //};
            //var user = _myMapper.ToSource(userDto);

            #endregion

            #region SearchQuery测试

            //var filters = new List<filterRule> {
            //    new filterRule{  field="Name", value = "aaa"},
            //    new filterRule{  field="CreateUserId", value = "aaa"},
            //    new filterRule{  field="CreateUserName", value = "bbb"},
            //    new filterRule{  field="CreateDate", value = "bbb"},
            //    new filterRule{  field="_CreateDate", value = "2021-12-09"},
            //    new filterRule{  field="CreateDate_", value = "2021-12-09"},
            //};
            ////var roleQuery = new Services.RoleQuery();
            //var qf = _roleService.QueryByFilterRules(filters);

            #endregion

            var user = new User
            {
                Name = "asdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasd",
                Roles = "1,2,3",
                Email = "aqqqqq.com",
                CreateUserId = Guid.Empty,
                ModifyUserId = Guid.Empty
            };

            //var tf = await TryUpdateModelAsync<User>(user);

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var uiculture = System.Globalization.CultureInfo.CurrentUICulture;
            ModelState.Clear();
            TryValidateModel(user);
            var s = ModelState.IsValid;
            if (!s)
            {
                var errs = ModelState.Select(x => x.Key + ":" + string.Join(" . ", x.Value?.Errors.Select((n, i) => $" {i + 1}. {n.ErrorMessage}").ToArray()));
                Arr.Add(new WeatherForecast
                {
                    Date = DateTime.Now,
                    Summary = string.Join(";", errs),
                    TemperatureC = 1
                });
            }
            _dbContext.User.ToList();

            return Ok(Arr);
        }

        public class Foo
        {
            public int Bar { get; set; }
            public int Baz { get; set; }
            public Foo InnerFoo { get; set; }
        }

        [HttpGet("GetHtmlByWord_NPOI")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHtmlByWord_NPOI()
        {
            string htmlStr;
            using (Stream stream = new FileStream(@"C:\Users\Michael Wang\Desktop\testDoc2Html.docx", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //1.Npoi方式
                NPOI.Word2Html.NpoiDoc npoiDoc = new NPOI.Word2Html.NpoiDoc();
                var html = await npoiDoc.NpoiDocx(stream);
                htmlStr = $"<html><header></header><body>{html}</body><html>";
            }
            return Content(htmlStr, "text/html", System.Text.Encoding.UTF8);
        }

        [HttpGet("GetHtmlByWord_OpenXml")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHtmlByWord_OpenXml()
        {
            string htmlStr;
            using (Stream stream = new FileStream(@"C:\Users\Michael Wang\Desktop\2HtmlRemark.docx", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //2.OpenXmlSkd
                byte[] byteArray = new byte[(int)stream.Length];
                await stream.ReadAsync(byteArray, 0, (int)stream.Length);
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        int imageCounter = 0;
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                            PageTitle = "My Page Title",
                            ImageHandler = imageInfo =>
                            {
                                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Bitmap));
                                var imgBase64 = Convert.ToBase64String((byte[])converter.ConvertTo(imageInfo.Bitmap, typeof(byte[])));
                                XElement imgXElemt;
                                //imgXElemt = new XElement("image", imgBase64);

                                imgXElemt = new XElement(Xhtml.img,
                                        new XAttribute(NoNamespace.src, $"data:image/png;base64, {imgBase64}"),
                                        imageInfo.ImgStyleAttribute,
                                        imageInfo.AltText != null ?
                                            new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                                return imgXElemt;
                            }
                             
                            //ImageHandler = imageInfo =>
                            //{
                            //    DirectoryInfo localDirInfo = new DirectoryInfo("img");
                            //    if (!localDirInfo.Exists)
                            //        localDirInfo.Create();
                            //    ++imageCounter;
                            //    string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                            //    ImageFormat imageFormat = null;
                            //    if (extension == "png")
                            //    {
                            //        extension = "gif";
                            //        imageFormat = ImageFormat.Gif;
                            //    }
                            //    else if (extension == "gif")
                            //        imageFormat = ImageFormat.Gif;
                            //    else if (extension == "bmp")
                            //        imageFormat = ImageFormat.Bmp;
                            //    else if (extension == "jpeg")
                            //        imageFormat = ImageFormat.Jpeg;
                            //    else if (extension == "tiff")
                            //    {
                            //        extension = "gif";
                            //        imageFormat = ImageFormat.Gif;
                            //    }
                            //    else if (extension == "x-wmf")
                            //    {
                            //        extension = "wmf";
                            //        imageFormat = ImageFormat.Wmf;
                            //    }
                            //    if (imageFormat == null)
                            //        return null;

                            //    string imageFileName = "img/image" +
                            //        imageCounter.ToString() + "." + extension;
                            //    try
                            //    {
                            //        imageInfo.Bitmap.Save(imageFileName, imageFormat);
                            //    }
                            //    catch (System.Runtime.InteropServices.ExternalException)
                            //    {
                            //        return null;
                            //    }
                            //    XElement img = new XElement(Xhtml.img,
                            //        new XAttribute(NoNamespace.src, imageFileName),
                            //        imageInfo.ImgStyleAttribute,
                            //        imageInfo.AltText != null ?
                            //            new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            //    return img;
                            //}
                        };

                        XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                        XNamespace w = "http://www.w3.org/1999/xhtml";
                        // Note: the XHTML returned by ConvertToHtmlTransform contains objects of type
                        // XEntity. PtOpenXmlUtil.cs defines the XEntity class. See
                        // http://blogs.msdn.com/ericwhite/archive/2010/01/21/writing-entity-references-using-linq-to-xml.aspx
                        // for detailed explanation.
                        //
                        // If you further transform the XML tree returned by ConvertToHtmlTransform, you
                        // must do it correctly, or entities do not serialize properly.
                        //var bodyContainer = html.Element(w + "body");
                        htmlStr = html.ToStringNewLineOnAttributes();
                    }
                }
            }

            return Content(htmlStr, "text/html", System.Text.Encoding.UTF8);
        }

        [HttpGet("GetHtmlByWord_GemBox")]
        [AllowAnonymous]
        public IActionResult GetHtmlByWord_GemBox()
        {
            string htmlStr;
            MemoryStream ms = new MemoryStream();
            using (Stream stream = new FileStream(@"C:\Users\Michael Wang\Desktop\2HtmlRemark.docx", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // If using Professional version, put your serial key below.
                GemBox.Document.ComponentInfo.SetLicense("D02V-DFLU-FDK7-XJKA");

                // Load Word file (DOC, DOCX, RTF, XML) into DocumentModel object.
                var document = DocumentModel.Load(stream, GemBox.Document.LoadOptions.DocxDefault);

                var saveOptions = new HtmlSaveOptions()
                {
                    HtmlType = HtmlType.Html,
                    EmbedImages = true,
                    UseSemanticElements = true,
                    Encoding = System.Text.Encoding.UTF8
                };

                // Save DocumentModel object to HTML (or MHTML) file.
                document.Save(ms, saveOptions);
                htmlStr = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
            return Content(htmlStr, "text/html", System.Text.Encoding.UTF8);

        }

        [HttpGet("GetPdfByHtml_GemBox")]
        [AllowAnonymous]
        public IActionResult GetPdfByHtml_GemBox()
        {
            MemoryStream ms = new MemoryStream();
            using (Stream stream = new FileStream(@"C:\Users\Michael Wang\Desktop\testDoc2Html.docx", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // If using Professional version, put your serial key below.
                GemBox.Document.ComponentInfo.SetLicense("D02V-DFLU-FDK7-XJKA");

                var bytes = System.Text.Encoding.UTF8.GetBytes($@"<!DOCTYPE html>
                <html>
                  <body>
                    {pdfhtml}
                  </body>
                </html>");
                // Load input HTML file.
                DocumentModel document = DocumentModel.Load(new MemoryStream(bytes, false), GemBox.Document.LoadOptions.HtmlDefault);

                // When reading any HTML content a single Section element is created.
                // We can use that Section element to specify various page options.
                Section section = document.Sections[0];
                PageSetup pageSetup = section.PageSetup;
                PageMargins pageMargins = pageSetup.PageMargins;
                pageMargins.Top = pageMargins.Bottom = pageMargins.Left = pageMargins.Right = 0;

                // Save output PDF file.
                document.Save(ms, GemBox.Document.SaveOptions.PdfDefault);
            }
            return File(ms, "application/pdf", "html2pdfByGemBox.pdf");

        }

        [HttpGet("GetHtmlByPdf_GemBox")]
        [AllowAnonymous]
        public  IActionResult GetHtmlByPdf_GemBox()
        {
            string htmlStr;
            MemoryStream ms = new MemoryStream();
            using (Stream stream = new FileStream(@"C:\Users\Michael Wang\Desktop\html2pdfByGemBox.pdf", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // If using Professional version, put your serial key below.
                GemBox.Document.ComponentInfo.SetLicense("D02V-DFLU-FDK7-XJKA");

                // Load Word file (DOC, DOCX, RTF, XML) into DocumentModel object.
                var document = DocumentModel.Load(stream, GemBox.Document.LoadOptions.PdfDefault);

                var saveOptions = new HtmlSaveOptions()
                {
                    HtmlType = HtmlType.Html,
                    EmbedImages = true,
                    UseSemanticElements = true,
                    Encoding = System.Text.Encoding.UTF8
                };

                // Save DocumentModel object to HTML (or MHTML) file.
                document.Save(ms, saveOptions);
                htmlStr = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
            return Content(htmlStr, "text/html", System.Text.Encoding.UTF8);
        }

        [HttpGet("GetHtmlByPdf_Spire")]
        [AllowAnonymous]
        public IActionResult GetHtmlByPdf_Spire()
        {
            string htmlStr;
            MemoryStream ms = new MemoryStream();
            //Create a pdf document.
            Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
            // Load the PDF Document
            doc.LoadFromFile(@"C:\Users\Michael Wang\Desktop\html2pdfByGemBox.pdf");
            doc.SaveToStream(ms, FileFormat.HTML);
            htmlStr = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            return Content(htmlStr, "text/html", System.Text.Encoding.UTF8);
        }

        string pdfhtml = @"<div id=""cookie-banner""><div id=""wcpConsentBannerCtrl"" class=""_23tra1HsiiP6cT-Cka-ycB"" dir=""ltr"" role=""alert"">
        <div class=""_1Upc2NjY8AlDn177YoVj0y"">
            <span class=""_1V_hlU-7jdtPiooHMu89BB w8hcgFksdo30C8w-bygqu"">
        <svg xmlns=""http://www.w3.org/2000/svg"" x=""0px"" y=""0px"" viewBox=""0 0 44 44"" width=""24px"" height=""24px"" fill=""none"" stroke=""currentColor"">
          <circle cx=""22"" cy=""22"" r=""20"" stroke-width=""2""></circle>
          <line x1=""22"" x2=""22"" y1=""18"" y2=""33"" stroke-width=""3""></line>
          <line x1=""22"" x2=""22"" y1=""12"" y2=""15"" stroke-width=""3""></line>
        </svg>
        </span> <!--  used for icon  -->
            <p class=""f6QKJD7fhSbnJLarTL-W- ydkKdaztSS0AeHWIeIHsQ w8hcgFksdo30C8w-bygqu"">
                We use optional cookies to improve your experience on our websites, such as through social media connections, and to display personalized advertising based on your online activity. If you reject optional cookies, only cookies necessary to provide you the services will be used. You may change your selection by clicking “Manage Cookies” at the bottom of the page. <a target=""_blank"" href=""https://go.microsoft.com/fwlink/?LinkId=521839"">Privacy Statement</a> <a target=""_blank"" href=""https://aka.ms/3rdpartycookies"">Third-Party Cookies</a>
            </p>
        </div>

        <div class=""_2j0fmugLb1FgYz6KPuB91w"">
            <button type=""button"" class=""_1XuCi2WhiqeWRUVp3pnFG3 erL690_8JwUW-R4bJRcfl"">Accept</button>
            <button type=""button"" class=""_1XuCi2WhiqeWRUVp3pnFG3 erL690_8JwUW-R4bJRcfl"">Reject</button>
            <button type=""button"" class=""_1XuCi2WhiqeWRUVp3pnFG3 erL690_8JwUW-R4bJRcfl"">Manage cookies</button>
        </div>
        </div></div>
<nav class=""navbar navbar-inverse"" role=""navigation"">
    <div class=""container"">
        <div class=""row"">
            <div class=""col-sm-12 text-center"">
                <a href=""#"" id=""skipToContent"" class=""showOnFocus"" title=""Skip To Content"">Skip To Content</a>
            </div>
        </div>
        <div class=""row"">
            <div class=""col-sm-12"">
                <div class=""navbar-header"">
                    <button type=""button"" class=""navbar-toggle collapsed"" data-toggle=""collapse"" data-target=""#navbar"" aria-expanded=""false"" aria-controls=""navbar"">
                        <span class=""sr-only"">Toggle navigation</span>
                        <span class=""icon-bar""></span>
                        <span class=""icon-bar""></span>
                        <span class=""icon-bar""></span>
                    </button>
                    <a href=""/"">
                        <img class=""navbar-logo"" width=""94"" height=""29"" alt=""NuGet home"" src=""/Content/gallery/img/logo-header.svg"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/logo-header-94x29.png'; this.onerror = null;"">
                    </a>
                </div>
                <div id=""navbar"" class=""navbar-collapse collapse"">
                    <ul class=""nav navbar-nav"" role=""tablist"">
                            <li class=""active"" role=""presentation"">
        <a role=""tab"" aria-selected=""true"" href=""/packages"">
            <span>Packages</span>
        </a>
    </li>

                            <li class="""" role=""presentation"">
        <a role=""tab"" aria-selected=""false"" href=""/packages/manage/upload"">
            <span>Upload</span>
        </a>
    </li>

    <li class="""" role=""presentation"">
        <a role=""tab"" aria-selected=""false"" href=""/stats"">
            <span>Statistics</span>
        </a>
    </li>
                                                    <li class="""" role=""presentation"">
        <a role=""tab"" aria-selected=""false"" href=""https://docs.microsoft.com/en-us/nuget/"">
            <span>Documentation</span>
        </a>
    </li>

                            <li class="""" role=""presentation"">
        <a role=""tab"" aria-selected=""false"" href=""/downloads"">
            <span>Downloads</span>
        </a>
    </li>

                            <li class="""" role=""presentation"">
        <a role=""tab"" aria-selected=""false"" href=""https://blog.nuget.org/"">
            <span>Blog</span>
        </a>
    </li>

                    </ul>
                        <ul class=""nav navbar-nav navbar-right"" role=""tablist"">
    <li class="""" role=""presentation"">
        <a role=""tab"" aria-selected=""false"" href=""/users/account/LogOn?returnUrl=%2Fpackages%2FSpire.PDF%2F"" title=""Sign in to an existing NuGet.org account"">
            <span>Sign in</span>
        </a>
    </li>
                        </ul>
                </div>
            </div>
        </div>
    </div>

        <div class=""container search-container"">
            <div class=""row"">
                <form aria-label=""Package search bar"" class=""col-sm-12"" action=""/packages"" method=""get"">
                    <div class=""input-group"">
    <input name=""q"" type=""text"" class=""form-control"" id=""search"" aria-label=""Enter packages to search"" placeholder=""Search for packages..."" autocomplete=""off"" value="""">
    <span class=""input-group-btn"">
        <button class=""btn btn-default btn-warning btn-search"" type=""submit"" title=""Search for packages"" aria-label=""Search"">
            <span class=""ms-Icon ms-Icon--Search"" aria-hidden=""true""></span>
        </button>
    </span>
</div>
                    <div id=""autocomplete-results-container"" class=""text-left"" tabindex=""0""></div>

<script type=""text/html"" id=""autocomplete-results-row"">
    <!-- ko if: $data -->
    <!-- ko if: $data.PackageRegistration -->
    <div class=""col-sm-4 autocomplete-row-id autocomplete-row-data"">
        <span data-bind=""attr: { id: 'autocomplete-result-id-' + $data.PackageRegistration.Id, title: $data.PackageRegistration.Id }, text: $data.PackageRegistration.Id""></span>
    </div>
    <div class=""col-sm-4 autocomplete-row-downloadcount text-right autocomplete-row-data"">
        <span data-bind=""text: $data.DownloadCount + ' downloads'""></span>
    </div>
    <div class=""col-sm-4 autocomplete-row-owners text-left autocomplete-row-data"">
        <span data-bind=""text: $data.OwnersString + ' '""></span>
    </div>
    <!-- /ko -->
    <!-- ko ifnot: $data.PackageRegistration -->
    <div class=""col-sm-12 autocomplete-row-id autocomplete-row-data"">
        <span data-bind=""attr: { id: 'autocomplete-result-id-' + $data, title: $data  }, text: $data""></span>
    </div>
    <!-- /ko -->
    <!-- /ko -->
</script>
<script type=""text/html"" id=""autocomplete-results-template"">
    <!-- ko if: $data.data.length > 0 -->
    <div data-bind=""foreach: $data.data"" id=""autocomplete-results-list"">
        <a data-bind=""attr: { id: 'autocomplete-result-row-' + $data, href: '/packages/' + $data, title: $data }"" tabindex=""-1"">
            <div data-bind=""attr:{ id: 'autocomplete-container-' + $data }"" class=""autocomplete-results-row"">
            </div>
        </a>
    </div>
    <!-- /ko -->
</script>

                </form>
            </div>
        </div>
</nav>
    <div id=""skippedToContent"">
<section role=""main"" class=""container main-container page-package-details"">
    <div class=""row"">
        <div class=""col-sm-9 package-details-main"">
            <div class=""package-header"">
                <div class=""package-title"">
                    <h1>
                        <span class=""pull-left"">
                            <img class=""package-icon img-responsive"" aria-hidden=""true"" alt="""" src=""https://api.nuget.org/v3-flatcontainer/spire.pdf/8.3.9/icon"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/default-package-icon-256x256.png'; this.onerror = null;"">
                        </span>
                        <span class=""title"" tabindex=""0"">
                            Spire.<wbr>PDF
                        </span>
                        <span class=""version-title"" tabindex=""0"">
                            8.3.9
                        </span>
                    </h1>
                </div>

<div class=""framework framework-badges"">
        <!-- .NET cannot be an empty version since the lowest version for this framework is ""net5.0"", if the package contains just ""net"" framework it will fall into .NET Framework badge instead.' -->
        <span class=""framework-badge-asset"" role=""button"" tabindex=""0"" data-content=""This package is compatible with this framework or higher."" data-original-title="""" title="""">.NET 6.0</span>
                <span class=""framework-badge-asset"" role=""button"" tabindex=""0"" data-content=""This package is compatible with this framework or higher."" data-original-title="""" title="""">.NET Core 2.0</span>
                    <span class=""framework-badge-asset"" role=""button"" tabindex=""0"" data-content=""This package is compatible with this framework or higher."" data-original-title="""" title="""">.NET Framework 2.0</span>
</div>
                    <div class=""install-tabs"">
                        <ul class=""nav nav-tabs"" role=""tablist"">

    <li role=""presentation"" class=""active"">
        <a href=""#package-manager"" aria-expanded=""true"" id=""package-manager-tab"" class=""package-manager-tab"" aria-selected=""true"" tabindex=""0"" aria-controls=""package-manager"" role=""tab"" data-toggle=""tab"" title=""Switch to tab panel which contains package installation command for Package Manager"">
            Package Manager
        </a>
    </li>
    <li role=""presentation"" class="""">
        <a href=""#dotnet-cli"" aria-expanded=""false"" id=""dotnet-cli-tab"" class=""package-manager-tab"" aria-selected=""false"" tabindex=""-1"" aria-controls=""dotnet-cli"" role=""tab"" data-toggle=""tab"" title=""Switch to tab panel which contains package installation command for .NET CLI"">
            .NET CLI
        </a>
    </li>
    <li role=""presentation"" class="""">
        <a href=""#package-reference"" aria-expanded=""false"" id=""package-reference-tab"" class=""package-manager-tab"" aria-selected=""false"" tabindex=""-1"" aria-controls=""package-reference"" role=""tab"" data-toggle=""tab"" title=""Switch to tab panel which contains package installation command for PackageReference"">
            PackageReference
        </a>
    </li>
    <li role=""presentation"" class="""">
        <a href=""#paket-cli"" aria-expanded=""false"" id=""paket-cli-tab"" class=""package-manager-tab"" aria-selected=""false"" tabindex=""-1"" aria-controls=""paket-cli"" role=""tab"" data-toggle=""tab"" title=""Switch to tab panel which contains package installation command for Paket CLI"">
            Paket CLI
        </a>
    </li>
    <li role=""presentation"" class="""">
        <a href=""#script-interactive"" aria-expanded=""false"" id=""script-interactive-tab"" class=""package-manager-tab"" aria-selected=""false"" tabindex=""-1"" aria-controls=""script-interactive"" role=""tab"" data-toggle=""tab"" title=""Switch to tab panel which contains package installation command for Script &amp; Interactive"">
            Script &amp; Interactive
        </a>
    </li>
    <li role=""presentation"" class="""">
        <a href=""#cake"" aria-expanded=""false"" id=""cake-tab"" class=""package-manager-tab"" aria-selected=""false"" tabindex=""-1"" aria-controls=""cake"" role=""tab"" data-toggle=""tab"" title=""Switch to tab panel which contains package installation command for Cake"">
            Cake
        </a>
    </li>
                        </ul>
                        <div class=""tab-content"">

    <div role=""tabpanel"" class=""tab-pane active"" id=""package-manager"">
        <div class=""install-script-row"">

            
            <pre class=""install-script"" id=""package-manager-text""><span class=""install-command-row"">Install-Package Spire.PDF -Version 8.3.9</span></pre>
            <div class=""copy-button"">
                <button id=""package-manager-button"" class=""btn btn-default btn-warning"" type=""button"" data-toggle=""popover"" data-placement=""bottom"" data-content=""Copied."" aria-label=""Copy the Package Manager command"" role=""button"" data-original-title="""" title="""">
                    <span class=""ms-Icon ms-Icon--Copy"" aria-hidden=""true""></span>
                </button>
            </div>
        </div>

    </div>
    <div role=""tabpanel"" class=""tab-pane "" id=""dotnet-cli"">
        <div class=""install-script-row"">

            
            <pre class=""install-script"" id=""dotnet-cli-text""><span class=""install-command-row"">dotnet add package Spire.PDF --version 8.3.9</span></pre>
            <div class=""copy-button"">
                <button id=""dotnet-cli-button"" class=""btn btn-default btn-warning"" type=""button"" data-toggle=""popover"" data-placement=""bottom"" data-content=""Copied."" aria-label=""Copy the .NET CLI command"" role=""button"" data-original-title="""" title="""">
                    <span class=""ms-Icon ms-Icon--Copy"" aria-hidden=""true""></span>
                </button>
            </div>
        </div>

    </div>
    <div role=""tabpanel"" class=""tab-pane "" id=""package-reference"">
        <div class=""install-script-row"">

            
            <pre class=""install-script"" id=""package-reference-text""><span class=""install-command-row"">&lt;PackageReference Include=""Spire.PDF"" Version=""8.3.9"" /&gt;</span></pre>
            <div class=""copy-button"">
                <button id=""package-reference-button"" class=""btn btn-default btn-warning"" type=""button"" data-toggle=""popover"" data-placement=""bottom"" data-content=""Copied."" aria-label=""Copy the PackageReference XML node"" role=""button"" data-original-title="""" title="""">
                    <span class=""ms-Icon ms-Icon--Copy"" aria-hidden=""true""></span>
                </button>
            </div>
        </div>

    <div class=""icon-text alert alert-info"">
        <i class=""ms-Icon ms-Icon--Info"" aria-hidden=""true""></i>
        
For projects that support <a href=""https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files"">PackageReference</a>, copy this XML node into the project file to reference the package.
                    
    </div>
    </div>
    <div role=""tabpanel"" class=""tab-pane "" id=""paket-cli"">
        <div class=""install-script-row"">

            
            <pre class=""install-script"" id=""paket-cli-text""><span class=""install-command-row"">paket add Spire.PDF --version 8.3.9</span></pre>
            <div class=""copy-button"">
                <button id=""paket-cli-button"" class=""btn btn-default btn-warning"" type=""button"" data-toggle=""popover"" data-placement=""bottom"" data-content=""Copied."" aria-label=""Copy the Paket CLI command"" role=""button"" data-original-title="""" title="""">
                    <span class=""ms-Icon ms-Icon--Copy"" aria-hidden=""true""></span>
                </button>
            </div>
        </div>

    <div class=""icon-text alert alert-warning"">
        <i class=""ms-Icon ms-Icon--Warning"" aria-hidden=""true""></i>
        
The NuGet Team does not provide support for this client. Please contact its <a href=""https://fsprojects.github.io/Paket/contact.html"" aria-label=""Contact the maintainers of the Paket CLI client"">maintainers</a> for support.
                    
    </div>
    </div>
    <div role=""tabpanel"" class=""tab-pane "" id=""script-interactive"">
        <div class=""install-script-row"">

            
            <pre class=""install-script"" id=""script-interactive-text""><span class=""install-command-row"">#r ""nuget: Spire.PDF, 8.3.9""</span></pre>
            <div class=""copy-button"">
                <button id=""script-interactive-button"" class=""btn btn-default btn-warning"" type=""button"" data-toggle=""popover"" data-placement=""bottom"" data-content=""Copied."" aria-label=""Copy the Script &amp; Interactive command"" role=""button"" data-original-title="""" title="""">
                    <span class=""ms-Icon ms-Icon--Copy"" aria-hidden=""true""></span>
                </button>
            </div>
        </div>

    <div class=""icon-text alert alert-info"">
        <i class=""ms-Icon ms-Icon--Info"" aria-hidden=""true""></i>
        
#r directive can be used in F# Interactive, C# scripting and .NET Interactive. Copy this into the interactive tool or source code of the script to reference the package.
                    
    </div>
    </div>
    <div role=""tabpanel"" class=""tab-pane "" id=""cake"">
        <div class=""install-script-row"">

            
            <pre class=""install-script"" id=""cake-text""><span class=""install-command-row"">// Install Spire.PDF as a Cake Addin
#addin nuget:?package=Spire.PDF&amp;version=8.3.9

// Install Spire.PDF as a Cake Tool
#tool nuget:?package=Spire.PDF&amp;version=8.3.9</span></pre>
            <div class=""copy-button"">
                <button id=""cake-button"" class=""btn btn-default btn-warning"" type=""button"" data-toggle=""popover"" data-placement=""bottom"" data-content=""Copied."" aria-label=""Copy the Cake command"" role=""button"" data-original-title="""" title="""">
                    <span class=""ms-Icon ms-Icon--Copy"" aria-hidden=""true""></span>
                </button>
            </div>
        </div>

    <div class=""icon-text alert alert-warning"">
        <i class=""ms-Icon ms-Icon--Warning"" aria-hidden=""true""></i>
        
The NuGet Team does not provide support for this client. Please contact its <a href=""https://cakebuild.net/support/nuget"" aria-label=""Contact the maintainers of the Cake client"">maintainers</a> for support.
                    
    </div>
    </div>
                        </div>
                    </div>
            </div>

            <div class=""body-tabs"">
                <ul class=""nav nav-tabs"" role=""tablist"">

                        <li role=""presentation"" class=""active"" id=""show-readme-container"">
                            <a href=""#readme-tab"" role=""tab"" data-toggle=""tab"" id=""readme-body-tab"" class=""body-tab"" aria-controls=""readme-tab"" aria-expanded=""true"" aria-selected=""true"" tabindex=""0"">
                                <i class=""ms-Icon ms-Icon--Dictionary"" aria-hidden=""true""></i>
                                README
                            </a>
                        </li>
                        <li role=""presentation"" id=""show-supportedframeworks-container"">
                            <a href=""#supportedframeworks-tab"" role=""tab"" data-toggle=""tab"" id=""supportedframeworks-body-tab"" class=""body-tab"" aria-controls=""supportedframeworks-tab"" aria-expanded=""false"" aria-selected=""false"" tabindex=""-1"">
                                <i class=""ms-Icon ms-Icon--Package"" aria-hidden=""true""></i>
                                Frameworks
                            </a>
                        </li>
                        <li role=""presentation"">
                            <a href=""#dependencies-tab"" role=""tab"" data-toggle=""tab"" id=""dependencies-body-tab"" class=""body-tab"" aria-controls=""dependencies-tab"" aria-expanded=""false"" aria-selected=""false"" tabindex=""-1"">
                                <i class=""ms-Icon ms-Icon--Packages"" aria-hidden=""true""></i>
                                Dependencies
                            </a>
                        </li>

                        <li role=""presentation"">
                            <a href=""#usedby-tab"" role=""tab"" data-toggle=""tab"" id=""usedby-body-tab"" class=""body-tab"" aria-controls=""usedby-tab"" aria-expanded=""false"" aria-selected=""false"" tabindex=""-1"">
                                <i class=""ms-Icon ms-Icon--BranchFork2"" aria-hidden=""true""></i>
                                Used By
                            </a>
                        </li>


                    <li role=""presentation"">
                        <a href=""#versions-tab"" role=""tab"" data-toggle=""tab"" id=""versions-body-tab"" class=""body-tab"" aria-controls=""versions-tab"" aria-expanded=""false"" aria-selected=""false"" tabindex=""-1"">
                            <i class=""ms-Icon ms-Icon--Stopwatch"" aria-hidden=""true""></i>
                            Versions
                        </a>
                    </li>

                        <li role=""presentation"">
                            <a href=""#releasenotes-tab"" role=""tab"" data-toggle=""tab"" id=""release-body-tab"" class=""body-tab"" aria-controls=""releasenotes-tab"" aria-expanded=""false"" aria-selected=""false"" tabindex=""-1"">
                                <i class=""ms-Icon ms-Icon--ReadingMode"" aria-hidden=""true""></i>
                                Release Notes
                            </a>
                        </li>
                </ul>
            </div>

            <div class=""tab-content body-tab-content"">
                    <div role=""tabpanel"" class=""tab-pane active"" id=""readme-tab"" aria-label=""Readme tab content"">
                            <div class=""readme-common"">
                                <div id=""readme-container"">


                                    <h3>.NET Library for Processing &amp; Manipulating PDF Files</h3>
<hr>
<p><a href=""https://www.e-iceblue.com/Introduce/pdf-for-net-introduce.html"" rel=""noopener noreferrer nofollow"">Product Page</a> 丨 <a href=""https://www.e-iceblue.com/Tutorials/Spire.PDF.html"" rel=""noopener noreferrer nofollow"">Documentation</a> 丨 <a href=""https://github.com/eiceblue/Spire.PDF-for-.NET"" rel=""noopener noreferrer nofollow"">Examples</a> 丨 <a href=""https://www.e-iceblue.com/forum/spire-pdf-f7.html"" rel=""noopener noreferrer nofollow"">Forum</a> 丨 <a href=""https://www.e-iceblue.com/TemLicense.html"" rel=""noopener noreferrer nofollow"">Temporary License</a> 丨 <a href=""https://www.e-iceblue.com/Misc/customized-demo.html"" rel=""noopener noreferrer nofollow"">Customized Demo</a></p>
<p><a href=""https://www.e-iceblue.com/Introduce/pdf-for-net-introduce.html"" rel=""noopener noreferrer nofollow"">Spire.PDF for .NET</a> is a professional PDF API applied to creating, writing, editing, handling and reading PDF files without any external dependencies within .NET ( C#, VB.NET, ASP.NET, .NET Core, .NET 5.0, MonoAndroid and Xamarin.iOS ) application.</p>
<p>Using this .NET PDF library, you can implement rich capabilities to <a href=""https://www.e-iceblue.com/Tutorials/Spire.PDF/Spire.PDF-Program-Guide/How-to-create-PDF-dynamically-and-send-it-to-client-browser-using-ASP.NET.html"" rel=""noopener noreferrer nofollow"">create PDF files</a> from scratch or process existing PDF documents entirely through C#/VB.NET without installing Adobe Acrobat.</p>
<h4>PDF Processing Features</h4>
<ul>
<li>Generate/write/read/edit PDF documents.</li>
<li>Supports 14 core, Type 1, True Type, Type 3, CJK &amp; Unicode fonts.</li>
<li>Extract images, text, pages and attachments from a PDF document with great speed and accuracy.</li>
<li>Merge/split PDF documents and overlay documents.</li>
<li>Convert HTML, XPS, Text and images to PDF and convert PDF to Excel,Word,images with efficient performance.</li>
<li>Encrypt/Decrypt PDF, modify PDF passwords and create PDF digital signatures.</li>
<li>Add and modify text/image bookmarks.</li>
<li>Add text in Footer/Header.</li>
<li>Export database table and pictures to PDF.</li>
<li>Add Hyperlinks, work with actions/Javascript Action/Action Chain/Action Annotation.</li>
<li>Add/remove/edit/fill fields.</li>
</ul>
<h4>Conversions</h4>
<ul>
<li>Convert Webpage HTML, HTML ASPX to PDF</li>
<li>Convert Image(Jpeg, Jpg, Png, Bmp, Tiff, Gif, EMF, Ico) to PDF</li>
<li>Convert Text to PDF</li>
<li>Convert PDF to HTML</li>
<li>Convert XPS to PDF</li>
<li>Convert PDF to SVG</li>
<li>Convert PDF to XPS</li>
<li>Convert PDF to Image</li>
<li>Convert PDF to Word</li>
</ul>
<h4>Support Environment</h4>
<ul>
<li>Fully written in C# and also support VB.NET.</li>
<li>Applied on .NET Framework 2.0, 3.5, 3.5 Client Profile, 4.0, 4.0 Client Profile,4.5 and .NET Standard 2.0, .NET Core, .NET 5.0, MonoAndroid and Xamarin.Ios.</li>
<li>Support Windows Forms and ASP.NET Applications.</li>
<li>Support 32-bit OS</li>
<li>Support 64-bit OS</li>
<li>Support PDF Version 1.2, 1.3, 1.4, 1.5, 1.6 and 1.7.</li>
<li>PDF API reference in HTML.</li>
<li>Be Independent and do not need Adobe Acrobat or other third party PDF libraries.</li>
</ul>
<h4>Convert PDF to DOC in C#</h4>
<pre><code class=""language-c#"">            //Create a PDF document and load sample PDF.
            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(""test.pdf"");

            //Use SaveToFile method and set conversion target parameter as FileFormat.DOC.
            doc.SaveToFile(""PDFtoDoc.doc"", FileFormat.DOC);
</code></pre>
<h4>Convert PDF to images in C#</h4>
<pre><code class=""language-C#"">            //Create a PDF document and load sample PDF.
            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(""sample.pdf"");

            Image bmp = doc.SaveAsImage(0);
            Image emf = doc.SaveAsImage(0, Spire.Pdf.Graphics.PdfImageType.Metafile);
            Image zoomImg = new Bitmap((int)(emf.Size.Width * 2), (int)(emf.Size.Height * 2));
            using (Graphics g = Graphics.FromImage(zoomImg))
            {
                g.ScaleTransform(2.0f, 2.0f);
                g.DrawImage(emf, new Rectangle(new Point(0, 0), emf.Size), new Rectangle(new Point(0, 0), emf.Size), GraphicsUnit.Pixel);
            }
            //Save as BMP
            bmp.Save(""convertToBmp.bmp"", ImageFormat.Bmp);
            System.Diagnostics.Process.Start(""convertToBmp.bmp"");

            //Save as EMF
            emf.Save(""convertToEmf.png"", ImageFormat.Png);
            System.Diagnostics.Process.Start(""convertToEmf.png"");

            //SAVE as ZoomImg
            zoomImg.Save(""convertToZoom.png"", ImageFormat.Png);
            System.Diagnostics.Process.Start(""convertToZoom.png"");
</code></pre>
<h4>Convert HTML to PDF in C#</h4>
<pre><code class=""language-C#"">            //Create a pdf document.
            PdfDocument doc = new PdfDocument();

            PdfPageSettings setting = new PdfPageSettings();

            setting.Size = new SizeF(1000,1000);
            setting.Margins = new Spire.Pdf.Graphics.PdfMargins(20);

            PdfHtmlLayoutFormat htmlLayoutFormat = new PdfHtmlLayoutFormat();
            htmlLayoutFormat.IsWaiting = true;
            
            String url = ""https://www.wikipedia.org/"";
         
            Thread thread = new Thread(() =&gt;
            { doc.LoadFromHTML(url, false, false, false, setting,htmlLayoutFormat); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            //Save pdf file.
            doc.SaveToFile(""output-wiki.pdf"");
</code></pre>
<p><a href=""https://www.e-iceblue.com/Introduce/pdf-for-net-introduce.html"" rel=""noopener noreferrer nofollow"">Product Page</a> 丨 <a href=""https://www.e-iceblue.com/Tutorials/Spire.PDF.html"" rel=""noopener noreferrer nofollow"">Documentation</a> 丨 <a href=""https://github.com/eiceblue/Spire.PDF-for-.NET"" rel=""noopener noreferrer nofollow"">Examples</a> 丨 <a href=""https://www.e-iceblue.com/forum/spire-pdf-f7.html"" rel=""noopener noreferrer nofollow"">Forum</a> 丨 <a href=""https://www.e-iceblue.com/TemLicense.html"" rel=""noopener noreferrer nofollow"">Temporary License</a> 丨 <a href=""https://www.e-iceblue.com/Misc/customized-demo.html"" rel=""noopener noreferrer nofollow"">Customized Demo</a></p>
                                </div>
                            </div>
                    </div>
                    <div role=""tabpanel"" class=""tab-pane "" id=""supportedframeworks-tab"" aria-label=""Supported frameworks tab content"">
<table class=""framework framework-table"" aria-label=""Supported frameworks"">
    <thead>
        <tr>
            <th scope=""col"" class=""framework-table-title""><b>Product</b></th>
            <th scope=""col"" class=""framework-table-title""><b>Versions</b></th>
        </tr>
    </thead>
    <tbody>
                <tr>
                    <td class=""framework-table-product"" tabindex=""0"">
                        .NET
                    </td>

                    <td class=""framework-table-frameworks"">
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net5.0</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net5.0-windows</span>
                                <span class=""framework-badge-asset framework-table-margin"" tabindex=""0"" data-original-title="""" title="""">net6.0</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net6.0-android</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net6.0-ios</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net6.0-maccatalyst</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net6.0-macos</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net6.0-tvos</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net6.0-windows</span>
                    </td>
                </tr>
                <tr>
                    <td class=""framework-table-product"" tabindex=""0"">
                        .NET Core
                    </td>

                    <td class=""framework-table-frameworks"">
                                <span class=""framework-badge-asset framework-table-margin"" tabindex=""0"" data-original-title="""" title="""">netcoreapp2.0</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">netcoreapp2.1</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">netcoreapp2.2</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">netcoreapp3.0</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">netcoreapp3.1</span>
                    </td>
                </tr>
                <tr>
                    <td class=""framework-table-product"" tabindex=""0"">
                        .NET Framework
                    </td>

                    <td class=""framework-table-frameworks"">
                                <span class=""framework-badge-asset framework-table-margin"" tabindex=""0"" data-original-title="""" title="""">net20</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net35</span>
                                <span class=""framework-badge-asset framework-table-margin"" tabindex=""0"" data-original-title="""" title="""">net40</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net403</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net45</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net451</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net452</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net46</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net461</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net462</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net463</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net47</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net471</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net472</span>
                                <span class=""framework-badge-computed framework-table-margin"" tabindex=""0"">net48</span>
                    </td>
                </tr>
                <tr>
                    <td class=""framework-table-product"" tabindex=""0"">
                        MonoAndroid
                    </td>

                    <td class=""framework-table-frameworks"">
                                <span class=""framework-badge-asset framework-table-margin"" tabindex=""0"" data-original-title="""" title="""">monoandroid</span>
                    </td>
                </tr>
                <tr>
                    <td class=""framework-table-product"" tabindex=""0"">
                        Xamarin.iOS
                    </td>

                    <td class=""framework-table-frameworks"">
                                <span class=""framework-badge-asset framework-table-margin"" tabindex=""0"" data-original-title="""" title="""">xamarinios</span>
                    </td>
                </tr>
    </tbody>
</table>
<div>
    <div>
        <i class=""ms-Icon ms-Icon--SquareShape frameworktableinfo-asset-icon""></i>
        <span class=""frameworktableinfo-text"">Compatible target framework(s)</span>
    </div>
    <div>
        <i class=""ms-Icon ms-Icon--SquareShape frameworktableinfo-computed-icon framework-badge-computed""></i>
        <span class=""frameworktableinfo-text"">Additional computed target framework(s)</span>
    </div>
    <span class=""frameworktableinfo-text""><i>Learn more about <a href=""https://docs.microsoft.com/dotnet/standard/frameworks"" aria-label=""Learn more about Target Frameworks"">Target Frameworks</a> and <a href=""https://docs.microsoft.com/dotnet/standard/net-standard"" aria-label=""Learn more about .NET Standard"">.NET Standard</a>.</i></span>
</div>                    </div>
                    <div role=""tabpanel"" class=""tab-pane "" id=""dependencies-tab"" aria-label=""Dependencies tab content"">
                                    <ul class=""list-unstyled dependency-groups"" id=""dependency-groups"">
                                            <li>
                                                    <h4><span>.NETCoreApp 2.0</span></h4>
                                                <ul class=""list-unstyled dependency-group"">
                                                        <li>
                                                                <a href=""/packages/System.Drawing.Common/"">System.Drawing.Common</a>
                                                                <span>(&gt;= 4.5.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Text.Encoding.CodePages/"">System.Text.Encoding.CodePages</a>
                                                                <span>(&gt;= 4.5.0)</span>
                                                        </li>
                                                </ul>
                                            </li>
                                            <li>
                                                    <h4><span>.NETFramework 2.0</span></h4>
                                                <ul class=""list-unstyled dependency-group"">
                                                        <li>
                                                                No dependencies.
                                                        </li>
                                                </ul>
                                            </li>
                                            <li>
                                                    <h4><span>.NETFramework 4.0</span></h4>
                                                <ul class=""list-unstyled dependency-group"">
                                                        <li>
                                                                No dependencies.
                                                        </li>
                                                </ul>
                                            </li>
                                            <li>
                                                    <h4><span>MonoAndroid 0.0</span></h4>
                                                <ul class=""list-unstyled dependency-group"">
                                                        <li>
                                                                <a href=""/packages/SkiaSharp/"">SkiaSharp</a>
                                                                <span>(&gt;= 1.68.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Buffers/"">System.Buffers</a>
                                                                <span>(&gt;= 4.5.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Memory/"">System.Memory</a>
                                                                <span>(&gt;= 4.5.3)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Runtime.CompilerServices.Unsafe/"">System.Runtime.CompilerServices.Unsafe</a>
                                                                <span>(&gt;= 4.6.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Text.Encoding.CodePages/"">System.Text.Encoding.CodePages</a>
                                                                <span>(&gt;= 4.6.0)</span>
                                                        </li>
                                                </ul>
                                            </li>
                                            <li>
                                                    <h4><span>net6.0</span></h4>
                                                <ul class=""list-unstyled dependency-group"">
                                                        <li>
                                                                <a href=""/packages/System.Drawing.Common/"">System.Drawing.Common</a>
                                                                <span>(&gt;= 6.0.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Text.Encoding.CodePages/"">System.Text.Encoding.CodePages</a>
                                                                <span>(&gt;= 6.0.0)</span>
                                                        </li>
                                                </ul>
                                            </li>
                                            <li>
                                                    <h4><span>Xamarin.iOS 0.0</span></h4>
                                                <ul class=""list-unstyled dependency-group"">
                                                        <li>
                                                                <a href=""/packages/SkiaSharp/"">SkiaSharp</a>
                                                                <span>(&gt;= 1.68.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Buffers/"">System.Buffers</a>
                                                                <span>(&gt;= 4.5.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Memory/"">System.Memory</a>
                                                                <span>(&gt;= 4.5.3)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Runtime.CompilerServices.Unsafe/"">System.Runtime.CompilerServices.Unsafe</a>
                                                                <span>(&gt;= 4.6.0)</span>
                                                        </li>
                                                        <li>
                                                                <a href=""/packages/System.Text.Encoding.CodePages/"">System.Text.Encoding.CodePages</a>
                                                                <span>(&gt;= 4.6.0)</span>
                                                        </li>
                                                </ul>
                                            </li>
                                    </ul>
                    </div>
                <div role=""tabpanel"" class=""tab-pane "" id=""usedby-tab"" aria-label=""Used by tab content"">
                        <div class=""used-by"" id=""used-by"">
                                    <h3>
                                        <strong>NuGet packages </strong> (3)
                                    </h3>
                                    <p>
                                        Showing the top 3 NuGet packages that depend on Spire.PDF:
                                    </p>
                                    <table class=""table borderless"" aria-label=""Packages that depend on Spire.PDF"">
                                        <thead>
                                            <tr>
                                                <th class=""used-by-adjust-table-head"" scope=""col"" role=""columnheader"" tabindex=""0"">Package</th>
                                                <th class=""used-by-adjust-table-head"" scope=""col"" role=""columnheader"" tabindex=""0"">Downloads</th>
                                            </tr>
                                        </thead>
                                        <tbody class=""no-border"">
                                                <tr>
                                                    <td class=""used-by-desc-column"" tabindex=""0"">
                                                        <a class=""text-left ngp-link"" href=""/packages/James.Testing.Pdf/"">
                                                            James.Testing.Pdf
                                                        </a>
                                                        <p class=""used-by-desc"">A library of helpers for the purpose of testing pdf documents/content.  It is named after the author who wrote the book of James in the Bible.  (James 1:2-3)</p>
                                                    </td>
                                                    <td tabindex=""0"">
                                                        <i class=""ms-Icon ms-Icon--Download used-by-download-icon"" aria-hidden=""true""></i> <label class=""used-by-count"">18.3K</label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class=""used-by-desc-column"" tabindex=""0"">
                                                        <a class=""text-left ngp-link"" href=""/packages/Xe.Framework.All/"">
                                                            Xe.Framework.All
                                                        </a>
                                                        <p class=""used-by-desc"">Package Description</p>
                                                    </td>
                                                    <td tabindex=""0"">
                                                        <i class=""ms-Icon ms-Icon--Download used-by-download-icon"" aria-hidden=""true""></i> <label class=""used-by-count"">6.4K</label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class=""used-by-desc-column"" tabindex=""0"">
                                                        <a class=""text-left ngp-link"" href=""/packages/Xe.Framework.PdfTools/"">
                                                            Xe.Framework.PdfTools
                                                        </a>
                                                        <p class=""used-by-desc"">Package Description</p>
                                                    </td>
                                                    <td tabindex=""0"">
                                                        <i class=""ms-Icon ms-Icon--Download used-by-download-icon"" aria-hidden=""true""></i> <label class=""used-by-count"">1.0K</label>
                                                    </td>
                                                </tr>
                                        </tbody>
                                    </table>

                                    <h3>
                                        <strong>GitHub repositories</strong>
                                    </h3>
                                    <p>
                                        This package is not used by any popular GitHub repositories.
                                    </p>
                        </div>
                </div>
                <div role=""tabpanel"" class=""tab-pane "" id=""versions-tab"" aria-label=""Versions tab content"">
                    <div class=""version-history"" id=""version-history"">
                        <table aria-label=""Version History of Spire.PDF"" class=""table borderless"">
                            <thead>
                                <tr>
                                    <th scope=""col"" role=""columnheader"" tabindex=""0"">Version</th>
                                    <th scope=""col"" role=""columnheader"" tabindex=""0"">Downloads</th>
                                    <th scope=""col"" role=""columnheader"" tabindex=""0"">Last updated</th>
                                                                                                                <th scope=""col"" role=""columnheader"" aria-hidden=""true"" abbr=""Package Warnings""></th>
                                </tr>
                            </thead>
                            <tbody class=""no-border"">
                                        <tr class=""bg-info"">
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/8.3.9"" title=""8.3.9"">
                                                    8.3.9
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                1,724
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2022-03-23T07:07:38.1370000"" title=""2022-03-23T07:07:38Z"">15 days ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/8.2.2"" title=""8.2.2"">
                                                    8.2.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,916
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2022-02-22T08:24:57.3270000"" title=""2022-02-22T08:24:57Z"">2 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/8.1.4"" title=""8.1.4"">
                                                    8.1.4
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,675
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2022-01-19T07:31:04.6570000"" title=""2022-01-19T07:31:04Z"">3 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/8.1.0"" title=""8.1.0"">
                                                    8.1.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,382
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2022-01-06T01:59:42.4970000"" title=""2022-01-06T01:59:42Z"">3 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.12.1"" title=""7.12.1"">
                                                    7.12.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                7,091
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-12-08T07:20:28.7070000"" title=""2021-12-08T07:20:28Z"">4 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.11.1"" title=""7.11.1"">
                                                    7.11.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                8,954
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-11-09T06:26:13.0100000"" title=""2021-11-09T06:26:13Z"">5 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.10.4"" title=""7.10.4"">
                                                    7.10.4
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                9,703
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-10-19T02:19:08.7270000"" title=""2021-10-19T02:19:08Z"">6 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.10.0"" title=""7.10.0"">
                                                    7.10.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                1,854
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-10-09T08:32:56.3900000"" title=""2021-10-09T08:32:56Z"">6 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.9.6"" title=""7.9.6"">
                                                    7.9.6
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,545
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-09-22T08:46:45.9000000"" title=""2021-09-22T08:46:45Z"">7 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.9.2"" title=""7.9.2"">
                                                    7.9.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,236
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-09-02T09:32:47.1530000"" title=""2021-09-02T09:32:47Z"">7 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.8.8"" title=""7.8.8"">
                                                    7.8.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,819
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-08-20T06:44:54.4500000"" title=""2021-08-20T06:44:54Z"">8 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.8.2"" title=""7.8.2"">
                                                    7.8.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                7,910
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-08-04T11:03:30.4600000"" title=""2021-08-04T11:03:30Z"">8 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.7.10"" title=""7.7.10"">
                                                    7.7.10
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,751
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-07-22T02:40:54.4530000"" title=""2021-07-22T02:40:54Z"">9 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.6.15"" title=""7.6.15"">
                                                    7.6.15
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                7,657
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-06-29T08:01:57.8030000"" title=""2021-06-29T08:01:57Z"">9 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.6.1"" title=""7.6.1"">
                                                    7.6.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,098
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-06-10T02:56:45.6500000"" title=""2021-06-10T02:56:45Z"">10 months ago</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.5.0"" title=""7.5.0"">
                                                    7.5.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,696
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-05-07T06:32:39.7570000"" title=""2021-05-07T06:32:39Z"">2021/5/7</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.4.13"" title=""7.4.13"">
                                                    7.4.13
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                9,765
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-04-27T09:03:26.1770000"" title=""2021-04-27T09:03:26Z"">2021/4/27</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.4.5"" title=""7.4.5"">
                                                    7.4.5
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                7,499
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-04-14T08:35:31.6730000"" title=""2021-04-14T08:35:31Z"">2021/4/14</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.3.3"" title=""7.3.3"">
                                                    7.3.3
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                8,066
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-03-19T09:16:25.7230000"" title=""2021-03-19T09:16:25Z"">2021/3/19</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.3.1"" title=""7.3.1"">
                                                    7.3.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,356
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-03-12T03:30:26.4670000"" title=""2021-03-12T03:30:26Z"">2021/3/12</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.2.9"" title=""7.2.9"">
                                                    7.2.9
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,392
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-02-20T09:49:39.6700000"" title=""2021-02-20T09:49:39Z"">2021/2/20</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.2.5"" title=""7.2.5"">
                                                    7.2.5
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,728
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-02-07T09:00:45.4870000"" title=""2021-02-07T09:00:45Z"">2021/2/7</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.2.0"" title=""7.2.0"">
                                                    7.2.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,439
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-02-02T08:43:15.3430000"" title=""2021-02-02T08:43:15Z"">2021/2/2</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.1.10"" title=""7.1.10"">
                                                    7.1.10
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,923
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-01-15T09:23:41.2500000"" title=""2021-01-15T09:23:41Z"">2021/1/15</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/7.1.0"" title=""7.1.0"">
                                                    7.1.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,066
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2021-01-07T10:02:10.0830000"" title=""2021-01-07T10:02:10Z"">2021/1/7</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.12.20"" title=""6.12.20"">
                                                    6.12.20
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,993
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-12-25T07:01:02.3700000"" title=""2020-12-25T07:01:02Z"">2020/12/25</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.12.10"" title=""6.12.10"">
                                                    6.12.10
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,171
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-12-10T08:56:13.0870000"" title=""2020-12-10T08:56:13Z"">2020/12/10</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.12.3"" title=""6.12.3"">
                                                    6.12.3
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,484
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-12-07T09:22:29.0500000"" title=""2020-12-07T09:22:29Z"">2020/12/7</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.11.6"" title=""6.11.6"">
                                                    6.11.6
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                8,891
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-11-13T08:25:39.1700000"" title=""2020-11-13T08:25:39Z"">2020/11/13</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.11.0"" title=""6.11.0"">
                                                    6.11.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,562
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-11-04T08:40:07.8270000"" title=""2020-11-04T08:40:07Z"">2020/11/4</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.10.9"" title=""6.10.9"">
                                                    6.10.9
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,353
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-10-27T09:04:31.4730000"" title=""2020-10-27T09:04:31Z"">2020/10/27</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.10.4"" title=""6.10.4"">
                                                    6.10.4
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,931
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-10-20T08:59:17.7730000"" title=""2020-10-20T08:59:17Z"">2020/10/20</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.10.0"" title=""6.10.0"">
                                                    6.10.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,972
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-10-10T07:58:24.7370000"" title=""2020-10-10T07:58:24Z"">2020/10/10</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.9.16"" title=""6.9.16"">
                                                    6.9.16
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,112
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-09-28T08:09:41.6200000"" title=""2020-09-28T08:09:41Z"">2020/9/28</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.9.0"" title=""6.9.0"">
                                                    6.9.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                17,177
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-09-03T09:23:37.4800000"" title=""2020-09-03T09:23:37Z"">2020/9/3</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.8.5"" title=""6.8.5"">
                                                    6.8.5
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                12,968
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-08-17T09:24:54.3800000"" title=""2020-08-17T09:24:54Z"">2020/8/17</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.8.1"" title=""6.8.1"">
                                                    6.8.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,287
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-08-06T06:48:40.3070000"" title=""2020-08-06T06:48:40Z"">2020/8/6</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.7.12"" title=""6.7.12"">
                                                    6.7.12
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,344
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-07-29T08:11:46.4500000"" title=""2020-07-29T08:11:46Z"">2020/7/29</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.7.8"" title=""6.7.8"">
                                                    6.7.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,368
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-07-20T08:16:59.1700000"" title=""2020-07-20T08:16:59Z"">2020/7/20</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.7.6"" title=""6.7.6"">
                                                    6.7.6
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,861
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-07-08T08:29:48.3600000"" title=""2020-07-08T08:29:48Z"">2020/7/8</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.7.2"" title=""6.7.2"">
                                                    6.7.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                8,342
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-07-06T07:12:14.4470000"" title=""2020-07-06T07:12:14Z"">2020/7/6</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.5.15"" title=""6.5.15"">
                                                    6.5.15
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                11,480
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-05-28T07:59:26.3300000"" title=""2020-05-28T07:59:26Z"">2020/5/28</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.5.9"" title=""6.5.9"">
                                                    6.5.9
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,918
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-05-18T06:45:11.7500000"" title=""2020-05-18T06:45:11Z"">2020/5/18</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.5.6"" title=""6.5.6"">
                                                    6.5.6
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                14,731
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-05-13T07:20:03.8070000"" title=""2020-05-13T07:20:03Z"">2020/5/13</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.4.11"" title=""6.4.11"">
                                                    6.4.11
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                34,936
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-04-20T10:11:23.6300000"" title=""2020-04-20T10:11:23Z"">2020/4/20</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.4.4"" title=""6.4.4"">
                                                    6.4.4
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,636
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-04-10T04:24:12.2130000"" title=""2020-04-10T04:24:12Z"">2020/4/10</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.3.16"" title=""6.3.16"">
                                                    6.3.16
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,086
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-03-30T09:47:06.8870000"" title=""2020-03-30T09:47:06Z"">2020/3/30</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.3.10"" title=""6.3.10"">
                                                    6.3.10
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                11,291
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-03-23T09:21:15.6000000"" title=""2020-03-23T09:21:15Z"">2020/3/23</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.3.0"" title=""6.3.0"">
                                                    6.3.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                10,938
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-03-04T08:27:05.1270000"" title=""2020-03-04T08:27:05Z"">2020/3/4</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.2.6"" title=""6.2.6"">
                                                    6.2.6
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,781
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-02-19T08:05:56.4030000"" title=""2020-02-19T08:05:56Z"">2020/2/19</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.2.1"" title=""6.2.1"">
                                                    6.2.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,565
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-02-11T09:17:29.7470000"" title=""2020-02-11T09:17:29Z"">2020/2/11</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.1.8"" title=""6.1.8"">
                                                    6.1.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                17,781
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-01-21T08:30:21.2000000"" title=""2020-01-21T08:30:21Z"">2020/1/21</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/6.1.4"" title=""6.1.4"">
                                                    6.1.4
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,421
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2020-01-09T08:17:32.5070000"" title=""2020-01-09T08:17:32Z"">2020/1/9</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.12.15"" title=""5.12.15"">
                                                    5.12.15
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                9,707
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-12-16T06:41:47.4400000"" title=""2019-12-16T06:41:47Z"">2019/12/16</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.12.3"" title=""5.12.3"">
                                                    5.12.3
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,300
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-12-05T10:02:46.6430000"" title=""2019-12-05T10:02:46Z"">2019/12/5</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.11.18"" title=""5.11.18"">
                                                    5.11.18
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,239
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-11-29T06:43:05.8330000"" title=""2019-11-29T06:43:05Z"">2019/11/29</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.11.2"" title=""5.11.2"">
                                                    5.11.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                7,671
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-11-07T05:58:17.8900000"" title=""2019-11-07T05:58:17Z"">2019/11/7</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.11.0"" title=""5.11.0"">
                                                    5.11.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,001
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-11-01T09:51:08.4200000"" title=""2019-11-01T09:51:08Z"">2019/11/1</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.10.8"" title=""5.10.8"">
                                                    5.10.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,142
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-10-22T09:58:18.5500000"" title=""2019-10-22T09:58:18Z"">2019/10/22</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.10.2"" title=""5.10.2"">
                                                    5.10.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,161
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-10-11T09:06:38.7470000"" title=""2019-10-11T09:06:38Z"">2019/10/11</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.9.16"" title=""5.9.16"">
                                                    5.9.16
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,490
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-09-30T06:31:10.4570000"" title=""2019-09-30T06:31:10Z"">2019/9/30</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.9.6"" title=""5.9.6"">
                                                    5.9.6
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,401
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-09-18T07:35:03.4230000"" title=""2019-09-18T07:35:03Z"">2019/9/18</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.8.16"" title=""5.8.16"">
                                                    5.8.16
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                12,049
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-08-30T09:04:01.1100000"" title=""2019-08-30T09:04:01Z"">2019/8/30</span>
                                            </td>


                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.8.7"" title=""5.8.7"">
                                                    5.8.7
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                9,135
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-08-13T09:39:18.4900000"" title=""2019-08-13T09:39:18Z"">2019/8/13</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.8.2"" title=""5.8.2"">
                                                    5.8.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,594
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-08-06T09:00:04.3200000"" title=""2019-08-06T09:00:04Z"">2019/8/6</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.7.20"" title=""5.7.20"">
                                                    5.7.20
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                1,714
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-07-31T08:18:40.0600000"" title=""2019-07-31T08:18:40Z"">2019/7/31</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.7.0"" title=""5.7.0"">
                                                    5.7.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                16,421
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-07-02T07:37:20.3770000"" title=""2019-07-02T07:37:20Z"">2019/7/2</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.6.31"" title=""5.6.31"">
                                                    5.6.31
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                923
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-07-01T02:07:14.8000000"" title=""2019-07-01T02:07:14Z"">2019/7/1</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.6.2"" title=""5.6.2"">
                                                    5.6.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                11,897
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-06-06T08:18:23.4570000"" title=""2019-06-06T08:18:23Z"">2019/6/6</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.5.21"" title=""5.5.21"">
                                                    5.5.21
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,764
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-05-24T07:00:29.0800000"" title=""2019-05-24T07:00:29Z"">2019/5/24</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.4.21"" title=""5.4.21"">
                                                    5.4.21
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                44,759
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-05-01T06:04:22.5100000"" title=""2019-05-01T06:04:22Z"">2019/5/1</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.4.20"" title=""5.4.20"">
                                                    5.4.20
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                959
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-04-29T09:35:34.7700000"" title=""2019-04-29T09:35:34Z"">2019/4/29</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.4.9"" title=""5.4.9"">
                                                    5.4.9
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,411
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-04-15T03:28:14.0970000"" title=""2019-04-15T03:28:14Z"">2019/4/15</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.4.1"" title=""5.4.1"">
                                                    5.4.1
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,726
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-04-04T06:49:36.1400000"" title=""2019-04-04T06:49:36Z"">2019/4/4</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.3.8"" title=""5.3.8"">
                                                    5.3.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                44,309
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-03-19T03:18:33.2570000"" title=""2019-03-19T03:18:33Z"">2019/3/19</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.3.2"" title=""5.3.2"">
                                                    5.3.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,844
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-03-11T09:50:59.2270000"" title=""2019-03-11T09:50:59Z"">2019/3/11</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.2.15"" title=""5.2.15"">
                                                    5.2.15
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,091
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-02-28T09:52:18.6000000"" title=""2019-02-28T09:52:18Z"">2019/2/28</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.2.3"" title=""5.2.3"">
                                                    5.2.3
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,032
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-02-14T06:46:09.2630000"" title=""2019-02-14T06:46:09Z"">2019/2/14</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.1.33"" title=""5.1.33"">
                                                    5.1.33
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                2,495
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-01-31T07:59:05.4400000"" title=""2019-01-31T07:59:05Z"">2019/1/31</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.1.16"" title=""5.1.16"">
                                                    5.1.16
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,664
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-01-19T09:33:27.0470000"" title=""2019-01-19T09:33:27Z"">2019/1/19</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.1.4"" title=""5.1.4"">
                                                    5.1.4
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                1,895
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-01-11T06:49:54.7470000"" title=""2019-01-11T06:49:54Z"">2019/1/11</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/5.1.0"" title=""5.1.0"">
                                                    5.1.0
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                6,001
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2019-01-02T08:22:32.7070000"" title=""2019-01-02T08:22:32Z"">2019/1/2</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/4.12.8"" title=""4.12.8"">
                                                    4.12.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                11,783
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2018-12-18T08:09:01.6500000"" title=""2018-12-18T08:09:01Z"">2018/12/18</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/4.12.2"" title=""4.12.2"">
                                                    4.12.2
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                3,399
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2018-12-07T10:13:36.9600000"" title=""2018-12-07T10:13:36Z"">2018/12/7</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/4.11.12"" title=""4.11.12"">
                                                    4.11.12
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                5,463
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2018-11-28T03:07:55.2870000"" title=""2018-11-28T03:07:55Z"">2018/11/28</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/4.11.8"" title=""4.11.8"">
                                                    4.11.8
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                16,952
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2018-11-16T09:09:13.9670000"" title=""2018-11-16T09:09:13Z"">2018/11/16</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/4.11.5"" title=""4.11.5"">
                                                    4.11.5
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                4,531
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2018-11-09T07:31:45.1500000"" title=""2018-11-09T07:31:45Z"">2018/11/9</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                                        <tr>
                                            <td tabindex=""0"">
                                                <a href=""/packages/Spire.PDF/4.11.3"" title=""4.11.3"">
                                                    4.11.3
                                                </a>
                                            </td>
                                            <td tabindex=""0"">
                                                10,529
                                            </td>
                                            <td tabindex=""0"">
                                                <span data-datetime=""2018-11-08T03:41:20.7230000"" title=""2018-11-08T03:41:20Z"">2018/11/8</span>
                                            </td>
                                                <td class=""package-icon-cell"" aria-hidden=""true""></td>
                                        </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                    <div role=""tabpanel"" class=""tab-pane "" id=""releasenotes-tab"" aria-label=""Release notes tab content"">
                        <p><a href=""https://www.e-iceblue.com/news/spire-pdf.html"" rel=""nofollow"">https://www.e-iceblue.com/news/spire-pdf.html</a></p>
                    </div>
            </div>
        </div>
        <aside aria-label=""Package details info"" class=""col-sm-3 package-details-info"">
            <div class=""sidebar-section"">
                <div class=""row sidebar-headers"">
                    <div class=""col-md-6"">
                        Downloads
                    </div>
                    <div class=""col-md-6 title-links"">
                            <a href=""/stats/packages/Spire.PDF?groupby=Version"" title=""Package Statistics"">Full stats →</a>
                    </div>
                </div>
                <div class=""download-info"">
                    <div class=""download-info-row"">
                        <span class=""download-info-header"">Total</span>
                        <span class=""download-info-content"">1.1M</span>
                    </div>
                    <div class=""download-info-row"">
                        <span class=""download-info-header"">Current version</span>
                        <span class=""download-info-content"">1.7K</span>
                    </div>
                    <div class=""download-info-row"">
                        <span class=""download-info-header"">Per day average</span>
                        <span class=""download-info-content"">331</span>
                    </div>
                </div>
            </div>
            <div class=""sidebar-section"">
                <div class=""sidebar-headers"">About</div>
                <ul class=""list-unstyled ms-Icon-ul sidebar-links"">
                    <li>
                        <i class=""ms-Icon ms-Icon--History"" aria-hidden=""true""></i>
                        Last updated <span data-datetime=""2022-03-23T07:07:38.1370000"" title=""2022-03-23T07:07:38Z"">15 days ago</span>
                    </li>
                        <li>
                            <i class=""ms-Icon ms-Icon--Globe"" aria-hidden=""true""></i>
                            <a href=""http://www.e-iceblue.com/Introduce/pdf-for-net-introduce.html"" data-track=""outbound-project-url"" title=""Visit the project site to learn more about this package"" rel=""nofollow"">
                                Project website
                            </a>
                        </li>
                            <li>
                                <i class=""ms-Icon ms-Icon--Certificate"" aria-hidden=""true""></i>
                                    <a href=""/packages/Spire.PDF/8.3.9/License"" data-track=""outbound-license-url"" title=""Make sure you agree with the license"" rel=""nofollow"">
                                        License Info
                                    </a>
                            </li>
                        <li>
                            <i class=""ms-Icon ms-Icon--CloudDownload"" aria-hidden=""true""></i>
                            <a href=""https://www.nuget.org/api/v2/package/Spire.PDF/8.3.9"" data-track=""outbound-manual-download"" title=""Download the raw nupkg file."" rel=""nofollow"">Download package</a>
                            &nbsp;(53.89 MB)
                        </li>

                        <li>
                            <i class=""ms-Icon ms-Icon--FabricFolderSearch"" aria-hidden=""true"" aria-label=""nuget.info is a 3rd party website, not controlled by Microsoft. This link is made available to you per the NuGet Terms of Use."" title=""nuget.info is a 3rd party website, not controlled by Microsoft. This link is made available to you per the NuGet Terms of Use.""></i>
                            <a href=""https://nuget.info/packages/Spire.PDF/8.3.9"" data-track=""outbound-nugetpackageexplorer-url"" aria-label=""open in NuGet Package Explorer"" title=""Explore additional package info on NuGet Package Explorer"" target=""_blank"" rel=""nofollow noreferrer"">
                                Open in NuGet Package Explorer
                            </a>
                        </li>
                        <li>
                            <img class=""icon"" aria-label=""fuget.org is a 3rd party website, not controlled by Microsoft. This link is made available to you per the NuGet Terms of Use."" title=""fuget.org is a 3rd party website, not controlled by Microsoft. This link is made available to you per the NuGet Terms of Use."" src=""https://www.nuget.org/Content/gallery/img/fuget.svg"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/fuget-32x32.png'; this.onerror = null;"">
                            <a href=""https://www.fuget.org/packages/Spire.PDF/8.3.9"" data-track=""outbound-fuget-url"" aria-label=""open in fuget.org explorer"" title=""Explore additional package info on fuget.org"" target=""_blank"" rel=""nofollow noreferrer"">
                                Open in FuGet Package Explorer
                            </a>
                        </li>

                        <li class=""report-link"">
                            <i class=""ms-Icon ms-Icon--Flag"" aria-hidden=""true""></i>
                            <a href=""/packages/Spire.PDF/8.3.9/ReportAbuse"" title=""Report the package as abusive"">
                                Report package
                            </a>
                        </li>
                </ul>
            </div>
            <div class=""sidebar-section"">
                <div class=""row sidebar-headers"">
                    <div class=""col-md-6"">
                        Owners
                    </div>
                    <div class=""col-md-6 title-links"">
                        <a href=""/packages/Spire.PDF/8.3.9/ContactOwners"" title=""Ask the package owners a question"">Contact owners →</a>
                    </div>
                </div>
                    <ul class=""list-unstyled owner-list"">
                            <li>
                                    <a href=""/profiles/e-iceblue"" title=""e-iceblue"">
                                        <img src=""/profiles/e-iceblue/avatar?imageSize=64"" class=""owner-image"" height=""32"" width=""32"" title=""e-iceblue"" alt=""e-iceblue gravatar"">
                                    </a>
                                <a href=""/profiles/e-iceblue"" title=""e-iceblue"">
                                    e-iceblue
                                </a>
                            </li>
                    </ul>
            </div>
                                    <p>
                            <a href=""/packages?q=Tags%3A%22PDF%22"" title=""Search for PDF"" class=""tag"">PDF</a>
                            <a href=""/packages?q=Tags%3A%22application%22"" title=""Search for application"" class=""tag"">application</a>
                            <a href=""/packages?q=Tags%3A%22PDF%22"" title=""Search for PDF"" class=""tag"">PDF</a>
                            <a href=""/packages?q=Tags%3A%22library%22"" title=""Search for library"" class=""tag"">library</a>
                            <a href=""/packages?q=Tags%3A%22component%22"" title=""Search for component"" class=""tag"">component</a>
                            <a href=""/packages?q=Tags%3A%22control%22"" title=""Search for control"" class=""tag"">control</a>
                            <a href=""/packages?q=Tags%3A%22to%22"" title=""Search for to"" class=""tag"">to</a>
                            <a href=""/packages?q=Tags%3A%22XPS%22"" title=""Search for XPS"" class=""tag"">XPS</a>
                            <a href=""/packages?q=Tags%3A%22Image%22"" title=""Search for Image"" class=""tag"">Image</a>
                            <a href=""/packages?q=Tags%3A%22HTML%22"" title=""Search for HTML"" class=""tag"">HTML</a>
                            <a href=""/packages?q=Tags%3A%22HTML%22"" title=""Search for HTML"" class=""tag"">HTML</a>
                            <a href=""/packages?q=Tags%3A%22PDF%22"" title=""Search for PDF"" class=""tag"">PDF</a>
                            <a href=""/packages?q=Tags%3A%22PDF%2FA%22"" title=""Search for PDF/A"" class=""tag"">PDF/A</a>
                            <a href=""/packages?q=Tags%3A%22word%22"" title=""Search for word"" class=""tag"">word</a>
                            <a href=""/packages?q=Tags%3A%22Excel%22"" title=""Search for Excel"" class=""tag"">Excel</a>
                            <a href=""/packages?q=Tags%3A%22PDF-to-DOC%22"" title=""Search for PDF-to-DOC"" class=""tag"">PDF-to-DOC</a>
                            <a href=""/packages?q=Tags%3A%22Docx%22"" title=""Search for Docx"" class=""tag"">Docx</a>
                            <a href=""/packages?q=Tags%3A%22OFD%22"" title=""Search for OFD"" class=""tag"">OFD</a>
                            <a href=""/packages?q=Tags%3A%22PDF%2FA%22"" title=""Search for PDF/A"" class=""tag"">PDF/A</a>
                            <a href=""/packages?q=Tags%3A%22form%22"" title=""Search for form"" class=""tag"">form</a>
                            <a href=""/packages?q=Tags%3A%22field%22"" title=""Search for field"" class=""tag"">field</a>
                            <a href=""/packages?q=Tags%3A%22pdf%22"" title=""Search for pdf"" class=""tag"">pdf</a>
                            <a href=""/packages?q=Tags%3A%22xlsx%22"" title=""Search for xlsx"" class=""tag"">xlsx</a>
                            <a href=""/packages?q=Tags%3A%22svg%22"" title=""Search for svg"" class=""tag"">svg</a>
                            <a href=""/packages?q=Tags%3A%22xps%22"" title=""Search for xps"" class=""tag"">xps</a>
                            <a href=""/packages?q=Tags%3A%22html%22"" title=""Search for html"" class=""tag"">html</a>
                            <a href=""/packages?q=Tags%3A%22PDF%22"" title=""Search for PDF"" class=""tag"">PDF</a>
                            <a href=""/packages?q=Tags%3A%22merge%22"" title=""Search for merge"" class=""tag"">merge</a>
                            <a href=""/packages?q=Tags%3A%22pdf%22"" title=""Search for pdf"" class=""tag"">pdf</a>
                            <a href=""/packages?q=Tags%3A%22split%22"" title=""Search for split"" class=""tag"">split</a>
                            <a href=""/packages?q=Tags%3A%22ofd%22"" title=""Search for ofd"" class=""tag"">ofd</a>
                            <a href=""/packages?q=Tags%3A%22signature%22"" title=""Search for signature"" class=""tag"">signature</a>
                            <a href=""/packages?q=Tags%3A%22net%22"" title=""Search for net"" class=""tag"">net</a>
                            <a href=""/packages?q=Tags%3A%22core%22"" title=""Search for core"" class=""tag"">core</a>
                            <a href=""/packages?q=Tags%3A%22.net%22"" title=""Search for .net"" class=""tag"">.net</a>
                            <a href=""/packages?q=Tags%3A%22standard%22"" title=""Search for standard"" class=""tag"">standard</a>
                            <a href=""/packages?q=Tags%3A%22Visual-Studio%22"" title=""Search for Visual-Studio"" class=""tag"">Visual-Studio</a>
                            <a href=""/packages?q=Tags%3A%22VisualStudio%22"" title=""Search for VisualStudio"" class=""tag"">VisualStudio</a>
                    </p>
                    <p>Copyright © 2022 e-iceblue. All Rights Reserved.</p>

                <p class=""share-buttons"">
                    <a href=""https://www.facebook.com/sharer/sharer.php?u=https://www.nuget.org/packages/Spire.PDF/&amp;t=Check+out+Spire.PDF+on+%23NuGet."" target=""_blank"" rel=""nofollow noreferrer"">
                        <img width=""24"" height=""24"" alt=""Share this package on Facebook"" src=""https://www.nuget.org/Content/gallery/img/facebook.svg"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/facebook-24x24.png'; this.onerror = null;"">
                    </a>
                    <a href=""https://twitter.com/intent/tweet?url=https://www.nuget.org/packages/Spire.PDF/&amp;text=Check+out+Spire.PDF+on+%23NuGet."" target=""_blank"" rel=""nofollow noreferrer"">
                        <img width=""24"" height=""24"" alt=""Tweet this package"" src=""https://www.nuget.org/Content/gallery/img/twitter.svg"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/twitter-24x24.png'; this.onerror = null;"">
                    </a>
                        <a href=""/packages/Spire.PDF/atom.xml"" data-track=""atom-feed"">
                            <img width=""24"" height=""24"" alt=""Use the Atom feed to subscribe to new versions of Spire.PDF"" src=""https://www.nuget.org/Content/gallery/img/rss.svg"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/rss-24x24.png'; this.onerror = null;"">
                        </a>
                </p>
        </aside>
    </div>
</section>


    </div>
    <footer class=""footer"">
    <div class=""container"">
        <div class=""row"">
            <div class=""col-sm-4"">
                <span class=""footer-heading""><a href=""/policies/Contact"">Contact</a></span>
                <p>
                    Got questions about NuGet or the NuGet Gallery?
                </p>
            </div>
            <div class=""col-sm-4"">
                <span class=""footer-heading""><a href=""https://status.nuget.org/"">Status</a></span>
                <p>
                    Find out the service status of NuGet.org and its related services.
                </p>
            </div>
            <div class=""col-sm-4"">
                <span class=""footer-heading"">
                    <a aria-label=""Frequently Asked Questions"" href=""https://docs.microsoft.com/en-us/nuget/policies/nuget-faq"">
                        <abbr title=""Frequently Asked Questions"">FAQ</abbr>
                    </a>
                </span>
                <p>
                    Read the Frequently Asked Questions about NuGet and see if your question made the list.
                </p>
            </div>
        </div>
        <div class=""row"">
            <div class=""col-md-3 row-gap"">
                <img alt="""" aria-hidden=""true"" width=""184"" height=""57"" src=""/Content/gallery/img/logo-footer.svg"" onerror=""this.src='https://www.nuget.org/Content/gallery/img/logo-footer-184x57.png'; this.onerror = null;"">
            </div>
            <div class=""col-md-9 row-gap"">
                <div class=""row"">
                        <div class=""col-md-12 footer-release-info"">
                            <p>
                                        © Microsoft 2022 -

                                <a href=""/policies/About"">About</a> -
                                <a href=""/policies/Terms"">Terms of Use</a> -
                                <a href=""https://go.microsoft.com/fwlink/?LinkId=521839"" id=""footer-privacy-policy-link"">Privacy Policy</a> - <a class=""button"" href=""javascript: window.nuget.wcpSiteConsent.manageConsent()""> Manage Cookies</a>
                                    - <a href=""https://www.microsoft.com/trademarks"">Trademarks</a>
                                <br>
                            </p>
                        </div>
                </div>
            </div>
        </div>
    </div>
<!--
    This is the NuGet Gallery version 4.4.5-main.
        Deployed from 4427e2b Link: https://www.github.com/NuGet/NuGetGallery/commit/4427e2b
        Built on main Link: https://www.github.com/NuGet/NuGetGallery/tree/main
        Built on 2022-03-29T20:21:02.0000000+00:00
    Deployment label: PROD-USNC-4.4.5-main-5952884
    You are on RD0003FFF9F05B.
-->
</footer>";

        #region 图片 Base64互转

        protected string ImgToBase64String(MemoryStream msImage)
        {
            try
            {
                byte[] arr = new byte[msImage.Length];
                msImage.Position = 0;
                msImage.Read(arr, 0, (int)msImage.Length);
                msImage.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //base64编码的字符串转为图片 
        protected Bitmap Base64StringToImage(string strbase64)
        {
            try
            {
                byte[] arr = Convert.FromBase64String(strbase64);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);
                //bmp.Save(@"d:\test.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Close();
                return bmp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
    }
}
