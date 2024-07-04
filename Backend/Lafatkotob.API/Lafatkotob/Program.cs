using Lafatkotob;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lafatkotob.Services.EventService;
using Lafatkotob.Services.BookService;
using Lafatkotob.Services.BadgeService;
using Lafatkotob.Services.HistoryService;
using Lafatkotob.Services.BookGenreService;
using Lafatkotob.Services.BookPostCommentService;
using Lafatkotob.Services.BookPostLikeServices;
using Lafatkotob.Services.BooksInWishlistsService;
using Lafatkotob.Services.GenreService;
using Lafatkotob.Services.ConversationService;
using Lafatkotob.Services.ConversationsUserService;
using Lafatkotob.Services.TokenService;
using Lafatkotob.Services.UserLikeService;
using Lafatkotob.Services.UserEventService;
using Lafatkotob.Services.WishedBookService;
using Lafatkotob.Services.WishListService;
using Lafatkotob.Services.AppUserService;
using Lafatkotob.Services.MessageService;
using Lafatkotob.Services.NotificationService;
using Lafatkotob.Services.EmailService;
using Lafatkotob.Services.UserPreferenceService;
using Lafatkotob.Services.NotificationUserService;
using Lafatkotob.Services.UserBadgeService;
using Lafatkotob.Services.UserReviewService;
using Lafatkotob.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Lafatkotob.Initialization;
using AspNetCoreRateLimit;
using Lafatkotob.Swagger;
using Lafatkotob.Hubs;
using Lafatkotob.Services.UserVoteService;
using Lafatkotob.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// Configuration for IP rate limiting
builder.Services.AddMemoryCache();

// Load IP rate limiting settings from appsettings.json
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

// Add IP rate limiting service
builder.Services.AddInMemoryRateLimiting();

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

builder.Services
    .AddSignalR(hubOptions =>
    {
        hubOptions.EnableDetailedErrors = true;
    })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IBookGenreServices, BookGenreServices>();
builder.Services.AddScoped<IBookPostCommentServices, BookPostCommentServices>();
builder.Services.AddScoped<IBookPostLikeService, BookPostLikeService>();
builder.Services.AddScoped<IBooksInWishlistsService, BooksInWishlistsService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IConversationsUserService, ConversationsUserService>();
builder.Services.AddScoped<ITokenSerive, TokenService>();
builder.Services.AddScoped<IUserLikeService, UserLikeService>();
builder.Services.AddScoped<IUserEventService, UserEventService>();
builder.Services.AddScoped<IWishedBookService, WishedBookService>();
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
builder.Services.AddScoped<INotificationUserService, NotificationUserService>();
builder.Services.AddScoped<IUserBadgeService, UserBadgeService>();
builder.Services.AddScoped<IUserReviewService, UserReviewService>();
builder.Services.AddScoped<IUserVoteService, UserVoteService>();
builder.Services.AddSingleton<IUserConnectionsService, UserConnectionsService>();
builder.Services.AddSingleton<IChatHub, ChatHub>();

builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(2),
                errorNumbersToAdd: null);
        })
    );

//configureing swagger to use token auth
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    // Define the OAuth2.0 Bearer Scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    c.OperationFilter<FileUploadOperation>();
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // Handling the token received in query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["Authorization"].ToString();

                // If the request is for our SignalR hub...
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat")) // Replace with your SignalR hub's path
                {
                    accessToken = accessToken.Replace("Bearer ", "");

                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Log the exception
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200");
}));


var app = builder.Build();

app.UseCors("CorsPolicy");

using (var scope = app.Services.CreateScope())
{
    await Rolesinitialization.SeedRoles(scope.ServiceProvider);
    await Badgeinitialization.SeedBadges(scope.ServiceProvider);
    await GenresInitialization.SeedGenres(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseStaticFiles();

app.UseIpRateLimiting();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/chat");

app.MapControllers();

app.Run();