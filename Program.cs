var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5294);
});

var app = builder.Build();

app.Use(async (context, next) => {
    if(context.Request.Query["secureConnection"] != "true") {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("No Https connection allowed. Please use a secure connection.");
        return; 
    } else {
        await next();
    }
});

app.Use(async (context, next) => {
    var input = context.Request.Query["input"];
    if (!isValidInput(input)) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid input. Please provide a valid input.");
        return;
    } else {
        await next();
    }
   
});

app.Use(async (context, next) => {
  if(context.Request.Path == "unauthorized") {
      context.Response.StatusCode = 401;
      await context.Response.WriteAsync("Unauthorized access.");
      return;
  } else {
      await next();
  }
});

app.Use(async (context, next) => {
    if (context.Request.Query["authentication"] != "true") {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Forbidden access. Authentication required.");
        return;
    } 
    context.Response.Cookies.Append("auth", "valid_token", new CookieOptions
    {
        HttpOnly = true,
        Secure = true
    });
    await next();
});

static bool isValidInput(string input)
{
    return !string.IsNullOrEmpty(input) || (input.All(char.isLetterOrDigit) && !input.Contains("<script>"));
}