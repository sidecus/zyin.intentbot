namespace sample
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Zyin.IntentBot;
    using Zyin.IntentBot.Bot;
    using Zyin.IntentBot.Config;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Services;
    using sample.Areas;
    using sample.Bot;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Add IOptions for required config objects.
            // These are optional configs. For local devevelopment they are all empty.
            services.Configure<BotConfig>(Configuration);
            services.Configure<LuisConfig>(Configuration);
            services.Configure<OAuthConfig>(Configuration);

            // Configure http client for graph service
            services.AddHttpClient<GraphService>(client =>
            {
                client.BaseAddress = new Uri(GraphService.BaseUri);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            
            // Add intent service
            services.AddSingleton<IIntentService, SampleIntentService>();

            // Add intent bot and sample user input
            services.AddIntentBot<SampleBot>();
            services.AddSingleton<UserStateManager<SampleUserInfo>>();

            // Register intents
            services.AddSimpleIntent<FallbackContext, SampleFallbackHandler>();
            services.AddSimpleIntent<GreetingsContext, GreetingsIntentHandler>();
            services.AddSimpleIntent<WhoAmIContext, WhoAmIIntentHandler>();
            services.AddUserInputIntent<AddNumberIntentContext, AddNumberIntentHandler>();
            services.AddSimpleIntent<MemorizedSumResultIntentContext, MemorizedSumResultIntentHandler>();
            services.AddUserInputIntent<BookFlightIntentContext, BookFlightIntentHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
