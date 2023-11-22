using System.Net;
using MassiveUDFMaker.Code;
using MassiveUDFMaker.Resource;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;

namespace MassiveUDFMaker;

public class Domain
{
    private ApplicationSettings _applicationSettings;
    private RestClient _httpClient;
    private string _b1Session;
    private string _routeSession;

    private Domain(ApplicationSettings? applicationSettings)
    {
        _applicationSettings = applicationSettings ?? throw new Exception(Strings.Error.APPLICATION_SETTINGS_NOT_FOUND);
    }
    
    public void Run()
    {
        Log.Logger.Information("Inicializando proceso");
        initialize();
        Log.Logger.Information("Ingresando a SAP B1");
        login();
        Log.Logger.Information("Creando UDFs");
        create_fields();
    }

    private void initialize()
    {
        _httpClient = make_http_client();
    }

    private RestClient make_http_client()
    {
        var baseUrl = new Uri(string.Format(Strings.Http.BASE_URI, _applicationSettings.ServiceLayer.API, _applicationSettings.ServiceLayer.Port));
        var options = new RestClientOptions(baseUrl)
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
            ThrowOnAnyError = false,
            MaxTimeout = int.MaxValue
        };

        return new RestClient(options, configureSerialization: serialization => serialization.UseNewtonsoftJson(),
            configureDefaultHeaders: headers => headers.Add(@"Content-Type", ContentType.Json));
    }

    private void login()
    {
        string urlLogin = string.Format(SAPUrlAction.URL_LOGIN, _applicationSettings.ServiceLayer.Version);
        Log.Logger.Information($"POST {_httpClient.Options.BaseUrl}/{urlLogin}");
        
        var restRequest = new RestRequest(urlLogin, Method.Post);
        restRequest.AddJsonBody(_applicationSettings.SAPCredentials);
        RestResponse restResponse = _httpClient.Execute(restRequest);

        HttpStatusCode responseCode = restResponse.StatusCode;
        if (responseCode != HttpStatusCode.OK)
            throw new Exception(restResponse.ErrorMessage);
        
        List<HeaderParameter>? parameters = restResponse.Headers
            ?.Where(item => item.Name == Strings.Cookie.HEADER)
            .ToList();

        if (parameters == null || parameters.Count == default)
            throw new Exception(Strings.Error.HEADER_NOT_FOUND);
        
        _b1Session = retrieve_parameter(parameters, Strings.Cookie.B1_SESSION);
        _routeSession = retrieve_parameter(parameters, Strings.Cookie.ROUTE_ID);
    }

    private string retrieve_parameter(IEnumerable<HeaderParameter> parameters, string tagName)
    {
        var parameterValue = (parameters
                .Single(t => (t.Value?.ToString() ?? string.Empty).Contains(tagName))
                .Value ?? string.Empty)
            .ToString();
        return extract_value_from_cookie(parameterValue, tagName);
    }

    private static string extract_value_from_cookie(string cookie, string tagName)
    {
        var result = string.Empty;
        for (var index = 0; index < cookie.Length; index++)
        {
            char headerLetter = cookie[index];
            if (headerLetter != tagName[0] || cookie.Substring(index, tagName.Length) != tagName)
                continue;

            for (int i = index + tagName.Length + 1; i < cookie.Length; i++)
            {
                if (cookie[i] == ';')
                    return result;

                result += cookie[i];
            }
        }

        return result;
    }
    
    private void create_fields()
    {
        string urlUserFields = string.Format(SAPUrlAction.URL_USER_FIELD, _applicationSettings.ServiceLayer.Version);
        Log.Logger.Information($"POST {_httpClient.Options.BaseUrl}/{urlUserFields}");
        var restRequest = new RestRequest(urlUserFields, Method.Post);
        restRequest.AddCookie(Strings.Cookie.B1_SESSION, _b1Session, Strings.Cookie.BASE_PATH, _httpClient.Options.BaseUrl?.Host ?? string.Empty);
        restRequest.AddCookie(Strings.Cookie.ROUTE_ID, _routeSession, Strings.Cookie.BASE_PATH, _httpClient.Options.BaseUrl?.Host ?? string.Empty);
        foreach (UserDefinedField userDefinedField in _applicationSettings.UserDefinedFields)
        {
            restRequest.AddJsonBody(userDefinedField);
            RestResponse restResponse = _httpClient.Execute(restRequest);
            if (restResponse.StatusCode != HttpStatusCode.OK && restResponse.StatusCode != HttpStatusCode.Created)
                throw new Exception(restResponse.ErrorMessage);
            var x =restResponse.Content;
        }
    }

    public static Domain CreateDomain(ApplicationSettings? applicationSettings) 
        => new Domain(applicationSettings);
}