// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using AdaptiveCards;

namespace POHowToBot.Bots
{
    public class DispatchBot : ActivityHandler
    {
        private readonly IBotService myBotServices;

        public DispatchBot(IBotService botServices)
        {
            myBotServices = botServices;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //use dispatch model to figure our which service
            var recogniserResult = await myBotServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);

            //determine top intent 
            var topIntent = recogniserResult.GetTopScoringIntent();

            //call dispatcher 
            await DispatchtoTopIntentAsync(turnContext, topIntent.intent, recogniserResult, cancellationToken);

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to PO Bot!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
        private async Task DispatchtoTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "q_POKB":
                    await AnswerPOQnA(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                    break;
                case "PO_Enquiry":
                    await ProcessPOEnquiry(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                    break;
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognised intent: {intent}."), cancellationToken);
                    break;
            }
        }

        private async Task AnswerPOQnA(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            var card = new AdaptiveCards.AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            //var pictureUrl = "";

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Purchase Order Help"
            });

            //card.BackgroundImage.Url.LocalPath

            card.Body.Add(new AdaptiveImage()
            {
                Type = "Image",
                //Url = $"data:image/png;base64,{robot.jpg}"
                Size = AdaptiveImageSize.Medium
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Thank you, hope this helps...",
                Color = AdaptiveTextColor.Accent
            });

            var options = new QnAMakerOptions { Top = 1 };
            var response = await myBotServices.POHowToQnA.GetAnswersAsync(turnContext, options);

            if (response != null && response.Length > 0)
            {

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = response[0].Answer,
                    Color = AdaptiveTextColor.Accent
                });

                Attachment attach = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };

                var msg = MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                });

                await turnContext.SendActivityAsync(msg, cancellationToken);

            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("No anwers found from KB"), cancellationToken);
            }

            //Attachment attach = new Attachment()
            //{
            //    ContentType = AdaptiveCard.ContentType,
            //    Content = card
            //};

            //var options = new QnAMakerOptions { Top = 1 };
            //var response = await myBotServices.POHowToQnA.GetAnswersAsync(turnContext, options);

            //if (response != null && response.Length > 0)
            //{
            //    await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
            //}
            //else
            //{
            //    await turnContext.SendActivityAsync(MessageFactory.Text("No anwers found from KB"), cancellationToken);
            //}
        }

        private async Task ProcessPOEnquiry(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Use Power BI"), cancellationToken);
        }
    }
}
