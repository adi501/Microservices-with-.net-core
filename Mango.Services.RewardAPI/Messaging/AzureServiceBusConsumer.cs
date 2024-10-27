using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string orderCreatedTopic;
        private readonly string orderCreatedRewardsSubscription;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _rewardProcesser;

        private readonly RewardService _rewardService;
        public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _rewardService = rewardService;
            _configuration = configuration;

          //  _emailCartProcesser = emailCartProcesser;


            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedRewardsSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

            var cliet = new ServiceBusClient(serviceBusConnectionString);

            _rewardProcesser = cliet.CreateProcessor(orderCreatedTopic,orderCreatedRewardsSubscription);
        }

        public async Task Start()
        {
            _rewardProcesser.ProcessMessageAsync += OnNewOrderRewardRequestReceived;
            _rewardProcesser.ProcessErrorAsync += ErrorHandler;
            await _rewardProcesser.StartProcessingAsync();

        }

       

        public async Task Stop()
        {
            await _rewardProcesser.StopProcessingAsync();
            await _rewardProcesser.DisposeAsync();

        }

        private async Task OnNewOrderRewardRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where you will receive message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            RewardsMessage objMessage =JsonConvert.DeserializeObject<RewardsMessage>(body);
            try
            {
                //TODO - try to log email
               await _rewardService.UpdateRewards(objMessage);
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
