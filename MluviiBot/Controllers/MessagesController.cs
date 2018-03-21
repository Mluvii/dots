﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using MluviiBot.BLL;
using MluviiBot.BotAssets.Extensions;
using MluviiBot.Properties;

namespace MluviiBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        ///     POST: api/Messages
        ///     Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                if (activity.Type == ActivityTypes.Message || activity.Type == ActivityTypes.Event)
                {
                    var dialog = scope.Resolve<IDialog<object>>();
                    await Conversation.SendAsync(activity, () => dialog);
                }
                else
                    await HandleSystemMessage(activity, scope);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
        }

        private async Task HandleSystemMessage(Activity message, ILifetimeScope scope)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

                // Note: Add introduction here:
                var memberAccount = message.MembersAdded.FirstOrDefault(o => o.Id == message.Recipient.Id);
                if (memberAccount != null)
                {
                    var crmService = scope.Resolve<ICrmService>();
                    var crmEntity = crmService.GetCrmData(memberAccount.Id);
                    var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
                    var reply = message.CreateReply();
                    reply.AddHeroCard(
                        crmEntity.Order.ProductName,
                        string.Format(Resources.WelcomeMessage_prompt, crmEntity.FullName, crmEntity.Order?.ProductName),
                        new[]
                        {
                            Resources.WelcomeMessage_operator,
                            Resources.MluviiDialog_virtual_assistant
                        },
                        new[] {crmEntity.Order.ProductPhotoUrl});
                    await client.Conversations.ReplyToActivityAsync(reply);
                }
                else if (message.Type == ActivityTypes.ContactRelationUpdate)
                {
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened
                }
                else if (message.Type == ActivityTypes.Typing)
                {
                    // Handle knowing tha the user is typing
                }
                else if (message.Type == ActivityTypes.Ping)
                {
                }
            }
        }
    }
}