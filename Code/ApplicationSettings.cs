namespace MassiveUDFMaker.Code;

public record ApplicationSettings(ServiceLayer ServiceLayer, SAPCredentials SAPCredentials, IEnumerable<UserDefinedField> UserDefinedFields);

public record ServiceLayer(string API, int Port, int Version);

public record SAPCredentials(string CompanyDB, string UserName, string Password);

public record UserDefinedField(string Name, string Description, string Type, string SubType, string TableName);