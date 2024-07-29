using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using blazorcognito;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("AuthWebAPI", httpClient => {
    httpClient.BaseAddress = new Uri(builder.Configuration["BaseUrl"]);
    //httpClient.DefaultRequestHeaders.Add("accountId", builder.Configuration["AccountId"]);
}).AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    .ConfigureHandler(
        authorizedUrls: new [] { builder.Configuration["BaseUrl"] },
        scopes: new[] { "read", "write" }));

builder.Services.AddHttpClient("WebAPI", httpClient => {
    httpClient.BaseAddress = new Uri(builder.Configuration["BaseUrl"]);
    //httpClient.DefaultRequestHeaders.Add("accountId", builder.Configuration["AccountId"]);
});

builder.Services.AddOidcAuthentication(options => {
    options.ProviderOptions.DefaultScopes.Add("aws.cognito.signin.user.admin");
    options.UserOptions.NameClaim = "given_name";
    options.UserOptions.RoleClaim = "cognito:groups";
    builder.Configuration.Bind("Cognito", options.ProviderOptions);
    options.ProviderOptions.AdditionalProviderParameters.Add("client_id", builder.Configuration["Cognito:ClientId"]);
    options.ProviderOptions.AdditionalProviderParameters.Add("redirect_uri", $"{builder.HostEnvironment.BaseAddress}authentication/logout-callback");
    options.ProviderOptions.AdditionalProviderParameters.Add("response_type", "code");
}).AddAccountClaimsPrincipalFactory<MultipleRoleClaimsPrincipalFactory<RemoteUserAccount>>();

await builder.Build().RunAsync();
