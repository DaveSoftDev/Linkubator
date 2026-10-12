$ErrorActionPreference = "Stop"

$repoRoot = (Get-Item (Join-Path $PSScriptRoot "..\..")).FullName
$domainProject = Join-Path $repoRoot "src\Linkubator.Domain\Linkubator.Domain.csproj"

if (-not (Test-Path $domainProject)) {
    throw "No se encuentra el proyecto de Domain: $domainProject"
}

# Sustituir por las 30 URLs reales de DLG antes de la revisión final.
$urls = @(
    "example.com/articulo",
    "https://Example.com/a",
    "https://example.com/c++/tutorial?q=c++",
    "https://example.com:8080/x",
    "https://münchen.de/日本語",
    "https://www.mediamarkt.es/es/product/_camara-instantanea-fujifilm-instax-mini-evo-iso-100-1600-pantalla-lcd-bluetooth-negro-1561872.html?utm_source=google&utm_medium=v2-sho&utm_campaign=es%7Cmm%7Crt%7Cv2-sho%7Cpmax%7Cnsp%7CTOP%7Cna%7Cd2c%7CNA%7CB0009TVC&utm_content=B0009TVC-TCID23908559954&gclsrc=aw.ds&gad_source=1&gad_campaignid=23912985034&gbraid=0AAAAADL_At-3b3_-pCuNKCsXxHKxHSfoY",
    "https://www.youtube.com/watch?v=kz0zNSoRYEY",
    "http://youpic.com/",
    "https://fotok.es/?aff=y2336",
    "https://fotok.es:443/?aff=y2336",
    "https://www.amazon.es/hz/wishlist/ls/1344AEK0W80T?ref_=list_d_wl_lfu_nav_1",
    "https://www.kentfaith.es/?tracking=600e8bca2c20a",
    "https://www.google.com/search?q=fuji+camara+carrete+digital&sca_esv=aeb3d629ae2e4e7f&sxsrf=APpeQnuOrjVGMBu1uixtySb2U_9nDI9r4A%3A1791740402119&source=hp&ei=8snLapTiBenY7M8PptbbyAw&iflsig=ABILxe8AAAAAasvYApVkUhde-2Um8iwW_5j5CjxutuAb&ved=0ahUKEwiUo8mRwbKXAxVpLPsDHSbrFskQ4dUDCB0&uact=5&oq=fuji+camara+carrete+digital&gs_lp=Egdnd3Mtd2l6IhtmdWppIGNhbWFyYSBjYXJyZXRlIGRpZ2l0YWwyBRAAGO8FMgUQABjvBTIFEAAY7wUyBRAAGO8FSNVgUABY6FxwAngAkAEAmAGMAaAB-hGqAQQyNi4zuAEDyAEA-AEBmAIfoALEE8ICCBAAGIAEGLQHwgILEC4YgAQYsQMYgwHCAhEQLhiABBixAxiDARjHARjRA8ICCxAAGIAEGLEDGIMBwgIIEAAYgAQYsQPCAgUQLhiABMICBRAAGIAEwgIOEC4YgAQYsQMYxwEY0QPCAggQLhiABBixA8ICDhAuGIAEGMcBGK8BGI4FwgILEC4YgAQYxwEY0QPCAgsQLhiABBjHARivAcICCBAAGIAEGKIEwgIGEAAYFhgewgIFECEYoAHCAgUQIRifBcICBBAhGBXCAgcQIRgKGKABmAMAkgcEMjYuNaAHzqYBsgcEMjQuNbgHuBPCBwkwLjE0LjE2LjHIB2mACAE&sclient=gws-wiz",
    "https://www.bbc.com/news/world-europe-65512345",
    "https://www.youtube.com/watch?v=r9_xnJnWrUM&pp=0gcJCTcMAYcqIYzv",
    "https://www.facebook.com/feec.cat?__cft__[0]=AZi65nd0hgCO6spYIH8XCfQy5AH0uRRcxKEL6dMo-MT6sDCcc_y7VH0OGm1KO0O4TIXzFywtwGyDoSpZzndzMAAdoBAZWgjBgpnmOv_xLglWBpIG9WcrMpjzXsXjpYUjPgybXqbaOW0pFmW1sDUhaRYC6I5Sr7kv-WYMPOdFDX8Xfstmr3nlVPB4qQY&__tn__=-]K-R"
    "https://www.tumblr.com/login?redirect_to=%2Fwidgets%2Fshare%2Ftool%3FcanonicalUrl%3Dhttps%253A%252F%252Fgurushots.com%252F",
    "https://x.com/i/jf/onboarding/web?redirect_after_login=%2Fintent%2Ftweet%3Fhashtags%3Dgurushots%26text%3Dhttps%253A%252F%252Fgurushots.com%252F%26redirect_uri%3Dhttps%253A%252F%252Fapi.gurushots.com%252Fviral%252Fredirect%252Fundefined&mode=login",
    "https://www.facebook.com/dialog/share?app_id=2024451431135713&display=popup&href=https%3A%2F%2Fgurushots.com%2F&redirect_uri=https%3A%2F%2Fapi.gurushots.com%2Fviral%2Fredirect%2Fundefined"
    "https://www.youtube.com/redirect?event=video_description&redir_token=QUZZTVljSFgyeVFGbkRzM1h0Q0kyMnBtVkxVc3xBSTVJdTVzY2tlbVdNczA5TUdWRDRtT1U1aFpvRjh5ODhnQjB5N0ZxTnkwd1VvOXF1eGpXUG5MZGVUQUZzV1BsMklpWU9sSWJUNnRRdzF6ZGVoM0Nldk4yVW1yUzNZRXNUNHNO&q=https%3A%2F%2Fwww.casanovafoto.com%2Ffujifilm-x-half-x-hf1-antracita-camara-compacta-digital&v=lQsGpwptyhg",
    "https://l.facebook.com/l.php?u=https%3A%2F%2Fviveharley.tumblr.com%2Fpost%2F140860833419%2Festuvimos-en-riverside-9-2014-crazy-car-hop%23_%3D_&h=AUBBoruzKjA8ouAVNN2JzC5gmUlgox7QRePJy1uLYLz4lCv4N1KcedyNv2Ros_yFkEssnOcDDkKaW54uy4jFUdNEHyW4P93R3lkDTAObJDqm8PrcbogmheCby_f60jzF7s53mk3jw2a-r-oLqGy9&__tn__=%2CmH-R&c[0]=AUCSl9V18dVNdmqEIHvSiT8QOCzztZlIqszghqQj6Gft9bL-cRBxius4HyKHodjiwR82sbsTTlWAJPKZI7UwZCnYgevNoe4q1hRUNUGcu9eUs0cdEQsVzmz4uLyb97J8CoY1Cidc0qQZ3KPc763rBCjEZCShCG3NsIwWvnMo0KRDWGBQM7w_P2Ls4YOBNL80Xt_f183D8-yRViGqefQir4jMUg"
    "http://suweb.mx/?utm_source=facebook&utm_medium=social&utm_campaign=lanzamiento_producto"
    "https://tuweb.com/producto?utm_source=instagram&utm_medium=social&utm_campaign=verano2025&utm_content=story",
    "http://www.affiliatexfiles.com/blog/?source=google&medium=organic"
    "https://www.amazon.co.jp/TOSHIBA-%E3%83%95%E3%83%A9%E3%83%83%E3%83%88%E3%83%86%E3%83%BC%E3%83%96%E3%83%AB-%E3%82%AA%E3%83%AC%E3%83%B3%E3%82%B8LED%E3%82%BF%E3%82%A4%E3%83%9E%E3%83%BC-ER-NS170A-%E3%83%98%E3%83%AB%E3%83%84%E3%83%95%E3%83%AA%E3%83%BC/dp/B0DPZBT572/?_encoding=UTF8&pd_rd_w=XIQqc&content-id=amzn1.sym.3e5fb751-95ea-4415-9e9c-3ab031dfb18e&pf_rd_p=3e5fb751-95ea-4415-9e9c-3ab031dfb18e&pf_rd_r=MN96K93MC8AZA67Q7NBT&pd_rd_wg=vwLOk&pd_rd_r=38b4ba6e-1b67-42b0-a9ba-9dc8ac197f22&ref_=pd_hp_d_atf_unk&th=1",
    "https://www.hostinet.com/formacion/marketing/mejores-programas-de-afiliados-para-ganar-un-extra-este-2023/?gad_source=5&gad_campaignid=23825122459",
    "http://blog.espacioharley.com/custom/estuvimos-en-riverside-9-2014-crazy-car-hop#utm_source=tumblr&utm_medium=link&utm_campaign=foto",
    "http://blog.espacioharley.com/custom/estuvimos-en-la-matinal-motera-de-riders-burger-grill#utm_source=tumblr&utm_medium=link&utm_campaign=foto",
    "https://nementio.com/normas-de-uso-de-las-redes-sociales/",
    "https://www.wix.com/my-account/site-selector/?buttonText=Administrar%20mi%20Blog&title=Selecciona%20un%20sitio&autoSelectOnSingleSite=true&actionUrl=https://www.wix.com/dashboard/%7B%7BmetaSiteId%7D%7D/blog",
    "http://creativecommons.org/licenses/by-nc-sa/3.0/es/"
)

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "linkubator-urls-probe"
if (Test-Path $tempRoot) {
    Remove-Item $tempRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $tempRoot | Out-Null

$projectFile = Join-Path $tempRoot "Linkubator.UrlProbe.csproj"
$programFile = Join-Path $tempRoot "Program.cs"

& dotnet new console --framework net10.0 --output $tempRoot --name "Linkubator.UrlProbe" --force | Out-Null
& dotnet add "$projectFile" reference "$domainProject" | Out-Null

$escapedUrls = foreach ($value in $urls) {
    '"' + ($value.Replace('"', '\"')) + '"'
}
$urlsLiteral = ($escapedUrls -join ",`n    ")

$programText = @"
using Linkubator.Domain.Policies;

var urls = new[]
{
    $urlsLiteral
};

Console.WriteLine($"SDK: {Environment.Version}");
Console.WriteLine($"URLs a probar: {urls.Length}");

for (int i = 0; i < urls.Length; i++)
{
    string url = urls[i];
    try
    {
        var adjusted = LinkUrlPolicy.Adjust(url);
        UrlValidationPolicy.Validate(adjusted);
        var normalized = UrlNormalizationPolicy.Normalize(adjusted);
        Console.WriteLine($"{i + 1}. {url} => AJUSTE={adjusted} | NORMALIZADA={normalized}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{i + 1}. {url} => EXCEPTION: {ex.GetType().Name}: {ex.Message}");
    }
}
"@

Set-Content -Path $programFile -Value $programText -Encoding UTF8

& dotnet run --project $projectFile --no-restore --verbosity minimal
$exitCode = $LASTEXITCODE

Remove-Item $tempRoot -Recurse -Force
exit $exitCode