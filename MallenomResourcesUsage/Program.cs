using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.WebHost.UseKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7230, listenOptions => listenOptions.UseHttps());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Swagger MallenomResourcesUsage Documentation",
            Version = "v1",
            Description = "This application is allows to see your computer resources usage, such as cpu loading, memory usage and free space in hard disks." +
            " The software can be start on windows and linux based machines.",
            Contact = new OpenApiContact
            {
                Name = "Konstantin Filyurin",
                Email = "kfilyurin@gmail.com"
            }
        });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSwagger();

app.UseSwaggerUI(options =>
options.SwaggerEndpoint("/swagger/v1/swagger.json",
"Swagger MallenomResourcesUsage Documentation v1"));

app.UseReDoc(options =>
{
    options.DocumentTitle = "Swagger Demo Documentation";
    options.SpecUrl = "/swagger/v1/swagger.json";
});


app.Run();
