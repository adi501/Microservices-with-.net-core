using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string emailCartQueue;
        private readonly string registerUserQueue;
        private readonly IConfiguration _configuration;

        private readonly string orderCreated_Topic;
        private readonly string orderUpdated_Topic_Subscription;
        private ServiceBusProcessor _emailOrderPlacedProcessor;

        private ServiceBusProcessor _emailCartProcesser;
        private ServiceBusProcessor _registerUserProcesser;

        private readonly EmailService _emailService;
        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;

          //  _emailCartProcesser = emailCartProcesser;


            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailshoppingcartQueue");
            registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");

            orderCreated_Topic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderUpdated_Topic_Subscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Email_Subscription");


            var cliet = new ServiceBusClient(serviceBusConnectionString);

            _emailCartProcesser = cliet.CreateProcessor(emailCartQueue);

            _registerUserProcesser = cliet.CreateProcessor(registerUserQueue);

            _emailOrderPlacedProcessor = cliet.CreateProcessor(orderCreated_Topic, orderUpdated_Topic_Subscription);

        }

        public async Task Start()
        {
            _emailCartProcesser.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcesser.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcesser.StartProcessingAsync();


            _registerUserProcesser.ProcessMessageAsync += OnUserRegisterRequestReceived;
            _registerUserProcesser.ProcessErrorAsync += ErrorHandler;
            await _registerUserProcesser.StartProcessingAsync();

            _emailOrderPlacedProcessor.ProcessMessageAsync += OnOrderPlacedRequestReceived;
            _emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailOrderPlacedProcessor.StartProcessingAsync();
        }

       

        public async Task Stop()
        {
            await _emailCartProcesser.StopProcessingAsync();
            await _emailCartProcesser.DisposeAsync();

            await _registerUserProcesser.StopProcessingAsync();
            await _registerUserProcesser.DisposeAsync();

            await _emailOrderPlacedProcessor.StopProcessingAsync();
            await _emailOrderPlacedProcessor.DisposeAsync();
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            CartDto objMessage=JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                //TODO - try to log email
               await _emailService.EmailCartAndLog(objMessage);
                await args.CompleteMessageAsync(args.Message);

            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            string email = JsonConvert.DeserializeObject<string>(body);
            try
            {
                //TODO - try to log email
                await _emailService.RegisterUserEmailAndLog(email);
                await args.CompleteMessageAsync(args.Message);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task OnOrderPlacedRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            RewardsMessage objRewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);
            try
            {
                //TODO - try to log email
                await _emailService.LogOrderPlaced(objRewardsMessage);
                await args.CompleteMessageAsync(args.Message);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
        private  Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        
    }
}
