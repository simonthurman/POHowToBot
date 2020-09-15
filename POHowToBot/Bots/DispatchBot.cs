// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using AdaptiveCards;

namespace POHowToBot.Bots
{
    public class DispatchBot<T> : ActivityHandler where T : Microsoft.Bot.Builder.Dialogs.Dialog
    {
        private readonly IBotService myBotServices;
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        protected readonly BotState UserState;

        public DispatchBot(IBotService botServices, ConversationState conversationState, UserState userState, T dialog)
        {
            myBotServices = botServices;

            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        //*********************************************

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
                case "POHowTo":
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
            var options = new QnAMakerOptions { Top = 1 };
            var response = await myBotServices.POHowToQnA.GetAnswersAsync(turnContext, options);

            if (response != null && response.Length > 0)
            {
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Please contact Fred for more help"), cancellationToken);
            }

        }

        private async Task ProcessPOEnquiry(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("Use Power BI"), cancellationToken);
        }
    }
}
